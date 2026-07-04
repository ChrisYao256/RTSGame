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
		if (_delayProjectile > 0)
		{
			Timer timer = new Timer();
			timer.WaitTime = _delayProjectile;
			timer.Timeout += () => ShootProjectiles();
			timer.OneShot = true;
			AddChild(timer);
			timer.Start();
		}
		else
		{
			ShootProjectiles();
		}
	}
	
	private async void ShootProjectiles()
	{
		for (int i = 0; i < _hitCount; i++)
		{
			if (_attackTarget == null)
			{
				return;
			}
			if (i % 2 == 0 || _firePoint2 is null)
			{
				Projectile projectile = SpawnProjectile(_firePoint1.GlobalPosition);
				AddChild(projectile);
			}
			else
			{
				Projectile projectile = SpawnProjectile(_firePoint2.GlobalPosition);
				AddChild(projectile);
			}
				

			if (i < _hitCount - 1)
			{
				await Task.Delay(TimeSpan.FromSeconds(_shotInterval));
			}
		}
		_parent.OnVolleyEnded();
	}

	public override float GetDPS()
	{
		return _hitCount * base.GetDPS();
	}

	public override void UpdateWeaponInfoContainerWithUpgrade(StatsIncreaseResource upgrade)
	{
		string greenHex = ThemePalette.Green.ToHtml(false);
		VBoxContainer infoV = _infoContainer.GetNode<VBoxContainer>("VBoxContainer");
		if (upgrade._damageIncrease != 0)
		{
			RichTextLabel damageLabel = infoV.GetNode<RichTextLabel>("DamageLabel");
			damageLabel.Text += $"[color=#{greenHex}]=>{upgrade._damageIncrease + GetDamage()}[/color]";
		}
		if (upgrade._attackSpeedIncrease != 0)
		{
			RichTextLabel cooldownLabel = infoV.GetNode<RichTextLabel>("CooldownLabel");
			cooldownLabel.Text += $"[color=#{greenHex}]=>{GetCooldown() / (1 + upgrade._attackSpeedIncrease):F2}[/color]";
		}
		if (upgrade._attackSpeedIncrease != 0 || upgrade._damageIncrease != 0)
		{
			RichTextLabel dpsLabel = infoV.GetNode<RichTextLabel>("DPSLabel");
			dpsLabel.Text += $"[color=#{greenHex}]=>{_hitCount * (GetDamage() + upgrade._damageIncrease) / (float)(GetCooldown() / (1 + upgrade._attackSpeedIncrease)):F0}[/color]";
		}
		if (upgrade._rangeIncrease != 0)
		{
			RichTextLabel rangeLabel = infoV.GetNode<RichTextLabel>("RangeLabel");
			rangeLabel.Text += $"[color=#{greenHex}]=>{GetRange() + upgrade._rangeIncrease}[/color]";
		}
	}
}