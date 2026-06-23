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
		MakeFloatingAnimation();
	}

	protected void MakeFloatingAnimation()
	{
		if (_resource._effectName != "")
		{
			_parentUnit._currentFloatingAnimationCount++;
			var textNode = GD.Load<PackedScene>("res://_Content/_Scenes/_Prefabs/UI/FloatingText.tscn").Instantiate<FloatingText>();
			textNode.BbcodeEnabled = true;
			textNode.FitContent = true;

			// Set the text
			textNode.Text = "+" + _resource._effectName;

			// Set the position to the unit's current global position
			textNode.GlobalPosition = GlobalPosition + new Vector2(0, 30) * (_parentUnit._currentFloatingAnimationCount - 1);

			Timer timer = new();
			timer.Timeout += () => _parentUnit._currentFloatingAnimationCount--;
			timer.OneShot = true;
			GetTree().Root.AddChild(timer);
			timer.Start(1f);

			// VERY IMPORTANT: Add it to the world, not the unit!
			// If you add it to the unit, it will disappear instantly when the unit is freed.
			GetTree().Root.AddChild(textNode);

			textNode.StartFloatingAnimation();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (_resource._tempDebuffIcon is not null && IsInstanceValid(_resource._tempDebuffIcon))
		{
			UpdateTempDebuffIcon(_resource._tempDebuffIcon);
		}
	}

	protected virtual void UpdateTempDebuffIcon(UpgradeButton button)
	{

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

	protected virtual void OnBeforeHitEnemy(Unit target)
	{

	}

	protected virtual void OnHitEnemy(Unit target)
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
}