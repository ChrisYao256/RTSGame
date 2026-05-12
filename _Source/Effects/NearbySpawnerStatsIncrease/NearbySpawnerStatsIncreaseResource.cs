using Godot;
using Godot.Collections;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class NearbySpawnerStatsIncreaseResource : EffectResource
{
	[Export]
	public SpawnerStatsIncreaseResource _buffResource;

	[Export]
	public Array<Vector2I> _area = new Array<Vector2I>();


	public override void SetDescription()
	{
		_effectDescription = "Buffs nearby spawners with the following: \n";
		if (_buffResource._units != null && _buffResource._units.Count > 0)
		{
			_effectDescription += "Spawns " + _buffResource._units.Count + " extra enemies.\n";
		}

		if (_buffResource._hpBuff != 0)
		{
			_effectDescription += "Increase spawned enemy HP by " + _buffResource._hpBuff + "\n";
		}

		if (_buffResource._speedBuff != 0)
		{
			_effectDescription += "Increase spawned enemy speed by " + _buffResource._speedBuff + "\n";
		}

		if (_buffResource._moneyBuff != 0)
		{
			_effectDescription += "Increase spawned enemy gold drop by " + _buffResource._moneyBuff + "\n";
		}
		if (_buffResource._locations.Count > 0)
		{
			_effectDescription += "Changes spawning area \n";
		}
		if (_buffResource._startingEffects.Count > 0)
		{
			_effectDescription += "Gives spawned units the following effects: \n";
			foreach (EffectResource effect in _buffResource._startingEffects)
			{
				effect.SetDescription();
				_effectDescription += effect._effectDescription + "\n";
			}
		}
	}

	public override Effect CreateNode()
	{
		return new NearbySpawnerStatsIncrease(this);
	}
}
