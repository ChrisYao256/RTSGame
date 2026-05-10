using Godot;
using System;

public partial class TurretTurner : Node2D
{
	[Export] public float _rotationSpeed = 20.0f;
	[Export] public Texture2D _turretTexture;
	private BaseWeapon _parentWeapon;

	public override void _Ready()
	{
		_parentWeapon = GetParent().GetNode<BaseWeapon>("WeaponComponent");
		GetNode<Sprite2D>("Sprite2D").Texture = _turretTexture;
	}

	public override void _Process(double delta)
	{
		if (_parentWeapon._attackTarget != null && IsInstanceValid(_parentWeapon._attackTarget))
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
}