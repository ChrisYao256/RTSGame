using Godot;
using RTSGame._Source.Units;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalLargerExplosionResource : GlobalEffectResource
{
	[Export]
	public float _extraRange;


	public override void SetDescription()
	{
		_effectDescription = $"Explosions are {_extraRange * 100:F0}% larger";
	}

	public override GlobalEffect CreateNode()
	{
		return new GlobalLargerExplosion(this);
	}
}
