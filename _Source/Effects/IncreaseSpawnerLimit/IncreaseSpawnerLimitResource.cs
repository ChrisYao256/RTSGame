using Godot;
using System;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class IncreaseSpawnerLimitResource : EffectResource
{
	public override void SetDescription()
	{
		_effectName = "Portal Limit Upgrade";
	}

	public override Effect CreateNode()
	{
		return new IncreaseSpawnerLimit(this);
	}
}
