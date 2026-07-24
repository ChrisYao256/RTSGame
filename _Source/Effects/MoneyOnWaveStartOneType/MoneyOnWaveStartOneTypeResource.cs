using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class MoneyOnWaveStartOneTypeResource : EffectResource
{
	[Export]
	public int _money;

	/// <summary>
	/// gives the type of resources the player has the least of if set to true. Otherwise gives random type.
	/// </summary>
	[Export]
	public bool _giveLowest;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		MoneyOnWaveStartOneTypeResource typedOldResource = (MoneyOnWaveStartOneTypeResource)oldResource;

		typedOldResource._money += _money;
		typedOldResource.SetDescription();

		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		if (_giveLowest)
		{
			_effectDescription = $"At the start of each round, generate {_money} of the resource you have the least of. ";
		}
		else
		{
			_effectDescription = $"At the start of each round, generate {_money} of a random resource. ";
		}
	}

	public override Effect CreateNode()
	{
		return new MoneyOnWaveStartOneType(this);
	}
}
