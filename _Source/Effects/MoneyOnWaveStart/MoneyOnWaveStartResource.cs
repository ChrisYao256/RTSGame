using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class MoneyOnWaveStartResource : EffectResource
{
	[Export]
	public Vector4I _money;

	public override void SetDescription()
	{
		_effectDescription = "Provide $" + _money + " each round. "; 
	}

	public override Effect CreateNode()
	{
		return new MoneyOnWaveStart(this);
	}
}
