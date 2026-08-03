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

	int _totalDamage;
	float _maxPercentDamage;
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
		if (((DamageOverTimeResource)_resource)._burnVisualScene is not null)
		{
			_visualSceneInstance = ((DamageOverTimeResource)_resource)._burnVisualScene.Instantiate<Node2D>();
			Utils.ScaleVisualToRadius(_visualSceneInstance.GetNode<AnimatedSprite2D>("AnimatedSprite2D"), _parentUnit._radius);
			_parentUnit.AddChild(_visualSceneInstance);
			_visualSceneInstance.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play();
		}
	}

	public void AddResource(DamageOverTimeResource newResource)
	{
		if (newResource._time != -1)
		{
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
		else
		{
			_debuffs.Add((newResource, null));
			RecalculateDebuff();
		}
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
		_maxPercentDamage = 0;
		foreach (var e in _debuffs)
		{
			_totalDamage += e.Item1._damage;
			_maxPercentDamage = Math.Max(e.Item1._percent, _maxPercentDamage);
		}
		int firstDamage = _firstResource._damage;
		float firstPercentDamage = _firstResource._percent;

		_firstResource._damage = _totalDamage;
		_firstResource._percent = _maxPercentDamage;

		_firstResource.SetDescription();
		_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		
		float maxDuration = 0;
		foreach (var e in _debuffs)
		{
			if (e.Item2 is null)
			{
				maxDuration = 9999;
			}
			else if (e.Item2.TimeLeft > maxDuration)
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
		_firstResource._percent = firstPercentDamage;


		if (_totalDamage <= 0 && _maxPercentDamage <= 0)
		{
			if (_visualSceneInstance != null)
			{
				_parentUnit.RemoveChild(_visualSceneInstance);
			}
			RemoveEffectResource();
			RemoveEffectNode();
			_parentUnit.EmitSignal(Unit.SignalName.UpdateInfo);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_totalDamage > 0)
		{
			_queuedDamage += delta * _totalDamage;
		}
		else if (_maxPercentDamage > 0)
		{
			_queuedDamage += delta * _maxPercentDamage * _parentUnit.GetHpMax();
		}
		
		if (_queuedDamage > 1)
		{
			DamageContext context = new DamageContext(null, _parentUnit, Mathf.FloorToInt(_queuedDamage), DamageType.Other);
			_parentUnit.Hit(context);
			_queuedDamage -= Mathf.FloorToInt(_queuedDamage);
		}
	}
}
