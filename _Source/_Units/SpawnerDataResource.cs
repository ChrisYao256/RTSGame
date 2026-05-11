using Godot;
using Godot.Collections;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class SpawnerDataResource : Resource
{
	[Export]
	public Array<string> _units = new Array<string>();

	[Export]
	public Array<Vector2I> _locations = new Array<Vector2I>();

	[Export]
	public float _hpBuff;

	[Export]
	public float _speedBuff;

	[Export]
	public int _moneyBuff;

	[Export]
	public int _hpLossBuff;

	[Export]
	public int _moneyLossBuff;

	[Export]
	public Array<EffectResource> _startingEffects;
}
