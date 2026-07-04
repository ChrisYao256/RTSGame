using Godot;
using Godot.Collections;
using RTSGame.Source;
using RTSGame.Units;

public partial class DummyEffect : Effect
{
	public DummyEffect(EffectResource resource) : base(resource)
	{
	}

	public override void ConnectSignals(Unit unit)
	{
		base.ConnectSignals(unit);
		QueueFree();
	}
}
