using Godot;
using RTSGame.Units;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ScannerWeapon : BaseWeapon
{
	[Export] public Color _tracerColor = ThemePalette.Blue;
	[Export] public float _tracerWidth = 6.0f;
	[Export] public EffectResource _prioritizeEnemyWithoutThisEffect;

	private Line2D _tracerLine;
	private Timer _tracerTimer;

	private bool _scanning = false;
	private Unit _queuedAttackTarget;

	public override void _Ready()
	{
		_tracerLine = GetNode<Line2D>("TracerLine");
		// Ensure the line has 2 points to work with
		_tracerLine.ClearPoints();
		_tracerLine.AddPoint(Vector2.Zero);
		_tracerLine.AddPoint(Vector2.Zero);
		_tracerLine.DefaultColor = _tracerColor;
		_tracerLine.Width = _tracerWidth;
		_tracerTimer = GetNode<Timer>("TracerLine/Timer");
		_tracerTimer.Timeout += () =>
		{
			_parent.OnBeforeHitEnemy(_attackTarget);
			_attackTarget.Hit(GetDamage(), _parent);
			_parent.OnHitEnemy(_attackTarget);

			ResetScanTarget();
		};
		_hasCustomPriority = true;
		base._Ready();
	}

	public override List<InvaderUnit> FormCustomTargetOrder(List<InvaderUnit> units)
	{
		Godot.Collections.Dictionary<InvaderUnit, float> orderDict = new Godot.Collections.Dictionary<InvaderUnit, float>();
		List<InvaderUnit> withEffect = [];
		List<InvaderUnit> withoutEffect = [];
		foreach (var unit in units)
		{
			bool hasMatchingEffect = unit._effects.Any(e => e.GetType() == _prioritizeEnemyWithoutThisEffect.GetType());

			if (hasMatchingEffect)
			{
				orderDict.Add(unit, unit.GetDistanceToExit() + 10000f);
			}
			else
			{
				orderDict.Add(unit, unit.GetDistanceToExit());
			}
		}
		return units.OrderBy(unit => orderDict[unit]).ToList();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_scanning && _queuedAttackTarget is not null)
		{
			_attackTimer -= delta;
			if (_attackTimer < 0 && GetParent().GetNode<TurretTurner>("TurretTurner")._finishedTurning)
			{
				if (_useAttackDelay)
				{
					_attackTimer = GetAttackDelay();
				}
				PerformAttack(_queuedAttackTarget, GetDamage());
			}
		}
		else
		{

			_tracerLine.SetPointPosition(0, _firePoint.GlobalPosition);
			if (IsInstanceValid(_attackTarget))
			{
				_tracerLine.SetPointPosition(1, _attackTarget.GlobalPosition);
			}
			else
			{
				ResetScanTarget();
			}
		}
	}

	public override void PerformAttack(Unit target, int d)
	{
		_scanning = true;
		_tracerLine.Visible = true;
		_attackTarget = target;

		_tracerLine.SetPointPosition(0, GlobalPosition);
		_tracerLine.SetPointPosition(1, target.GlobalPosition);

		_tracerTimer.Start(_attackCooldown);

		// Start (or restart) the timer for the set duration
		_tracerTimer.Start();
	}

	public override void BeginAttackingTarget(Unit target)
	{
		if (_queuedAttackTarget != null)
		{
			throw new Exception("Already have a target!");
		}
		_queuedAttackTarget = target;
		if (_useAttackDelay)
		{
			_attackTimer = GetAttackDelay();
		}
	}

	public override void StopAttackingTarget()
	{
		_queuedAttackTarget = null;
	}

	private void ResetScanTarget()
	{
		_tracerTimer.Stop();
		_tracerLine.Visible = false;
		_scanning = false;
	}

	public override void UpdateWeaponInfoContainerWithUpgrade(StatsIncreaseResource upgrade)
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
			dpsLabel.Text = $"[color=#{greenHex}]DPS: {(GetDamage() + upgrade._damageIncrease) / (float)(GetCooldown() / (1 + upgrade._attackSpeedIncrease) + GetAttackDelay() / (1 + upgrade._attackDelayModifierIncrease)):F0}[/color]";
		}
		if (upgrade._rangeIncrease != 0)
		{
			RichTextLabel rangeLabel = infoV.GetNode<RichTextLabel>("RangeLabel");
			rangeLabel.Text = $"[color=#{greenHex}]Range: {GetRange() + upgrade._rangeIncrease}[/color]";
		}
	}

	public override float GetDPS()
	{
		return (GetDamage()) / (float)(GetCooldown() + GetAttackDelay());
	}
}