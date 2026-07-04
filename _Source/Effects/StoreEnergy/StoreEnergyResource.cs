using Godot;
using System.Collections.Generic;
namespace RTSGame.Units;

[GlobalClass]
public partial class StoreEnergyResource : EffectResource
{
	[Export]
	public int _increaseAmount;

	[Export]
	public Vector4I _moneyGain;

	[Export]
	public int _increaseLimit;

	public int _turnsWithoutAttacking;

	public override bool MergeWithOld(EffectResource oldResource, List<EffectResource> allMatchingResource)
	{
		StoreEnergyResource typedOldResource = (StoreEnergyResource)oldResource;

		typedOldResource._increaseAmount += _increaseAmount;
		typedOldResource._moneyGain += _moneyGain;
		typedOldResource.SetDescription();
		return false;
	}

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		_effectDescription = $"+{Utils.MakeMoneyText(_moneyGain)} and damage +{_increaseAmount} if this didn't attack last wave. Damage buff stacks {_increaseLimit} times. Currently {_turnsWithoutAttacking} stacks.";
	}

	public override void SetUpgradeDescription()
	{
		_effectDescription = $"Extra +{Utils.MakeMoneyText(_moneyGain)} and +{_increaseAmount} damage each time. ";
	}

	public override Effect CreateNode()
	{
		return new StoreEnergy(this);
	}
}
