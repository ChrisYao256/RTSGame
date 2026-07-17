using Godot;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.Threading;
using static Godot.TextServer;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RTSGame.Source;

public partial class Grid : TileMapLayer
{
	public static Vector2 GetRandomOffset()
	{
		Random random = new Random();
		float x = random.Next(-40, 40);
		float y = random.Next(-30, 40);
		return new Vector2(x, y);
	}

	public static Vector2 ClampOffset(Vector2 offset)
	{
		offset.X = Math.Clamp(offset.X, -40, 40);
		offset.Y = Math.Clamp(offset.Y, -30, 40);
		return offset;
	}

	private static readonly Vector2I[] Directions = new Vector2I[]
		{
				new Vector2I(0, -1), // Up
        new Vector2I(1, 0),  // Right
        new Vector2I(0, 1),  // Down
        new Vector2I(-1, 0)  // Left
    };


	private AStarGrid2D _astar = new AStarGrid2D();

	private Dictionary<Vector2I, TowerUnit> _occupiedCells = new Dictionary<Vector2I, TowerUnit>();

	private TileMapLayer _overlayLayer;


	public override void _Ready()
	{
		SetupNavigation();
		_overlayLayer = GetParent().GetNode<TileMapLayer>("OverlayLayer");
	}

	public Vector2 GetSnappedPosition(Vector2 worldPosition)
	{
		Vector2I gridPos = LocalToMap(worldPosition);
		return MapToLocal(gridPos);
	}

	public bool CanBuildAt(Vector2I gridPos)
	{

		if (!IsCellVacant(gridPos))
		{
			return false;
		}

		// 1. Check TileSet data (is it grass/dirt?)
		TileData data = GetCellTileData(gridPos);
		if (data != null)
		{
			// Access the custom data we set up in the editor
			return(bool)data.GetCustomData("Buildable");
		}
		return false;
	}

	public bool IsPath(Vector2I gridPos)
	{
		// 1. Check TileSet data (is it grass/dirt?)
		TileData data = GetCellTileData(gridPos);
		if (data != null)
		{
			// Access the custom data we set up in the editor
			return (bool)data.GetCustomData("Path");
		}
		return false;
	}

	public Vector2I FindClosestBuildableCell(Vector2I startCell, bool mustBePathAdjacent, int maxRadius = 10)
	{
		// 1. If the starting cell is already matching, return it immediately
		if (CanBuildAt(startCell))
		{
			if (!mustBePathAdjacent)
			{
				return startCell;
			}
			foreach (Vector2I dir_ in Directions)
			{
				Vector2I neighbor_ = startCell + dir_;
				if (IsPath(neighbor_))
				{
					return startCell;
				}
			}
		}

		// 2. Track visited cells so we don't look at the same tile twice (avoids infinite loops)
		HashSet<Vector2I> visited = new HashSet<Vector2I>();

		// 3. The queue stores cells we need to check, processing them in First-In, First-Out order
		Queue<Vector2I> queue = new Queue<Vector2I>();

		queue.Enqueue(startCell);
		visited.Add(startCell);

		while (queue.Count > 0)
		{
			Vector2I current = queue.Dequeue();

			// Early exit safeguard: Stop searching if we're drifting too far away
			if (Mathf.Abs(current.X - startCell.X) > maxRadius || Mathf.Abs(current.Y - startCell.Y) > maxRadius)
				continue;

			// Check all valid neighbors
			foreach (Vector2I dir in Directions)
			{
				Vector2I neighbor = current + dir;

				if (!visited.Contains(neighbor))
				{
					visited.Add(neighbor);

					// If this neighbor meets our criteria, it is guaranteed to be the closest!
					if (!mustBePathAdjacent && CanBuildAt(neighbor))
					{
						return neighbor;
					}
					else if (mustBePathAdjacent && CanBuildAt(neighbor))
					{
						foreach (Vector2I dir_ in Directions)
						{
							Vector2I neighbor_ = neighbor + dir_;
							if (IsPath(neighbor_))
							{
								return neighbor;
							}
						}
					}

					// Otherwise, queue it up so we can search its neighbors in the next layer
					queue.Enqueue(neighbor);
				}
			}
		}

		return new Vector2I(999,999);
	}

	public Vector2 MapToGlobal(Vector2I mapPosition)
	{
		Vector2 local = MapToLocal(mapPosition);
		return ToGlobal(local);
	}

	public Rect2 GetGlobalTileRect(Vector2I mapPosition)
	{
		float leftX = MapToGlobal(mapPosition).X - TileSet.TileSize.X / 2 * Scale.X;
		float topY = MapToGlobal(mapPosition).Y - TileSet.TileSize.Y / 2 * Scale.Y;
		return new Rect2(leftX, topY, TileSet.TileSize.X * Scale.X, TileSet.TileSize.Y * Scale.Y);
	}

	public Rect2 GetGlobalTileRect(Vector2 globalPosition)
	{
		Vector2I mapPosition = LocalToMap(ToLocal(globalPosition));
		float leftX = MapToGlobal(mapPosition).X - TileSet.TileSize.X / 2 * Scale.X;
		float topY = MapToGlobal(mapPosition).Y - TileSet.TileSize.Y / 2 * Scale.Y;
		return new Rect2(leftX, topY, TileSet.TileSize.X * Scale.X, TileSet.TileSize.Y * Scale.Y);
	}

	public Vector2 GetEntrancePosition()
	{
		foreach (Vector2I cell in GetUsedCells())
		{
			TileData data = GetCellTileData(cell);
			if (data != null && (bool)data.GetCustomData("Entrance"))
			{
				return ToGlobal(MapToLocal(cell));
			}
		}
		GD.PrintErr("No spawn tile found!");
		return Vector2.Zero;
	}

	public Vector2 GetExitLocation()
	{
		foreach (Vector2I cell in GetUsedCells())
		{
			TileData data = GetCellTileData(cell);
			if (data != null && (bool)data.GetCustomData("Exit"))
			{
				return ToGlobal(MapToLocal(cell));
			}
		}
		GD.PrintErr("No spawn tile found!");
		return Vector2.Zero;
	}

	private void SetupNavigation()
	{
		// 1. Define the grid boundaries and cell size
		_astar.Region = GetUsedRect();
		_astar.CellSize = TileSet.TileSize;
		_astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		_astar.Update();

		// 2. Loop through every tile in the grid
		foreach (Vector2I cell in GetUsedCells())
		{
			TileData data = GetCellTileData(cell);

			// 3. If it's NOT a path, mark it as solid/unnavigable
			if (data == null || !(bool)data.GetCustomData("Path"))
			{
				_astar.SetPointSolid(cell, true);
			}
		}
	}

	public List<Vector2> GetPath(Vector2 startWorld, Vector2 endWorld)
	{
		Vector2I startMap = LocalToMap(ToLocal(startWorld));
		TileData data = GetCellTileData(startMap);
		bool startedFromPath = (bool)data.GetCustomData("Path");
		if (!startedFromPath)
		{
			_astar.SetPointSolid(startMap, false);
		}

		Vector2I endMap = LocalToMap(ToLocal(endWorld));

		// This returns a list of world positions for the enemy to follow
		Godot.Collections.Array<Vector2I> path = _astar.GetIdPath(startMap, endMap);
		List<Vector2> waypoints = [];
		foreach (Vector2I cell in path)
		{
			waypoints.Add(ToGlobal(MapToLocal(cell)));
		}
		if (!startedFromPath)
		{
			_astar.SetPointSolid(startMap, true);
		}
		return waypoints;
	}

	public List<Vector2> GetPath(Vector2I startMap, Vector2 endWorld)
	{
		TileData data = GetCellTileData(startMap);
		bool startedFromPath = (bool)data.GetCustomData("Path");
		if (!startedFromPath)
		{
			_astar.SetPointSolid(startMap, false);
		}
		Vector2I endMap = LocalToMap(ToLocal(endWorld));

		// This returns a list of world positions for the enemy to follow
		Godot.Collections.Array<Vector2I> path = _astar.GetIdPath(startMap, endMap);
		List<Vector2> waypoints = [];
		foreach (Vector2I cell in path)
		{
			waypoints.Add(ToGlobal(MapToLocal(cell)));
		}
		if (!startedFromPath)
		{
			_astar.SetPointSolid(startMap, true);
		}
		return waypoints;
	}

	public void OccupyCell(Vector2I cell, TowerUnit tower)
	{
		_occupiedCells.Add(cell, tower);
	}

	public void UnoccupyCell(Vector2I cell)
	{
		_occupiedCells.Remove(cell);
	}

	public bool IsCellVacant(Vector2I cell)
	{
		foreach (Vector2I occupiedCell in _occupiedCells.Keys)
		{
			if (occupiedCell == cell) return false;
		}
		return true;
	}

	public TowerUnit GetTowerOnCell(Vector2I cell)
	{
		if (IsCellVacant(cell))
		{
			throw new Exception("No tower at given cell");
		}
		return _occupiedCells[cell];
	}

	public void DrawVisualTiles(List<Vector2I> localCoords, Vector2I unitGridPos)
	{
		// 1. Clear previous visuals
		_overlayLayer.Clear();

		// 2. The ID of the visual tile in your TileSet
		int sourceId = 1;
		Vector2I atlasCoords = new Vector2I(0, 0); // The "green glow" tile, for example

		foreach (Vector2I offset in localCoords)
		{
			// Calculate the absolute position on the map
			Vector2I targetTile = unitGridPos + offset;

			// Set the cell. It will automatically match the TileSet size.
			_overlayLayer.SetCell(targetTile, sourceId, atlasCoords);
		}
	}

	public void HideVisualTiles()
	{
		_overlayLayer.Clear();
	}
}
