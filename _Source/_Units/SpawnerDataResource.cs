using Godot;
using Godot.Collections;
using System.Linq.Expressions;

namespace RTSGame.Units;

[GlobalClass]
public partial class SpawnerDataResource : Resource
{
	[Export]
	public Array<InvaderStatsIncreaseResource> _units = new Array<InvaderStatsIncreaseResource>();
}
