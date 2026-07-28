using Godot;
using Godot.NativeInterop;
using RTSGame.Units;
using System;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading.Tasks;

public partial class MultiProjectileWeapon : ProjectileWeapon
{
	[Export]
	private int _hitCount;

	[Export]
	private float _shotInterval;

	private Marker2D _firePoint1;
	private Marker2D _firePoint2;

	public override void _Ready()
	{
		base._Ready();
		_firePoint1 = GetParent().GetNode("TurretTurner").GetNode<Marker2D>("FirePoint1");
		_firePoint2 = GetParent().GetNode("TurretTurner").GetNodeOrNull<Marker2D>("FirePoint2");
	}

	public override void PerformAttack(Unit target, int d)
	{
		_parent.EmitSignal(Unit.SignalName.ShotFired);
		int d_ = d;

		if (_delayProjectile > 0)
		{
			Timer timer = new Timer();
			timer.WaitTime = _delayProjectile;
			timer.Timeout += () => ShootProjectiles(d_);
			timer.OneShot = true;
			AddChild(timer);
			timer.Start();
		}
		else
		{
			ShootProjectiles(d);
		}
	}
	
	private async void ShootProjectiles(int damage)
	{
		for (int i = 0; i < _hitCount; i++)
		{
			if (_attackTarget == null || !IsInstanceValid(_attackTarget))
			{
				return;
			}
			if (i % 2 == 0 || _firePoint2 is null)
			{
				Projectile projectile = SpawnProjectile(_firePoint1.GlobalPosition, damage);
				AddChild(projectile);
			}
			else
			{
				Projectile projectile = SpawnProjectile(_firePoint2.GlobalPosition, damage);
				AddChild(projectile);
			}
				

			if (i < _hitCount - 1)
			{
				await ToSignal(GetTree().CreateTimer(_shotInterval, processAlways: false), SceneTreeTimer.SignalName.Timeout);
			}
		}
		_parent.OnVolleyEnded();
	}

	public override float GetDPS()
	{
		return _hitCount * base.GetDPS();
	}
}