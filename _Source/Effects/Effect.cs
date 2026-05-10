using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public abstract partial class Effect : Node
{
	[Export]
	public string _effectName;

	[Export]
	public Texture2D _effectIcon;

	protected Unit _parentUnit;

	public virtual void ConnectSignals(Unit unit)
	{
		_parentUnit = unit;
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
}
