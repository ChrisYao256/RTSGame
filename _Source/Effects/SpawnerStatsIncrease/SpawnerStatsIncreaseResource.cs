using Godot;
using Godot.Collections;
using System.Linq.Expressions;
using static Godot.HttpRequest;

namespace RTSGame.Units;

[GlobalClass]
public partial class SpawnerStatsIncreaseResource : EffectResource
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
	public int _armorBuff;

	[Export]
	public int _moneyBuff;

	[Export]
	public int _hpLossBuff;

	[Export]
	public int _moneyLossBuff;

	[Export]
	public Array<EffectResource> _startingEffects;

	public SpawnerStatsIncreaseResource MultiplyEffect(int n)
	{
		SpawnerStatsIncreaseResource newResource = (SpawnerStatsIncreaseResource)Duplicate();
		if (_units.Count > 0)
		{
			Array<string> newUnits = [];
			for (int i = 0; i < n; i++)
			{
				newUnits.AddRange(_units);
			}
			newResource._units = newUnits;
		}
		newResource._hpBuff *= n;
		newResource._hpLossBuff *= n;
		newResource._armorBuff *= n;
		newResource._moneyBuff *= n;
		newResource._moneyLossBuff *= n;
		newResource._speedBuff *= n;
		newResource.SetDescription();
		return newResource;
	}

	public override void SetDescription()
	{
		_effectDescription = "";
		if (_units != null && _units.Count > 0)
		{
			
			_effectDescription += "Spawns " + _units.Count + " extra enemies.\n";
		}

		if (_hpBuff != 0)
		{
			
			_effectDescription += "Increase spawned enemy HP by " + _hpBuff * 100 + "%\n";
		}

		if (_speedBuff != 0)
		{
			
			_effectDescription += "Increase spawned enemy speed by " + _speedBuff + "\n";
		}

		if (_armorBuff != 0)
		{

			_effectDescription += "Increase spawned enemy armor by " + _armorBuff + "\n";
		}

		if (_moneyBuff != 0)
		{
			
			_effectDescription += "Increase spawned enemy gold drop by " + _moneyBuff + "\n";
		}
		if (_locations.Count > 0)
		{
			
			_effectDescription += "Changes spawning area \n";
		}
		if (_startingEffects.Count > 0)
		{
			_effectDescription += "Gives spawned units the following effects: \n";
			foreach (EffectResource effect in _startingEffects)
			{
				effect.SetDescription();
				_effectDescription += effect._effectDescription + "\n";
			}
		}
	}

	public override Effect CreateNode()
	{
		return new SpawnerStatsIncrease(this);
	}
}
