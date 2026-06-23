using Godot;
using Godot.Collections;
using System.Linq;
namespace RTSGame.Units;

[GlobalClass]
public partial class DropMoreMoneyBuffResource : EffectBuffResource
{
	[Export] public Vector4I _increase;
	[Export] public int _allIncrease;
	[Export] public float _time;

	public override void SetDescription()
	{
		_effectDescription = "";
	}

	public override void ApplyBuff(Array<EffectResource> candidates) // applies this buffresource to a matching in candidates
	{
		DropMoreMoneyResource resource = candidates.OfType<DropMoreMoneyResource>().FirstOrDefault();
		resource._increase += _increase;
		resource._time += _time;
		resource.SetDescription();
	}

	public override Effect CreateNode()
	{
		return new DropMoreMoneyBuff(this);
	}
}
