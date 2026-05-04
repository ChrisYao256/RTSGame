using Godot;
using RTSGame.Units;

public partial class MeleeWeapon : BaseWeapon
{

	public override void PerformAttack(Unit target)
	{
		target.Hit(_damage, _parent);
	}
}