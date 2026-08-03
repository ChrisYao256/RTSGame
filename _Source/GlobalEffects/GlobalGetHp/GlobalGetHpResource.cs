using Godot;
using Godot.Collections;
using System.Data;
using System.Drawing;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class GlobalGetHpResource : GlobalEffectResource
{
	[Export]
	public int _hp;

	[Export]
	public bool _everyTurn;

	public override GlobalEffect CreateNode()
	{
		return new GlobalGetHp(this);
	}

	public override void SetDescription()
	{
		_effectDescription = $"Get {_hp} base HP";
		if (_everyTurn)
		{
			_effectDescription += " every turn";
		}

	}
}
