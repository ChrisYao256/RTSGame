using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace RTSGame.Units;

[GlobalClass]
public partial class TowerRewardResource : RewardResource
{
	public enum TowerType
	{
		Defense,
		Portal,
		Tower,
		DefenseAll,
		PortalAll,
		TowerAll,
	}

	[Export]
	public TowerType _type;
}
