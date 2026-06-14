using Godot;
using System.Collections.Generic;

namespace RTSGame.Units;

public abstract partial class Effect : Node2D
{

	//public string _effectName;

	//public Texture2D _effectIcon;

	//public string _effectDescription;

	protected Unit _parentUnit;

	protected EffectResource _resource;

	public Effect(EffectResource resource)
	{
		resource.SetDescription();
		_resource = resource;
	}

	public virtual void ConnectSignals(Unit unit)
	{
		_parentUnit = unit;
	}

	public virtual void RemoveEffectResource()
	{
		_parentUnit._effects.Remove(_resource);
	}

	public virtual void RemoveEffectNode()
	{
		QueueFree();
	}

	protected virtual void OnCreation()
	{

	}

	protected virtual void OnUnitDied(Unit unit)
	{

	}

	protected virtual void OnHpChange(int change)
	{

	}

	protected virtual void OnBeginAttack(Unit target)
	{

	}

	protected virtual void OnStopAttack(Unit target)
	{

	}

	protected virtual void OnHitEnemy(Unit target)
	{

	}
	
	protected virtual void OnIsHit(Unit source)
	{

	}

	protected virtual void OnKilledEnemy(Unit target)
	{

	}

	protected virtual void OnPlacedTower(TowerUnit tower)
	{

	}

	protected virtual void OnNewWave()
	{

	}
}