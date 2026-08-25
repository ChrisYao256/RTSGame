using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RTSGame.Source;

[GlobalClass]
public partial class ChunkResource : Resource
{
	[Export]
	public bool _connectionLeft;

	[Export]
	public bool _connectionRight;

	[Export]
	public bool _connectionUp;

	[Export]
	public bool _connectionDown;

	[Export]
	public Vector2I _chunkCoord;

	public Chunk MakeChunk()
	{
		Chunk chunk = new Chunk();
		chunk._connectionLeft = _connectionLeft;
		chunk._connectionRight = _connectionRight;
		chunk._connectionUp = _connectionUp;
		chunk._connectionDown = _connectionDown;
		chunk._chunkCoord = _chunkCoord;
		return chunk;
	}
}