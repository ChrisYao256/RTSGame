using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RTSGame.Source;

public partial class Chunk : TileMapLayer
{
	[Export]
	public bool _connectionLeft;

	[Export]
	public bool _connectionRight;

	[Export]
	public bool _connectionUp;

	[Export]
	public bool _connectionDown;

	public Vector2I _chunkCoord;
}