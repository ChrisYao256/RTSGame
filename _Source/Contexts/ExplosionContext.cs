using Godot;
using RTSGame.Units;

public partial class ExplosionContext : RefCounted
{
	public float _radius;

	public int _damage;

	public ExplosionContext(float radius, int damage)
	{
		_radius = radius;
		_damage = damage;
	}
}