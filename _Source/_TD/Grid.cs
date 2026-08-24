using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Collections.Generic;
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


	private AStar2D _astar = new AStar2D();

	private Godot.Collections.Dictionary<Vector2I, TowerUnit> _occupiedCells = new Godot.Collections.Dictionary<Vector2I, TowerUnit>();

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

	public Vector2I FindClosestBuildableCell(Vector2I startCell, bool mustBeNextToPath, int maxRadius = 10)
	{
		// 1. If the starting cell is already matching, return it immediately
		if (CanBuildAt(startCell))
		{
			if (!mustBeNextToPath)
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
					if (!mustBeNextToPath && CanBuildAt(neighbor))
					{
						return neighbor;
					}
					else if (mustBeNextToPath && CanBuildAt(neighbor))
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

	/// <summary>
	/// Makes the tile passable for pathfinding. Doesn't actually make the tile a Path. 
	/// </summary>
	/// <param name="gridPos"></param>
	/// <param name="walkable"></param>
	public void MakeTileWalkable(Vector2I gridPos, bool walkable = true, bool bidirectinoal = true)
	{
		TileData data = GetCellTileData(gridPos);
		long currentId = GetIdForCell(gridPos);

		if (data != null)
		{
			SetPointSolid(gridPos, !walkable);
		}
		System.Collections.Generic.Dictionary<string, bool> connectedDirections = new System.Collections.Generic.Dictionary<string, bool>()
		{
			{"Up", false },
			{"Down", false },
			{"Left", false },
			{"Right", false },
		};
		foreach (Vector2I dir in Directions)
		{
			Vector2I neighbor = gridPos + dir;
			long neighborId = GetIdForCell(neighbor);

			// Skip if the neighbor doesn't exist in our graph at all
			if (!_astar.HasPoint(neighborId)) continue;

			if (!walkable)
			{
				// If turning unwalkable, sever the connection
				_astar.DisconnectPoints(currentId, neighborId);

				TileData neighborData = GetCellTileData(neighbor);
				Vector2I newNeighborAtlas;
				if (dir == new Vector2I(1, 0))
				{
					newNeighborAtlas = GetAtlasCoordsForPath((bool)neighborData.GetCustomData("PathUp"), (bool)neighborData.GetCustomData("PathDown"), false, (bool)neighborData.GetCustomData("PathRight"));
				}
				else if (dir == new Vector2I(-1, 0))
				{
					newNeighborAtlas = GetAtlasCoordsForPath((bool)neighborData.GetCustomData("PathUp"), (bool)neighborData.GetCustomData("PathDown"), (bool)neighborData.GetCustomData("PathLeft"), false);
				}
				else if (dir == new Vector2I(0, 1))
				{
					newNeighborAtlas = GetAtlasCoordsForPath(false, (bool)neighborData.GetCustomData("PathDown"), (bool)neighborData.GetCustomData("PathLeft"), (bool)neighborData.GetCustomData("PathRight"));
				}
				else
				{
					newNeighborAtlas = GetAtlasCoordsForPath((bool)neighborData.GetCustomData("PathUp"), false, (bool)neighborData.GetCustomData("PathLeft"), (bool)neighborData.GetCustomData("PathRight"));
				}
				
				SetCell(neighbor, 0, atlasCoords: newNeighborAtlas);
			}
			else
			{
				TileData neighborData = GetCellTileData(neighbor);

				if (neighborData != null && (bool)neighborData.GetCustomData("Path"))
				{
					Vector2I newNeighborAtlas;
					if (dir == new Vector2I(1, 0))
					{
						connectedDirections["Right"] = true;
						newNeighborAtlas = GetAtlasCoordsForPath((bool)neighborData.GetCustomData("PathUp"), (bool)neighborData.GetCustomData("PathDown"), true, (bool)neighborData.GetCustomData("PathRight"));
					}
					else if (dir == new Vector2I(-1, 0))
					{
						connectedDirections["Left"] = true;
						newNeighborAtlas = GetAtlasCoordsForPath((bool)neighborData.GetCustomData("PathUp"), (bool)neighborData.GetCustomData("PathDown"), (bool)neighborData.GetCustomData("PathLeft"), true);
					}
					else if (dir == new Vector2I(0, 1))
					{
						connectedDirections["Down"] = true;
						newNeighborAtlas = GetAtlasCoordsForPath(true, (bool)neighborData.GetCustomData("PathDown"), (bool)neighborData.GetCustomData("PathLeft"), (bool)neighborData.GetCustomData("PathRight"));
					}
					else
					{
						connectedDirections["Up"] = true;
						newNeighborAtlas = GetAtlasCoordsForPath((bool)neighborData.GetCustomData("PathUp"), true, (bool)neighborData.GetCustomData("PathLeft"), (bool)neighborData.GetCustomData("PathRight"));
					}
					SetCell(neighbor, 0, atlasCoords: newNeighborAtlas);
					_astar.ConnectPoints(currentId, neighborId, bidirectinoal);
				}
			}
		}
		if (!walkable)
		{
			SetCell(gridPos, 0, atlasCoords: GetAtlasCoordsForPath(false, false, false, false));
		}
		else
		{
			SetCell(gridPos, 0, atlasCoords: GetAtlasCoordsForPath(connectedDirections["Up"], connectedDirections["Down"], connectedDirections["Left"], connectedDirections["Right"]), alternativeTile: 1);
		}
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

	public Vector2 GetDefaultEntrancePosition()
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

	public Vector2 GetDefaultExitPosition()
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

	public Vector2I GetDefaultEntranceMapPosition()
	{
		foreach (Vector2I cell in GetUsedCells())
		{
			TileData data = GetCellTileData(cell);
			if (data != null && (bool)data.GetCustomData("Entrance"))
			{
				return cell;
			}
		}
		GD.PrintErr("No spawn tile found!");
		return Vector2I.Zero;
	}

	public Vector2I GetDefaultExitMapPosition()
	{
		foreach (Vector2I cell in GetUsedCells())
		{
			TileData data = GetCellTileData(cell);
			if (data != null && (bool)data.GetCustomData("Exit"))
			{
				return cell;
			}
		}
		GD.PrintErr("No spawn tile found!");
		return Vector2I.Zero;
	}

	private void SetupNavigation()
	{
		// 1. Add all points to the graph
		foreach (Vector2I cell in GetUsedCells())
		{
			long id = GetIdForCell(cell);

			// AStar2D needs a Vector2 position to calculate pathfinding heuristics (distance).
			// Using the grid coordinates works perfectly for this.
			_astar.AddPoint(id, new Vector2(cell.X, cell.Y));

			TileData data = GetCellTileData(cell);
			bool isPath = data != null && (bool)data.GetCustomData("Path");

			// 2. Disable the point if it's not a path. 
			// (It still exists in the graph, so your portal spawning code can temporarily enable it later)
			if (!isPath)
			{
				_astar.SetPointDisabled(id, true);
			}
		}

		Vector2I[] directions = { Vector2I.Right, Vector2I.Down };

		foreach (Vector2I cell in GetUsedCells())
		{
			long currentId = GetIdForCell(cell);
			TileData currentData = GetCellTileData(cell);
			if (currentData == null) continue;

			foreach (Vector2I dir in directions)
			{
				Vector2I neighbor = cell + dir;
				long neighborId = GetIdForCell(neighbor);

				// Only proceed if the neighbor actually exists in our graph
				if (_astar.HasPoint(neighborId))
				{
					TileData neighborData = GetCellTileData(neighbor);
					if (neighborData == null) continue;

					bool isBlocked = false;

					// Check Right connection
					if (dir == Vector2I.Right)
					{
						bool myRightPassable = (bool)currentData.GetCustomData("PathRight");
						bool neighborLeftPassable = (bool)neighborData.GetCustomData("PathLeft");

						if (!myRightPassable || !neighborLeftPassable)
							isBlocked = true;
					}
					// Check Down connection
					else if (dir == Vector2I.Down)
					{
						bool myDownPassable = (bool)currentData.GetCustomData("PathDown");
						bool neighborUpPassable = (bool)neighborData.GetCustomData("PathUp");

						if (!myDownPassable || !neighborUpPassable)
							isBlocked = true;
					}

					// If neither tile blocks the connection, draw the edge
					if (!isBlocked)
					{
						_astar.ConnectPoints(currentId, neighborId);
					}
				}
			}
		}
	}

	public List<Vector2> GetPath(Vector2 startWorld, Vector2 endWorld)
	{
		Vector2I startMap = LocalToMap(ToLocal(startWorld));
		Vector2I endMap = LocalToMap(ToLocal(endWorld)); // Moved up for cleaner ID generation

		TileData data = GetCellTileData(startMap);

		// Good practice to null-check TileData in case the start is out of bounds
		bool startedFromPath = data != null && (bool)data.GetCustomData("Path");

		long startId = GetIdForCell(startMap);
		long endId = GetIdForCell(endMap);

		if (!startedFromPath)
		{
			// AStar2D uses "Disabled" instead of "Solid"
			_astar.SetPointDisabled(startId, false);
		}

		// GetIdPath in AStar2D returns a long[] instead of a Godot Array of Vector2I
		long[] pathIds = _astar.GetIdPath(startId, endId);
		List<Vector2> waypoints = new List<Vector2>();

		foreach (long id in pathIds)
		{
			// Convert the ID back to map coordinates
			Vector2I cell = GetCellForId(id);
			waypoints.Add(ToGlobal(MapToLocal(cell)));
		}

		if (!startedFromPath)
		{
			_astar.SetPointDisabled(startId, true);
		}

		return waypoints;
	}

	// --- HELPER METHODS ---

	// Packs a Vector2I (X and Y) into a single unique 64-bit integer
	private long GetIdForCell(Vector2I cell)
	{
		return (long)((ulong)cell.X << 32 | (uint)cell.Y);
	}

	// Unpacks the 64-bit integer back into a Vector2I
	private Vector2I GetCellForId(long id)
	{
		return new Vector2I((int)(id >> 32), (int)id);
	}
	
	private void SetPointSolid(Vector2I cell, bool disabled = true)
	{
		_astar.SetPointDisabled(GetIdForCell(cell), disabled);
	}

	public List<Vector2> GetPath(Vector2I startMap, Vector2 endWorld)
	{
		TileData data = GetCellTileData(startMap);
		bool startedFromPath = (bool)data.GetCustomData("Path");
		if (!startedFromPath)
		{
			SetPointSolid(startMap);
		}
		Vector2I endMap = LocalToMap(ToLocal(endWorld));

		// This returns a list of world positions for the enemy to follow
		long[] path = _astar.GetIdPath(GetIdForCell(startMap), GetIdForCell(endMap));
		List<Vector2> waypoints = [];
		foreach (long id in path)
		{
			waypoints.Add(ToGlobal(MapToLocal(GetCellForId(id))));
		}
		if (!startedFromPath)
		{
			SetPointSolid(startMap, true);
		}
		return waypoints;
	}

	public List<Vector2> GetPath(Vector2I startMap, Vector2I endMap)
	{
		TileData data = GetCellTileData(startMap);
		bool startedFromPath = (bool)data.GetCustomData("Path");
		if (!startedFromPath)
		{
			SetPointSolid(startMap, false);
		}

		// This returns a list of world positions for the enemy to follow
		long[] path = _astar.GetIdPath(GetIdForCell(startMap), GetIdForCell(endMap));
		List<Vector2> waypoints = [];
		foreach (long id in path)
		{
			waypoints.Add(ToGlobal(MapToLocal(GetCellForId(id))));
		}
		if (!startedFromPath)
		{
			SetPointSolid(startMap, true);
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

	private Vector2I GetAtlasCoordsForPath(bool up, bool down, bool left, bool right)
	{
		if (!up && !down && !left && !right) return new Vector2I(5, 2);

		if (!up && !down && left && right) return new Vector2I(1, 0);

		if (up && down && !left && !right) return new Vector2I(5, 0);

		if (!up && down && left && !right) return new Vector2I(6, 0);

		if (!up && down && !left && right) return new Vector2I(7, 0);

		if (up && !down && !left && right) return new Vector2I(8, 0);

		if (up && !down && left && !right) return new Vector2I(9, 0);

		if (up && !down && left && right) return new Vector2I(10, 0);

		if (!up && down && left && right) return new Vector2I(11, 0);

		if (up && down && !left && right) return new Vector2I(12, 0);

		if (up && down && left && !right) return new Vector2I(13, 0);

		if (up && down && left && right) return new Vector2I(14, 0);

		if (!up && !down && left && !right) return new Vector2I(15, 0);

		if (!up && !down && !left && right) return new Vector2I(0, 1);

		if (up && !down && !left && !right) return new Vector2I(1, 1);

		if (!up && down && !left && !right) return new Vector2I(2, 1);

		// Fallback tile
		return new Vector2I(-1, -1);
	}
}
