using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class ReviveResource : EffectResource
{
	[Export]
	public int _reviveCount = 1;

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;
		
		if (_reviveCount > 1)
		{
			_effectDescription = $"Revives when dying for the next {_reviveCount} times. ";
		}
		else
		{
			_effectDescription = "Revives when dying. ";
		}
	}

	public override Effect CreateNode()
	{
		return new Revive(this);
	}
}
