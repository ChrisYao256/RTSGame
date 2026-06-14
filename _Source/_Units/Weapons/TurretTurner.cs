using Godot;
using Godot.Collections;
using RTSGame.Source;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class TurretTurner : Node2D
{
	[Export] public float _rotationSpeed = 20.0f;
	[Export] public bool _turn;

	private BaseWeapon _parentWeapon;
	private TowerUnit _baseTower;

	private bool _queueStopAttacking = false;

	private AnimatedSprite2D _animatedSprite;

	public override void _Ready()
	{
		_parentWeapon = GetParent().GetNode<BaseWeapon>("WeaponComponent");
		_baseTower = (TowerUnit)GetParent();
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_animatedSprite.Play();

		_baseTower.Connect(Unit.SignalName.BeginAttack, Callable.From<Unit>(OnBeginAttack));
		_baseTower.Connect(Unit.SignalName.StopAttack, Callable.From<Unit>(OnStopAttack));
		_baseTower.Connect(Unit.SignalName.ShotFired, Callable.From(OnShotFired));

		_animatedSprite.AnimationFinished += () =>
		{
			if (_animatedSprite.Animation == "Charging")
			{
				_animatedSprite.Play("Attacking");
			}
			else if (_animatedSprite.Animation == "Fire")
			{
				if (_animatedSprite.SpriteFrames.HasAnimation("Attacking") && !_queueStopAttacking)
				{
					_animatedSprite.Play("Attacking");
				}
				else
				{
					_animatedSprite.Play("Idle");
				}
			}
		};
	}

	public override void _Process(double delta)
	{
		if (_turn && _parentWeapon._attackTarget != null && IsInstanceValid(_parentWeapon._attackTarget))
		{
			RotateTowardsTarget(delta);
		}
	}

	private void RotateTowardsTarget(double delta)
	{
		// 1. Calculate the angle to the target
		Vector2 targetDir = _parentWeapon._attackTarget.GlobalPosition - GlobalPosition;
		float targetAngle = targetDir.Angle();

		// 2. Smoothly rotate the pivot toward that angle
		// Use AngleLerp to prevent the turret from spinning 360 degrees the wrong way
		float currentAngle = GlobalRotation;
		GlobalRotation = (float)Mathf.LerpAngle(currentAngle, targetAngle, _rotationSpeed * delta);
	}

	private void OnBeginAttack(Unit target)
	{
		if (_parentWeapon._useAttackDelay && _animatedSprite.SpriteFrames.HasAnimation("Charging"))
		{
			SpriteFrames frames = _animatedSprite.SpriteFrames;

			int frameCount = frames.GetFrameCount("Charging");

			float speed = frameCount / (float)_parentWeapon.GetAttackDelay();

			_animatedSprite.SpeedScale = speed;
			_animatedSprite.Play("Charging");
		}
		else if (_animatedSprite.SpriteFrames.HasAnimation("Attacking"))
		{
			_animatedSprite.Play("Attacking");
		}
		else
		{
			return;
		}
	}

	private void OnShotFired()
	{
		if (_animatedSprite.SpriteFrames.HasAnimation("Fire"))
		{
			_animatedSprite.Play("Fire");
		}
	}

	private void OnStopAttack(Unit target)
	{
		if (_animatedSprite.Animation != "Fire")
		{
			_animatedSprite.Play("Idle");
		}
		else
		{
			_queueStopAttacking = true;
		}
	}
}