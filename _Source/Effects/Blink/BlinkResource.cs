using Godot;
namespace RTSGame.Units;

[GlobalClass]
public partial class BlinkResource : EffectResource
{
	[Export]
	public float _distance;
	[Export] public float _tracerWidth = 2f;
	[Export] public Color _tracerColor = ThemePalette.Blue;

	public override void SetDescription()
	{
		_displayType = DisplayTypes.Large;

		_effectDescription = $"Blinks when hit for the first time. ";
	}

	public override Effect CreateNode()
	{
		return new Blink(this);
	}
}
