using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class Unit : CharacterBody2D
{
	private UnitPathfinder _pathfinder;

	private Sprite2D _selectionVisual;

	private TextureProgressBar _healthBar;

	private BaseWeapon _weapon;

	public int _teamId;

	[Export]
	public int DebugTeamId {
		get => _teamId;
		set {}
}

	[Export] public bool _isBuilding;
	[Export] public float _moveSpeed;

	[Export] public int _hpMax;
	private int _hp;

	[Export]
	public int DebugHp
	{
		get => _teamId;
		set { }
	}

	private UnitPathfinder.State _state;

	public bool _displayAttackRange;

	public event Action<Unit> Died;

	public override void _Ready()
	{
		GD.Print($"{Name} initialized.");
		_weapon = GetNode<BaseWeapon>("WeaponComponent");
		_pathfinder = GetNode<UnitPathfinder>("UnitPathfinder");
		_pathfinder.SetAttackRange(_weapon._range);
		_pathfinder.SetSpeed(_moveSpeed);
		_pathfinder.SetTeamId(_teamId);

		_selectionVisual = GetNode<Sprite2D>("SelectionCircle");

		_healthBar = GetNode<TextureProgressBar>("HealthBar");
		_hp = _hpMax;
		_healthBar.MaxValue = _hpMax;
		_healthBar.Value = _hp;
		UpdateHealthBar(_hpMax, _hpMax);

		
	}

	public void SetSelectionVisible(bool b)
	{
		_selectionVisual.Visible = b;
	}

	public override void _PhysicsProcess(double delta)
	{
		// The Unit just tells the pathfinder to do its job
		_pathfinder.ProcessMovement(delta);
		_state = _pathfinder.GetState(); ;
	}

	public void MoveTo(Vector2 position)
	{
		_pathfinder.SetMoveTarget(position);
	}

	public void AttackMoveTo(Vector2 position)
	{
		_pathfinder.SetAttackMoveTarget(position);
	}

	public void ForceAttack(Unit unit)
	{
		if (unit._teamId != _teamId)
		{
			_pathfinder.SetForceAttackTarget(unit);
		}
	}

	public void Chase(Unit unit)
	{
		if (unit._teamId != _teamId)
		{
			_pathfinder.SetChaseTarget(unit);
		}
	}

	public void StopMoving()
	{
		_pathfinder.ForceFinishNavigation();
	}

	public void BeginAttackingTarget(Unit unit)
	{
		_weapon.BeginAttackingTarget(unit);
	}

	public void Hit(int damage, Unit source)
	{
		_hp -= damage;
		UpdateHealthBar(_hp, _hpMax);

		Area2D socialArea = GetNode<Area2D>("AidArea");
		var nearbyBodies = socialArea.GetOverlappingBodies();

		foreach (var body in nearbyBodies)
		{
			// Check if the body is a Unit and on the same team
			if (body is Unit ally && ally._teamId == this._teamId)
			{
				ally.Retaliate(source);
			}
		}

		if (_hp <= 0)
		{
			Die();
		}
	}

	public void Retaliate(Unit unit)
	{
		if (_state == UnitPathfinder.State.AttackMoving || _state == UnitPathfinder.State.Idle)
		{
			Chase(unit);
		}
	}

	private void UpdateHealthBar(float currentHp, float maxHp)
	{
		float healthPercent = currentHp / maxHp;
		_healthBar.Value = currentHp;

		// Transition from Green (0.33) to Red (0.0) using HSV
		// Or use a simple Lerp between two specific colors:
		Color healthyColor = Colors.Green;
		Color criticalColor = Colors.Red;

		// This blends the two colors based on the health percentage
		_healthBar.Modulate = criticalColor.Lerp(healthyColor, healthPercent);
	}

	private void Die()
	{
		SetProcess(false);
		SetPhysicsProcess(false);

		// 2. Disable collisions so other units don't bump into a corpse
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);

		Died?.Invoke(this);
		QueueFree();
	}

	

	public override void _Draw()
	{
		if (_displayAttackRange)
		{
			// Get the radius from your CollisionShape2D's resource
			var attackAreaShape = GetNode<CollisionShape2D>("UnitPathfinder/AttackArea/AttackAreaCollision").Shape as CircleShape2D;

			if (attackAreaShape != null)
			{
				Color drawColor = new Color(0.2f, 0.6f, 1.0f, 0.3f); // Light blue, semi-transparent
				DrawCircle(Vector2.Zero, attackAreaShape.Radius, drawColor);
			}
		}
	}

	public void DisplayAttackRange()
	{
		_displayAttackRange = true;
		UpdateVisualRange();
	}

	public void HideAttackRange()
	{
		_displayAttackRange = false;
		UpdateVisualRange();
	}

	// Call this whenever the range changes to update the visual
	public void UpdateVisualRange()
	{
		QueueRedraw();
	}
	public UnitPathfinder GetPathfinder()
	{
		return _pathfinder;
	}
}
