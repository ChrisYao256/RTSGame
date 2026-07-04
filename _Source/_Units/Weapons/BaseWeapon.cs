using Godot;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

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
	public float _damageBuffPercent = 0f;
	public float _rangeDebuffPercent = 0;
	public double _attackSpeedDebuff = 0;

	public bool _hasCustomPriority = false;

	protected PanelContainer _infoContainer;

	protected Marker2D _firePoint;

	public override void _Ready()
	{
		_parent = GetParent<Unit>();
		_attackTimer = _attackCooldown;
		_firePoint = GetParent().GetNode("TurretTurner").GetNode<Marker2D>("Marker2D");
	}

	public void SetDisplayWeapon()
	{
		_parent = GetParent<Unit>();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_attackTimer >= 0)
		{
			_attackTimer -= delta;
		}
		if (_attackTarget is not null)
		{
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
			_attackTimer += GetAttackDelay();
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

		RichTextLabel dpsLabel = new();
		dpsLabel.Text = "DPS: " + GetDPS().ToString("F0");
		dpsLabel.Name = "DPSLabel";
		dpsLabel.BbcodeEnabled = true;
		dpsLabel.FitContent = true;
		infoV.AddChild(dpsLabel);

		RichTextLabel damageLabel = new();
		damageLabel.Text = "Damage: " + GetDamage().ToString();
		damageLabel.Name = "DamageLabel";
		damageLabel.BbcodeEnabled = true;
		damageLabel.FitContent = true;
		infoV.AddChild(damageLabel);

		RichTextLabel cooldownLabel = new();
		cooldownLabel.Text = "Cooldown: " + GetCooldown().ToString("F2");
		cooldownLabel.Name = "CooldownLabel";
		cooldownLabel.BbcodeEnabled = true;
		cooldownLabel.FitContent = true;
		infoV.AddChild(cooldownLabel);

		RichTextLabel rangeLabel = new();
		rangeLabel.Text = "Range: " + GetRange().ToString();
		rangeLabel.Name = "RangeLabel";
		rangeLabel.BbcodeEnabled = true;
		rangeLabel.FitContent = true;
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

		RichTextLabel damageLabel = infoV.GetNode<RichTextLabel>("DamageLabel");
		damageLabel.Text = "Damage: " + GetDamage().ToString();

		RichTextLabel cooldownLabel = infoV.GetNode<RichTextLabel>("CooldownLabel");
		cooldownLabel.Text = "Cooldown: " + GetCooldown().ToString("F2");

		RichTextLabel dpsLabel = infoV.GetNode<RichTextLabel>("DPSLabel");
		dpsLabel.Text = "DPS: " + GetDPS().ToString("F0");

		RichTextLabel rangeLabel = infoV.GetNode<RichTextLabel>("RangeLabel");
		rangeLabel.Text = "Range: " + GetRange().ToString();
	}

	public virtual List<InvaderUnit> FormCustomTargetOrder(List<InvaderUnit> units)
	{
		return units;
	}

	public virtual void UpdateWeaponInfoContainerWithUpgrade(StatsIncreaseResource upgrade)
	{
		string greenHex = ThemePalette.Green.ToHtml(false);
		VBoxContainer infoV = _infoContainer.GetNode<VBoxContainer>("VBoxContainer");
		if (upgrade._damageIncrease != 0)
		{
			RichTextLabel damageLabel = infoV.GetNode<RichTextLabel>("DamageLabel");
			damageLabel.Text = $"[color=#{greenHex}]Damage: {upgrade._damageIncrease + GetDamage()}[/color]";
		}
		if (upgrade._attackSpeedIncrease != 0)
		{
			RichTextLabel cooldownLabel = infoV.GetNode<RichTextLabel>("CooldownLabel");
			cooldownLabel.Text = $"[color=#{greenHex}]Cooldown: {GetCooldown() / (1 + upgrade._attackSpeedIncrease):F2}[/color]";
		}
		if (upgrade._attackSpeedIncrease != 0 || upgrade._damageIncrease != 0)
		{
			RichTextLabel dpsLabel = infoV.GetNode<RichTextLabel>("DPSLabel");
			dpsLabel.Text = $"[color=#{greenHex}]DPS: {(GetDamage() + upgrade._damageIncrease) / (float)(GetCooldown() / (1 + upgrade._attackSpeedIncrease)):F0}[/color]";
		}
		if (upgrade._rangeIncrease != 0)
		{
			RichTextLabel rangeLabel = infoV.GetNode<RichTextLabel>("RangeLabel");
			rangeLabel.Text = $"[color=#{greenHex}]Range: {GetRange() + upgrade._rangeIncrease}[/color]";
		}
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
		return (_range + _parent._data._rangeIncrease) * (1 - _rangeDebuffPercent);
	}

	public int GetDamage()
	{
		return (int)Math.Ceiling((_damage + _parent._data._damageIncrease) * (1f + _parent._data._damagePercentIncrease) * (1f + _damageBuffPercent));
	}

	public double GetCooldown()
	{
		return _attackCooldown / ((1 + _parent._data._attackSpeedIncrease) * (1 - _attackSpeedDebuff));
	}

	public double GetAttackDelay()
	{
		return GD.RandRange(_attackDelayLow / (1 + _parent._data._attackDelayModifierIncrease), _attackDelayHigh / (1 + _parent._data._attackDelayModifierIncrease));
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