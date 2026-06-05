using Godot;
using Godot.Collections;
using System.Xml.Linq;

namespace RTSGame.Units;

[GlobalClass]
public partial class StatsIncreaseOnKillResource : EffectResource
{
	[Export]
	public StatsIncreaseResource _buffResource;

	[Export]
	public int _cap = -1;

	public int _stack = 0;

	public override void SetDescription()
	{
		_effectDescription = $"For each enemy killed: \n";
		_buffResource.SetDescription();
		_effectDescription += _buffResource._effectDescription;
		_effectDescription += "Currently killed " + _stack;
	}

	public override Effect CreateNode()
	{
		return new StatsIncreaseOnKill(this);
	}
}
