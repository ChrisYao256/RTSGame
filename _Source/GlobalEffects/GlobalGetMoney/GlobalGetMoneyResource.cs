using Godot;
using Godot.Collections;
using System.Data;
using System.Drawing;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalGetMoneyResource : GlobalEffectResource
{
	[Export]
	public Vector4I _money;

	[Export]
	public float _percent;

	public override GlobalEffect CreateNode()
	{
		return new GlobalGetMoney(this);
	}

	public override void SetDescription()
	{
		if (_percent != 0)
		{
			_effectDescription = $"Get {_percent * 100:F0}% of your current resources now";
		}
		else
		{
			_effectDescription = $"Get {Utils.MakeMoneyText(_money)} now";
		}
		
	}
}
