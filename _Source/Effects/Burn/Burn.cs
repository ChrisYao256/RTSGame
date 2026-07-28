using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class Burn : Effect
{
	BurnResource _firstResource;

	double _damageCooldown = 1.0;

	Node2D _visualSceneInstance;

	float _queuedDecrease;

	public Burn(BurnResource resource) : base(resource)
	{
		_firstResource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		_firstResource.SetDescription();
		_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		OnCreation();
	}

	protected override void OnCreation()
	{
		_damageCooldown = _damageCooldown / (1f + _firstResource._burnIntervalMultiplier);
		_visualSceneInstance = ((BurnResource)_resource)._burnVisualScene.Instantiate<Node2D>();
		Utils.ScaleVisualToRadius(_visualSceneInstance.GetNode<AnimatedSprite2D>("AnimatedSprite2D"), _parentUnit._radius);
		_parentUnit.AddChild(_visualSceneInstance);
		_visualSceneInstance.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play();
		DamageContext context = new DamageContext(null, _parentUnit, _firstResource._damage, DamageType.Burn);
		_parentUnit.Hit(context);
	}

	public void AddResource(BurnResource newResource)
	{
		_firstResource._damage += newResource._damage;
		_firstResource.SetDescription();
		_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_parentUnit._effects.Any(e => e.GetType() == typeof(PermBurnResource)))
		{
			float decrease = _parentUnit.GetHpMax() * _firstResource._decayRate * (float)delta + _queuedDecrease;
			_queuedDecrease = decrease - (float)Math.Floor(decrease);
			if (decrease >= 1.0)
			{
				_firstResource._damage -= (int)Math.Floor(decrease);
				_firstResource.SetDescription();
				_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
				if (_firstResource._damage <= 0)
				{
					_parentUnit.RemoveChild(_visualSceneInstance);
					RemoveEffectResource();
					RemoveEffectNode();
					_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
				}
			}
		}
		_damageCooldown -= delta;
		if (_damageCooldown <= 0.0)
		{
			_damageCooldown += BurnResource.BaseBurnInterval / _firstResource._burnIntervalMultiplier;
			DamageContext context = new DamageContext(null, _parentUnit, _firstResource._damage, DamageType.Burn);
			_parentUnit.Hit(context);
		}
	}
}
