using Godot;
using RTSGame.Units;

public partial class MeleeWeapon : BaseWeapon
{

	public override void PerformAttack(Unit target, int d)
	{
		target.Hit(new DamageContext(_parent, target, d, DamageType.DirectAttack));
	}
}