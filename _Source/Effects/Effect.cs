using Godot;

namespace RTSGame.Units;

public abstract partial class Effect : Node
{
	public string _effectName;

	public Texture2D _effectIcon;

	public string _effectDescription;

	protected Unit _parentUnit;

	public Effect(EffectResource resource)
	{
		_effectName = resource._effectName;
		_effectIcon = resource._effectIcon;
		_effectDescription = resource._effectDescription;
	}

	public virtual void ConnectSignals(Unit unit)
	{
		_parentUnit = unit;
		OnCreation();
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
}