using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class DamageOverTime : Effect
{
	DamageOverTimeResource _firstResource;

	List<(DamageOverTimeResource, Timer)> _debuffs = [];

	const int MaxDamage = 1000;

	int _totalDamage;
	double _queuedDamage = 0;

	Node2D _visualSceneInstance;

	Timer _longestTimer;

	public DamageOverTime(DamageOverTimeResource resource) : base(resource)
	{
		_firstResource = resource;
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		AddResource(_firstResource);
		OnCreation();
	}

	protected override void OnCreation()
	{
		_visualSceneInstance = ((DamageOverTimeResource)_resource)._burnVisualScene.Instantiate<Node2D>();
		Utils.ScaleVisualToRadius(_visualSceneInstance.GetNode<AnimatedSprite2D>("AnimatedSprite2D"), _parentUnit._radius);
		_parentUnit.AddChild(_visualSceneInstance);
		_visualSceneInstance.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play();
	}

	public void AddResource(DamageOverTimeResource newResource)
	{
		_parentUnit.IncreaseDamageDealtStat(newResource._damage * newResource._time);
		Timer timer = new Timer();
		AddChild(timer);
		timer.WaitTime = newResource._time;
		timer.OneShot = true;
		timer.Start();
		timer.Timeout += (() => {
			_debuffs.Remove((newResource, timer));
			RecalculateDebuff();
		});
		_debuffs.Add((newResource, timer));
		RecalculateDebuff();
	}

	protected override void UpdateTempDebuffIcon(UpgradeButton button)
	{
		if (_longestTimer is null)
		{
			return;
		}
		button.UpdateAffordabilityDisplay((float)_longestTimer.TimeLeft / _firstResource._time);
	}

	public void RecalculateDebuff()
	{
		_totalDamage = 0;
		foreach (var e in _debuffs)
		{
			_totalDamage += e.Item1._damage;
		}
		int firstDamage = _firstResource._damage;
		_firstResource._damage = _totalDamage;
		_firstResource.SetDescription();
		_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		
		float maxDuration = 0;
		foreach (var e in _debuffs)
		{
			if (e.Item2.TimeLeft > maxDuration)
			{
				maxDuration = (float)e.Item2.TimeLeft;
			}
		}
		if (_longestTimer is null)
		{
			_longestTimer = new Timer();
			_longestTimer.OneShot = true;
			AddChild(_longestTimer);
			_longestTimer.Start(maxDuration);
		}
		else if (maxDuration > _longestTimer.TimeLeft)
		{
			_longestTimer.QueueFree();
			_longestTimer = new Timer();
			_longestTimer.OneShot = true;
			AddChild(_longestTimer);
			_longestTimer.Start(maxDuration);
		}

		_firstResource._damage = firstDamage;
		_totalDamage = Math.Min(MaxDamage, _totalDamage);

		if (_totalDamage == 0)
		{
			_parentUnit.RemoveChild(_visualSceneInstance);
			RemoveEffectResource();
			RemoveEffectNode();
			_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_queuedDamage += delta * _totalDamage;
		if (_queuedDamage > 1)
		{
			_parentUnit.Hit((int)Mathf.FloorToInt(_queuedDamage), null, true);
			_queuedDamage -= Mathf.FloorToInt(_queuedDamage);
		}
	}
}
