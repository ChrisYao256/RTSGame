using Godot;
using RTSGame.Units;
using System;

public abstract class Command
{
	protected Unit _parent;
	public Command(Unit parent)
	{
		_parent = parent;
	}

	public abstract void CheckFinish();

	public abstract string GetDescription();

	public virtual void ForceFinish()
	{
		FinishCommand();
	}

	protected void FinishCommand()
	{
		_parent.ProcessNextCommand();
	}
}

public class NoCommand: Command
{
	public NoCommand(Unit parent): base(parent) { }

	public override void CheckFinish()
	{
		
	}

	public override void ForceFinish()
	{
	}

	public override string GetDescription()
	{
		return "No command";
	}
}

public class AttackMove : Command
{
	public Vector2 _targetLocation;

	public AttackMove(Unit parent, Vector2 targetLocation) : base(parent)
	{
		_targetLocation = targetLocation;

	}

	public override void CheckFinish()
	{
		if ((_parent.GlobalPosition - _targetLocation).Length() <= UnitPathfinder.DesiredDistance)
		{
			FinishCommand();
		}
	}

	public override string GetDescription()
	{
		return "Attack moving to (" + _targetLocation.X.ToString() + ", " + _targetLocation.Y.ToString() + ")";
	}
}

public class ForceMove : Command
{
	public Vector2 _targetLocation;

	public ForceMove(Unit parent, Vector2 targetLocation) : base(parent)
	{
		_targetLocation = targetLocation;
	}

	public override void CheckFinish()
	{
		if ((_parent.GlobalPosition-_targetLocation).Length() <= UnitPathfinder.DesiredDistance)
		{
			FinishCommand();
		}
	}

	public override string GetDescription()
	{
		return "Force moving to (" + _targetLocation.X.ToString() + ", " + _targetLocation.Y.ToString() + ")";
	}
}

public class ForceAttack : Command
{
	public Unit _targetUnit;

	public ForceAttack(Unit parent, Unit targetUnit) : base(parent)
	{
		_targetUnit = targetUnit;
	}

	public override void CheckFinish()
	{
		if (!GodotObject.IsInstanceValid(_targetUnit))
		{
			FinishCommand();
		}
	}

	public override string GetDescription()
	{
		return "Force attacking " + _targetUnit._name;
	}
}