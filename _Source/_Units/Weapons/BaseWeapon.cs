using Godot;
using RTSGame.Units;
using System;

public abstract partial class BaseWeapon : Node2D
{
	public enum DamageType
	{
		Null,
		Physical,
		Energy,
		Flame,
		Explosive
	}

	[Export] protected int _damage;
	[Export] protected float _range;

	[Export] protected double _attackCooldown = 1.0;
	[Export] private double _attackDelayLow = 0.1;
	[Export] private double _attackDelayHigh = 0.2;

	[Export] public bool _useAttackDelay = false;

	[Export] public DamageType _damageType;

	[Export] private string _description;

	protected double _attackTimer = 0;

	protected Unit _parent;
	public Unit _attackTarget;

	public int _damageModifier = 0;
	public float _damagePercentModifier = 1f;
	public float _rangeModifier = 0;
	public double _attackSpeedModifier = 0;
	public double _attackSpeedDebuff = 0;
	public double _attackDelayModifier = 0;

	protected PanelContainer _infoContainer;

	public override void _Ready()
	{
		_parent = GetParent<Unit>();
		_attackTimer = _attackCooldown;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_attackTarget is not null)
		{
			_attackTimer -= delta;
			if (_attackTimer < 0)
			{
				_attackTimer = GetCooldown();
				PerformAttack(_attackTarget, GetDamage());
			}
		}
	}

	public virtual void BeginAttackingTarget(Unit target)
	{
		if (_attackTarget != null)
		{
			throw new Exception("Already have a target!");
		}
		_attackTarget = target;
		if (_useAttackDelay)
		{
			_attackTimer = GetAttackDelay();
		}
	}

	public virtual void StopAttackingTarget()
	{
		_attackTarget = null;
	}

	// Abstract method: Every attacker must define HOW they hit
	public abstract void PerformAttack(Unit target, int damage);

	public virtual PanelContainer MakeWeaponInfoContainer()
	{
		_infoContainer = new();
		_infoContainer.CustomMinimumSize = new(200, 0);

		VBoxContainer infoV = new();
		infoV.Name = "VBoxContainer";

		Label dpsLabel = new();
		dpsLabel.Text = "DPS: " + GetDPS().ToString("F0");
		dpsLabel.Name = "DPSLabel";
		infoV.AddChild(dpsLabel);

		Label damageLabel = new();
		damageLabel.Text = "Weapon Damage: " + GetDamage().ToString();
		damageLabel.Name = "DamageLabel";
		infoV.AddChild(damageLabel);

		Label cooldownLabel = new();
		cooldownLabel.Text = "Weapon Cooldown: " + GetCooldown().ToString("F2");
		cooldownLabel.Name = "CooldownLabel";
		infoV.AddChild(cooldownLabel);

		Label rangeLabel = new();
		rangeLabel.Text = "Range: " + GetRange().ToString();
		rangeLabel.Name = "RangeLabel";
		infoV.AddChild(rangeLabel);

		if (GetDescription() != "" && GetDescription() is not null)
		{
			Label descriptionLabel = new();
			descriptionLabel.Text = GetDescription();
			descriptionLabel.Name = "DescriptionLabel";
			infoV.AddChild(descriptionLabel);
		}

		_infoContainer.AddChild(infoV);
		return _infoContainer;
	}

	public virtual void UpdateWeaponInfoContainer()
	{
		VBoxContainer infoV = _infoContainer.GetNode<VBoxContainer>("VBoxContainer");

		Label damageLabel = infoV.GetNode<Label>("DamageLabel");
		damageLabel.Text = "Weapon Damage: " + GetDamage().ToString();

		Label cooldownLabel = infoV.GetNode<Label>("CooldownLabel");
		cooldownLabel.Text = "Weapon Cooldown: " + GetCooldown().ToString("F2");

		Label dpsLabel = infoV.GetNode<Label>("DPSLabel");
		dpsLabel.Text = "DPS: " + GetDPS().ToString("F0");

		Label rangeLabel = infoV.GetNode<Label>("RangeLabel");
		rangeLabel.Text = "Range: " + GetRange().ToString();
	}

	public virtual void ResetCooldown()
	{
		_attackTimer = GetCooldown();
	}

	public virtual void ResetWeaponInfoContainer()
	{
		_infoContainer = null;
	}

	public float GetRange()
	{
		return _range + _rangeModifier;
	}

	public int GetDamage()
	{
		return (int)((_damage + _damageModifier) * _damagePercentModifier);
	}

	public double GetCooldown()
	{
		return _attackCooldown / ((1 + _attackSpeedModifier) * (1 - _attackSpeedDebuff));
	}

	public double GetAttackDelay()
	{
		return GD.RandRange(_attackDelayLow / (1 + _attackDelayModifier), _attackDelayHigh / (1 + _attackDelayModifier));
	}

	public virtual float GetDPS()
	{
		return (GetDamage()) / (float) (GetCooldown());
	}

	public string GetDescription()
	{
		return _description;
	}
}