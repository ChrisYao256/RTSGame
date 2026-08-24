using Godot;
using System;
using System.ComponentModel.Design;
namespace RTSGame.Units;

[GlobalClass]
public partial class MakeGroundWalkableResource : EffectResource
{
	[Export]
	public bool _bidirectional = true;

	public override void SetDescription()
	{
		_effectName = "";
		_displayType = DisplayTypes.Hidden;
	}

	public override Effect CreateNode()
	{
		return new MakeGroundWalkable(this);
	}
}
