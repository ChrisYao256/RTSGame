using Godot;
using RTSGame.Units;
using System;
using System.Collections.Generic;

namespace RTSGame.Source;

public partial class Grid : TileMapLayer
{
	private AStarGrid2D _astar = new AStarGrid2D();

	private HashSet<Vector2I> _occupiedCells = new HashSet<Vector2I>();

	public override void _Ready()
	{
		SetupNavigation();
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
		if (_occupiedCells.Contains(gridPos))
		{
			return false;
		}

		return true;
	}

	public void MarkCellAsOccupied(Vector2 worldPosition)
	{
		Vector2I gridPos = LocalToMap(worldPosition);
		_occupiedCells.Add(gridPos);
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

	public void OccupyCell(Vector2I cell)
	{
		_occupiedCells.Add(cell);
	}

	public bool IsCellVacant(Vector2I cell)
	{
		foreach (Vector2I occupiedCell in _occupiedCells)
		{
			if (occupiedCell == cell) return false;
		}
		return true;
	}
}
