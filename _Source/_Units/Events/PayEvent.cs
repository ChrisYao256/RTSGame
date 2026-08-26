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
public partial class PayEvent : Event
{
	/// Boss: spawn enemies near the tower. When they are all cleared, get a reward
	/// Reward: get a reward for free
	/// Pay: pay to get a reward

	[Export]
	public Vector4I _price;

	[Export]
	public int _hpPrice;

	[Export]
	public Array<RewardResource> _reward;
}