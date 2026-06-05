using Godot;
using Godot.Collections;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class NearbyUnlockUpgradeResource : EffectResource
{
	[Export]
	public bool _unlockSecondUpgrades;

	[Export]
	public bool _unlockThirdUpgrades;

	[Export]
	public Array<Vector2I> _area = new Array<Vector2I>();

	public override void SetDescription()
	{
		_effectDescription = $"Unlocks the {(_unlockSecondUpgrades ? "second" : "third")} tier of upgrades for nearby towers.";
	}

	public override Effect CreateNode()
	{
		return new NearbyUnlockUpgrade(this);
	}
}
