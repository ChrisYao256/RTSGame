using Godot;
using Godot.Collections;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class LevelResource : Resource
{
	[Export]
	public int _mapID;

	[Export]
	public bool _portalsEnabled = true;

	[Export]
	public bool _challengeEnabled = true;

	[Export]
	public bool _inspectionEnabled = true;

	[Export]
	public int _inspectionInterval;

	[Export]
	public int _finalWave;

	[Export]
	public SpawnerDataResource _finalBoss;

	[Export]
	public bool _randomizeFinalBoss;
}

