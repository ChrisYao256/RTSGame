using Godot;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RTSGame.Source;

public partial class Grid : TileMapLayer
{
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
	public bool CanBuildAt(Vector2 worldPosition)
	{
		Vector2I gridPos = LocalToMap(worldPosition);

		// 1. Check TileSet data (is it grass/dirt?)
		TileData data = GetCellTileData(gridPos);
		if (data == null || !(bool)data.GetCustomData("buildable"))
		{
			return false;
		}

		// 2. Check if a tower is already there
		if (!IsCellVacant(gridPos))	
		{
			return false;
		}

		return true;
	}

	public Vector2 MapToGlobal(Vector2I mapPosition)
	{
		Vector2 local = MapToLocal(mapPosition);
		return ToGlobal(local);
	}

	//public void MarkCellAsOccupied(Vector2 worldPosition)
	//{
	//	Vector2I gridPos = LocalToMap(worldPosition);
	//	_occupiedCells.Add((gridPos, ));
	//}

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
		Vector2I endMap = LocalToMap(ToLocal(endWorld));

		// This returns a list of world positions for the enemy to follow
		Godot.Collections.Array<Vector2I> path = _astar.GetIdPath(startMap, endMap);
		List<Vector2> waypoints = [];
		foreach (Vector2I cell in path)
		{
			waypoints.Add(ToGlobal(MapToLocal(cell)));
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
