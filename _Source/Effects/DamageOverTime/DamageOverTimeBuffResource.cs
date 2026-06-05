using Godot;
using Godot.Collections;
using System.Linq;
namespace RTSGame.Units;

[GlobalClass]
public partial class DamageOverTimeBuffResource : EffectBuffResource
{
	[Export]
	public int _damage;

	[Export]
	public float _time;

	public override void SetDescription()
	{
		_effectDescription = "";
	}

	public override void ApplyBuff(Array<EffectResource> candidates)
	{
		DamageOverTimeResource resource = candidates.OfType<DamageOverTimeResource>().FirstOrDefault();
		resource._damage += _damage;
		resource._time += _time;
		resource.SetDescription();
	}

	public override Effect CreateNode()
	{
		return new DamageOverTimeBuff(this);
	}
}
