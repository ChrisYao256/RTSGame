using Godot;
using RTSGame.Units;

public enum DamageType
{
	DirectAttack,
	Burn,
	Explosion,
	Other,
}

public partial class DamageContext : RefCounted
{
	public Unit _attacker { get; set; }
	public Unit _target { get; set; }
	public float _rawDamage { get; set; }
	public float _finalDamage { get; set; }
	public DamageType Type { get; set; }

	public DamageContext(Unit attacker, Unit target, float rawDamage, DamageType type)
	{
		_attacker = attacker;
		_target = target;
		_rawDamage = rawDamage;
		_finalDamage = rawDamage;
		Type = type;
	}
}