using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RTSGame.Source;

public partial class GridManager : TileMapLayer
{
	public const int ChunkSize = 3;

	private static Godot.Collections.Dictionary<int, PackedScene> ChunkLibrary = new Godot.Collections.Dictionary<int, PackedScene>
	{
		{ 0, GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Chunks/Chunk00.tscn") },
		{ 1, GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Chunks/Chunk01.tscn") },
		{ 2, GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Chunks/Chunk02.tscn") },
		{ 3, GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Chunks/Chunk03.tscn") },
		{ 4, GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Chunks/Chunk04.tscn") },
		{ 5, GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Chunks/Chunk05.tscn") },
		{ 6, GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Chunks/Chunk06.tscn") },
		{ 7, GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Chunks/Chunk07.tscn") },
		{ 8, GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Chunks/Chunk08.tscn") },
		{ 9, GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Chunks/Chunk09.tscn") },
		{ 10, GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/Chunks/Chunk10.tscn") }
	};

	/// <summary>
	/// Gives the map coord of the center tile of the chunk (assuming ChunkSize = 3).
	/// </summary>
	/// <param name="chunkCoord"></param>
	/// <returns></returns>
	public static Vector2I ChunkCoordToMapCoord(Vector2I chunkCoord)
	{
		return chunkCoord * ChunkSize + new Vector2I(1, 1);
	}

	//public static Chunk GetChunkAtMapCoord(Vector2I mapCoord)
	//{

	//}

	[Export]
	public Vector2I _defaultExitLocation;

	[Export]
	public Vector2I _defaultEntranceLocation;

	[Export]
	public Array<Vector2I> _startingRevealableChunks;

	[Export]
	public Array<ChunkResource> _startingBoundaryChunks;

	[Export]
	public Vector2I _mapSize;

	/// <summary>
	/// Map coordinates for the top left tile in the map. These should almost always be two negative numbers.
	/// </summary>
	[Export]
	public Vector2I _topLeftTile;

	private Array<Vector2I> _unrevealableChunks = [];

	public Array<Chunk> _revealedChunks = [];

	private Godot.Collections.Dictionary<Vector2I, Array<bool>> _walkableTiles = new();

	private Array<Vector2I> _startingCells;

	private Array<Chunk> _chunksWithUp = [];
	private Array<Chunk> _chunksWithDown = [];
	private Array<Chunk> _chunksWithLeft = [];
	private Array<Chunk> _chunksWithRight = [];

	private CanvasLayer _choicesLayer;
	private Label _choicesTitle;
	private PanelContainer _choicesPanel;

	private Node2D _chunkUI;

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


	private AStar2D _astar;

	private Godot.Collections.Dictionary<Vector2I, TowerUnit> _occupiedCells = new Godot.Collections.Dictionary<Vector2I, TowerUnit>();

	private TileMapLayer _overlayLayer;


	public override void _Ready()
	{
		_overlayLayer = GetParent().GetNode<TileMapLayer>("OverlayLayer");
		CategorizeChunkLibrary();
		_choicesLayer = GetParent().GetNode<CanvasLayer>("ChunkSelectLayer");
		_choicesPanel = GetParent().GetNode<PanelContainer>("ChunkSelectLayer/PanelContainer");
		_choicesTitle = _choicesPanel.GetNode<Label>("VBoxContainer/Label");
	}

	private void CategorizeChunkLibrary()
	{
		_chunksWithUp = [];
		_chunksWithDown = [];
		_chunksWithLeft = [];
		_chunksWithRight = [];
		foreach (PackedScene scene in ChunkLibrary.Values)
		{
			Chunk chunk = scene.Instantiate<Chunk>();
			if (chunk._connectionUp)
			{
				_chunksWithUp.Add(chunk);
			}
			if (chunk._connectionDown)
			{
				_chunksWithDown.Add(chunk);
			}
			if (chunk._connectionLeft)
			{
				_chunksWithLeft.Add(chunk);
			}
			if (chunk._connectionRight)
			{
				_chunksWithRight.Add(chunk);
			}
		}
	}

	public void InitializeChunks()
	{
		_startingCells = GetUsedCells().Duplicate();
		if (_topLeftTile.X % ChunkSize != 0 || _topLeftTile.Y % ChunkSize != 0)
		{
			throw new Exception("Top left offset must be a multiple of chunk size!");
		}
		if (_mapSize.X % ChunkSize != 0 || _mapSize.Y % ChunkSize != 0)
		{
			throw new Exception("Map size must be a multiple of chunk size!");
		}
		_chunkUI = new Node2D();
		GetParent().AddChild(_chunkUI);
		
		Array<Vector2I> usedCells = GetUsedCells();
		for (int i = _topLeftTile.X / ChunkSize; i < (_mapSize.X + _topLeftTile.X) / ChunkSize; i++)
		{
			for (int j = _topLeftTile.Y / ChunkSize; j < (_mapSize.Y + _topLeftTile.Y) / ChunkSize; j++)
			{
				if (!usedCells.Contains(new Vector2I(i * ChunkSize, j * ChunkSize)))
				{
					SetCell(ChunkCoordToMapCoord(new Vector2I(i, j)), 0, new Vector2I(0, 6));
					if (!_startingRevealableChunks.Contains(new Vector2I(i, j)))
					{
						_unrevealableChunks.Add(new Vector2I(i, j));
					}
					else
					{
						MakeNewRevealableChunk(new Vector2I(i, j));
					}
				}
			}
		}

		foreach (ChunkResource boundaryChunk in _startingBoundaryChunks)
		{
			_revealedChunks.Add(boundaryChunk.MakeChunk());
		}
		
		SetupNavigation();
	}

	private void MakeNewRevealableChunk(Vector2I chunkCoord)
	{
		SetCell(ChunkCoordToMapCoord(chunkCoord), 0, atlasCoords: new Vector2I(3, 6));
		_unrevealableChunks.Remove(chunkCoord);

		Button unlockButton = new Button();
		unlockButton.Text = "Unlock";
		_chunkUI.AddChild(unlockButton);
		var test = ChunkCoordToMapCoord(chunkCoord);
		unlockButton.GlobalPosition = MapToGlobal(ChunkCoordToMapCoord(chunkCoord)) + new Vector2(50, 50);

		unlockButton.Pressed += () =>
		{
			unlockButton.QueueFree();

			bool upBlocked = false;
			bool upRequired = false;
			bool downBlocked = false;
			bool downRequired = false;
			bool rightRequired = false;
			bool rightBlocked = false;
			bool leftBlocked = false;
			bool leftRequired = false;

			foreach (Chunk oldChunk in _revealedChunks)
			{
				// Neighbor is to the Right
				if (oldChunk._chunkCoord - chunkCoord == new Vector2I(1, 0))
				{
					if (oldChunk._connectionLeft)
					{
						rightRequired = true;
					}
					else
					{
						rightBlocked = true;
					}
				}

				// Neighbor is to the Left
				if (oldChunk._chunkCoord - chunkCoord == new Vector2I(-1, 0))
				{
					if (oldChunk._connectionRight)
					{
						leftRequired = true;
					}
					else
					{
						leftBlocked = true;
					}
				}

				// Neighbor is Down
				if (oldChunk._chunkCoord - chunkCoord == new Vector2I(0, 1))
				{
					if (oldChunk._connectionUp)
					{
						downRequired = true;
					}
					else
					{
						downBlocked = true;
					}
				}

				// Neighbor is Up
				if (oldChunk._chunkCoord - chunkCoord == new Vector2I(0, -1))
				{
					if (oldChunk._connectionDown)
					{
						upRequired = true;
					}
					else
					{
						upBlocked = true;
					}
				}
			}

			Array<string> requiredConnections = [];
			if (rightRequired)
			{
				requiredConnections.Add("Right");
			}
			if (leftRequired)
			{
				requiredConnections.Add("Left");
			}
			if (upRequired)
			{
				requiredConnections.Add("Up");
			}
			if (downRequired)
			{
				requiredConnections.Add("Down");
			}

			Vector2I upChunk = new Vector2I(chunkCoord.X, chunkCoord.Y - 1);
			Vector2I downChunk = new Vector2I(chunkCoord.X, chunkCoord.Y + 1);
			Vector2I leftChunk = new Vector2I(chunkCoord.X - 1, chunkCoord.Y);
			Vector2I rightChunk = new Vector2I(chunkCoord.X + 1, chunkCoord.Y);

			Vector2I upMap = ChunkCoordToMapCoord(upChunk);
			Vector2I downMap = ChunkCoordToMapCoord(downChunk);
			Vector2I leftMap = ChunkCoordToMapCoord(leftChunk);
			Vector2I rightMap = ChunkCoordToMapCoord(rightChunk);

			// Blocked checks (Part of center or out of bounds)
			if (_startingCells.Contains(upMap) || upMap.Y < _topLeftTile.Y)
			{
				upBlocked = true;
			}

			if (_startingCells.Contains(downMap) || downMap.Y > _topLeftTile.Y + _mapSize.Y)
			{
				downBlocked = true;
			}

			if (_startingCells.Contains(leftMap) || leftMap.X < _topLeftTile.X)
			{
				leftBlocked = true;
			}

			if (_startingCells.Contains(rightMap) || rightMap.X > _topLeftTile.X + _mapSize.X)
			{
				rightBlocked = true;
			}

			if (upRequired && upBlocked)
			{
				upBlocked = false;
			}

			if (downRequired && downBlocked)
			{
				downBlocked = false;
			}

			if (leftRequired && leftBlocked)
			{
				leftBlocked = false;
			}

			if (rightRequired && rightBlocked)
			{
				rightBlocked = false;
			}

			MakeChunkChoicePrompt(
			upBlocked, downBlocked, leftBlocked, rightBlocked,
			upRequired, downRequired, leftRequired, rightRequired,
			chunkCoord
			);
		};
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
	public void MakeTileWalkable(Vector2I gridPos, bool walkable = true, bool bidirectional = true)
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
					_astar.ConnectPoints(currentId, neighborId, bidirectional);
				}
			}
		}
		if (!walkable)
		{
			SetCell(gridPos, 0, atlasCoords: GetAtlasCoordsForPath(false, false, false, false));
		}
		else
		{
			SetCell(gridPos, 0, atlasCoords: GetAtlasCoordsForPath(connectedDirections["Up"], connectedDirections["Down"], connectedDirections["Left"], connectedDirections["Right"], false));
		}
		if (_walkableTiles.ContainsKey(gridPos))
		{
			_walkableTiles[gridPos] = [walkable, bidirectional];
		}
		else
		{
			_walkableTiles.Add(gridPos, [walkable, bidirectional]);
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
		return MapToGlobal(_defaultEntranceLocation);
	}

	public Vector2 GetDefaultExitPosition()
	{
		return MapToGlobal(_defaultExitLocation);
	}

	public Vector2I GetDefaultEntranceMapPosition()
	{
		return _defaultEntranceLocation;
	}

	public Vector2I GetDefaultExitMapPosition()
	{
		return _defaultExitLocation;
	}

	private void SetupNavigation()
	{
		_astar = new AStar2D();
		// 1. Add all points to the graph
		foreach (Vector2I cell in GetUsedCells())
		{
			long id = GetIdForCell(cell);
			// AStar2D needs a Vector2 position to calculate pathfinding heuristics (distance).
			// Using the grid coordinates works perfectly for this.
			

			TileData data = GetCellTileData(cell);
			bool isPath = data != null && (bool)data.GetCustomData("Path");
			bool isFog = data != null && (bool)data.GetCustomData("Fog");

			// 2. Disable the point if it's not a path. 
			// (It still exists in the graph, so your portal spawning code can temporarily enable it later)
			if (!isFog)
			{
				_astar.AddPoint(id, new Vector2(cell.X, cell.Y));
			}
			if (!isPath && !isFog)
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
						Vector2I test = GetCellForId(currentId);
						Vector2I test2 = GetCellForId(neighborId);
					}
				}
			}
		}
	}

	private void RefreshNavigation()
	{
		SetupNavigation();
		foreach (Vector2I pos in _walkableTiles.Keys)
		{
			MakeTileWalkable(pos, _walkableTiles[pos][0], _walkableTiles[pos][1]);
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

	private long GetIdForCell(Vector2I pos)
	{
		ulong packed = ((ulong)(uint)pos.X << 32) | (uint)pos.Y;
		return (long)(packed & 0x7FFFFFFFFFFFFFFF);
	}


	// Unpacks the 64-bit integer back into a Vector2I
	private Vector2I GetCellForId(long id)
	{
		int x = (int)((ulong)id >> 32);
		int y = (int)(id & 0xFFFFFFFF);
		return new Vector2I(x, y);
	}

	private void SetPointSolid(Vector2I cell, bool disabled = true)
	{
		_astar.SetPointDisabled(GetIdForCell(cell), disabled);
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

	public void MakeChunkChoicePrompt(bool upBlocked, bool downBlocked, bool leftBlocked, bool rightBlocked, bool upRequired, bool downRequired, bool leftRequired, bool rightRequired,Vector2I chunkCoord, int n = 3)
	{
		_choicesLayer.Show();
		_choicesPanel.Show();
		Array<Chunk> matchingChunks = new Array<Chunk>(_chunksWithUp.Union(_chunksWithDown).Union(_chunksWithLeft).Union(_chunksWithRight));

		if (upBlocked)
		{
			matchingChunks = new Array<Chunk>(matchingChunks.Except(_chunksWithUp));
		}
		if (downBlocked)
		{
			matchingChunks = new Array<Chunk>(matchingChunks.Except(_chunksWithDown));
		}
		if (leftBlocked)
		{
			matchingChunks = new Array<Chunk>(matchingChunks.Except(_chunksWithLeft));
		}
		if (rightBlocked)
		{
			matchingChunks = new Array<Chunk>(matchingChunks.Except(_chunksWithRight));
		}

		// Apply mandatory connection constraints (must have paths matching adjacent open exits)
		if (upRequired)
		{
			matchingChunks = new Array<Chunk>(matchingChunks.Intersect(_chunksWithUp));
		}
		if (downRequired)
		{
			matchingChunks = new Array<Chunk>(matchingChunks.Intersect(_chunksWithDown));
		}
		if (leftRequired)
		{
			matchingChunks = new Array<Chunk>(matchingChunks.Intersect(_chunksWithLeft));
		}
		if (rightRequired)
		{
			matchingChunks = new Array<Chunk>(matchingChunks.Intersect(_chunksWithRight));
		}

		Array<Chunk> selectedChunks = Utils.GetRandomElements<Chunk>(matchingChunks, n);
		GridContainer gridContainer = _choicesPanel.GetNode<GridContainer>("VBoxContainer/GridContainer");
		foreach (var node in gridContainer.GetChildren())
		{
			node.QueueFree();
		}
		_choicesTitle.Text = StringDB.Entries["ChunkChoice"];
		if (selectedChunks.Count < 4)
		{
			gridContainer.Columns = selectedChunks.Count;
		}
		else
		{
			gridContainer.Columns = 3;
		}
		foreach (Chunk chunk in selectedChunks)
		{
			VBoxContainer vbox = new();

			Control wrapper = new Control();

			Chunk chunk_ = (Chunk)chunk.Duplicate();

			Vector2 chunkScale = new Vector2(2, 2);

			chunk_.Scale = chunkScale;

			wrapper.AddChild(chunk_.Duplicate());

			Vector2 chunkPixelSize = new Vector2(ChunkSize, ChunkSize) * TileSet.TileSize * chunkScale;

			wrapper.CustomMinimumSize = chunkPixelSize;

			vbox.AddChild(wrapper);

			Button pickButton
				= new Button();
			pickButton.Pressed += (() =>
			{
				PlaceChunk(chunkCoord, chunk_);
				if (chunk_._connectionRight && _unrevealableChunks.Contains(chunkCoord + new Vector2I(1, 0)))
				{
					MakeNewRevealableChunk(chunkCoord + new Vector2I(1, 0));
				}
				if (chunk_._connectionLeft && _unrevealableChunks.Contains(chunkCoord + new Vector2I(-1, 0)))
				{
					MakeNewRevealableChunk(chunkCoord + new Vector2I(-1, 0));
				}
				if (chunk_._connectionDown && _unrevealableChunks.Contains(chunkCoord + new Vector2I(0, 1)))
				{
					MakeNewRevealableChunk(chunkCoord + new Vector2I(0, 1));
				}
				if (chunk_._connectionUp && _unrevealableChunks.Contains(chunkCoord + new Vector2I(0, -1)))
				{
					MakeNewRevealableChunk(chunkCoord + new Vector2I(0, -1));
				}
				_choicesLayer.Hide();
			});

			pickButton.Text = "Select";

			vbox.AddChild(pickButton);

			PanelContainer panelContainer = new();
			panelContainer.AddChild(vbox);
			gridContainer.AddChild(panelContainer);
		}
	}

	private void PlaceChunk(Vector2I chunkCoord, Chunk newChunk)
	{
		Vector2I topLeft = ChunkCoordToMapCoord(chunkCoord) - new Vector2I(1,1);
		for (int i = 0; i < ChunkSize; i++)
		{
			for (int j = 0; j < ChunkSize; j++)
			{
				Vector2I atlasCoords = newChunk.GetCellAtlasCoords(new Vector2I(i, j));
				SetCell(topLeft + new Vector2I(i,j), 0, atlasCoords: atlasCoords);
			}
		}
		_revealedChunks.Add(newChunk);
		newChunk._chunkCoord = chunkCoord;

		RefreshNavigation();
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

	private Vector2I GetAtlasCoordsForPath(bool up, bool down, bool left, bool right, bool bidirectional = true)
	{
		if (bidirectional)
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
		else
		{
			if (!up && !down && left && right) return new Vector2I(1, 4);

			if (up && down && !left && !right) return new Vector2I(5, 4);

			if (!up && down && left && !right) return new Vector2I(6, 4);

			if (!up && down && !left && right) return new Vector2I(7, 4);

			if (up && !down && !left && right) return new Vector2I(8, 4);

			if (up && !down && left && !right) return new Vector2I(9, 4);

			if (up && !down && left && right) return new Vector2I(10, 4);

			if (!up && down && left && right) return new Vector2I(11, 4);

			if (up && down && !left && right) return new Vector2I(12, 4);

			if (up && down && left && !right) return new Vector2I(13, 4);

			if (up && down && left && right) return new Vector2I(14, 4);

			if (!up && !down && left && !right) return new Vector2I(15, 4);

			if (!up && !down && !left && right) return new Vector2I(0, 5);

			if (up && !down && !left && !right) return new Vector2I(1, 5);

			if (!up && down && !left && !right) return new Vector2I(2, 5);

			// Fallback tile
			return new Vector2I(-1, -1);
		}

			
	}
}
