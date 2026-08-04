using Godot;
using RTSGame.Source;
using System.Collections.Generic;

namespace RTSGame.Units;

public abstract partial class GlobalEffect : Node2D
{
	protected GlobalEffectResource _resource;
	public TDManager _tdManager;
	public bool _noImmediateEffect;

	public GlobalEffect(GlobalEffectResource resource)
	{
		resource.SetDescription();
		_resource = resource;
	}

	public void ConnectSignals()
	{
		_tdManager.Connect(TDManager.SignalName.GlobalPlacedTower, Callable.From<TowerUnit>(OnPlacedTower));
		_tdManager.Connect(TDManager.SignalName.GlobalBeforeDamage, Callable.From<DamageContext>(OnBeforeDamage));
		_tdManager.Connect(TDManager.SignalName.GlobalAddEffect, Callable.From<AddEffectContext>(OnAddNewEffect));
		_tdManager.Connect(TDManager.SignalName.GlobalExplosion, Callable.From<ExplosionContext>(OnExplosion));
		_tdManager.Connect(TDManager.SignalName.GlobalBossSpawned, Callable.From(OnBossSpawned));
		_tdManager.Connect(TDManager.SignalName.NewWave, Callable.From(OnNewWave));
	}

	public virtual void OnCreation()
	{

	}

	protected virtual void OnUnitDied()
	{

	}

	protected virtual void OnHpChange(int change)
	{

	}

	protected virtual void OnExplosion(ExplosionContext context)
	{

	}

	protected virtual void OnBeginAttack(Unit target)
	{

	}

	protected virtual void OnStopAttack(Unit target)
	{

	}

	protected virtual void OnBeforeDamage(DamageContext damageContext)
	{

	}

	protected virtual void OnAddNewEffect(AddEffectContext addEffectContext)
	{

	}

	protected virtual void OnHitEnemy(Unit target)
	{

	}

	protected virtual void OnVolleyEnded()
	{

	}

	protected virtual void OnBeforeIsHit(Unit source)
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

	protected virtual void OnBossSpawned()
	{

	}
}