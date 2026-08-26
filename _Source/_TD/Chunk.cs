using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using static RTSGame.Units.InvaderUnit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RTSGame.Source;

public partial class Chunk : TileMapLayer
{
	public enum EventDifficulty
	{
		None,
		Easy, 
		Medium, 
		Hard
	}

	private static readonly Vector2I[] Directions = new Vector2I[]
	{
				new Vector2I(0, -1), // Up
        new Vector2I(1, 0),  // Right
        new Vector2I(0, 1),  // Down
        new Vector2I(-1, 0)  // Left
	};

	[Export]
	public bool _connectionLeft;

	[Export]
	public bool _connectionRight;

	[Export]
	public bool _connectionUp;

	[Export]
	public bool _connectionDown;

	[Export]
	public EventDifficulty _event;

	public Vector2I _chunkCoord;

	public bool CanBuildAt(Vector2I gridPos)
	{
		// 1. Check TileSet data (is it grass/dirt?)
		TileData data = GetCellTileData(gridPos);
		if (data != null)
		{
			// Access the custom data we set up in the editor
			return (bool)data.GetCustomData("Buildable");
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
					if (!mustBeNextToPath && (CanBuildAt(neighbor)))
					{
						return neighbor;
					}
					else if (mustBeNextToPath && (CanBuildAt(neighbor)))
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

		return new Vector2I(999, 999);
	}
}