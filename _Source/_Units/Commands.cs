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

	public abstract bool CheckFinish();

	public abstract string GetDescription();

	public virtual void ForceFinish()
	{
		FinishCommand();
	}

	protected virtual void FinishCommand()
	{
		_parent.ProcessNextCommand();
	}
}

public class NoCommand: Command
{
	public NoCommand(Unit parent): base(parent) { }

	public override bool CheckFinish()
	{
		return false;
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

	public override bool CheckFinish()
	{
		if ((_parent.GlobalPosition - _targetLocation).Length() <= UnitPathfinder.DesiredDistance)
		{
			FinishCommand();
			return true;
		}
		return false;
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

	public override bool CheckFinish()
	{
		if ((_parent.GlobalPosition-_targetLocation).Length() <= UnitPathfinder.DesiredDistance)
		{
			FinishCommand();
			return true;
		}
		return false;
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

	public override bool CheckFinish()
	{
		if (!GodotObject.IsInstanceValid(_targetUnit))
		{
			FinishCommand();
			return true;
		}
		return false;
	}

	public override string GetDescription()
	{
		return "Force attacking " + _targetUnit._name;
	}
}

public class AggroedAttackMove : Command
{
	public Unit _targetUnit;
	public Vector2 _originalPosition;

	public AggroedAttackMove(Unit parent, Vector2 originalPosition, Unit targetUnit) : base(parent)
	{
		_targetUnit = targetUnit;
		_originalPosition = originalPosition;
	}

	public override bool CheckFinish()
	{
		if (!GodotObject.IsInstanceValid(_targetUnit))
		{
			FinishCommand();
			return true;
		}
		else if ((_parent.GlobalPosition - _originalPosition).Length() > _parent.LeashDistance)
		{
			FinishCommand();
			return true;
		}
		return false;
	}

	protected override void FinishCommand()
	{
		_parent.InsertCommand(0, new ForceMove(_parent, _originalPosition));
		base.FinishCommand();
	}

	public override string GetDescription()
	{
		return "Aggroed by " + _targetUnit._name;
	}
}