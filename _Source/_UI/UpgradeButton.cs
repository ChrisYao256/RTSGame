using Godot;
using System;

[GlobalClass]
public partial class UpgradeButton : HoverInfoLabel
{
	private TextureProgressBar _borderProgress;
	private TextureRect _interiorRect;

	public UpgradeButton() : base()
	{
		_borderProgress = new TextureProgressBar();
		_borderProgress.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_borderProgress.SizeFlagsHorizontal = Control.SizeFlags.Fill | Control.SizeFlags.Expand;
		AddChild(_borderProgress);
	}

	public override void _Ready()
	{
		StyleBoxEmpty emptyStyle = new StyleBoxEmpty();
		AddThemeStyleboxOverride("pressed", emptyStyle);
		AddThemeStyleboxOverride("hover", emptyStyle);
		AddThemeStyleboxOverride("normal", emptyStyle);
		QueueRedraw();

		_borderProgress.Name = "BorderProgress";
		_borderProgress.MouseFilter = MouseFilterEnum.Ignore;
		_borderProgress.TextureProgress = GD.Load<Texture2D>("res://_Assets/ButtonBorder.png");
		_borderProgress.TextureUnder = GD.Load<Texture2D>("res://_Assets/ButtonBorder.png");
		_borderProgress.MinValue = 0;
		_borderProgress.MaxValue = 100;

		_borderProgress.NinePatchStretch = true;
		_borderProgress.StretchMarginBottom = 12;
		_borderProgress.StretchMarginTop = 12;
		_borderProgress.StretchMarginLeft = 12;
		_borderProgress.StretchMarginRight = 12;

		_borderProgress.TintUnder = ThemePalette.Red;
		_borderProgress.TintProgress = ThemePalette.Green;

		

		//Size = new Vector2(Size.X + 24, Size.Y + 24);
		CustomMinimumSize = Size;
		QueueRedraw();
		base._Ready();
	}

	/// <summary>
	/// Call this whenever the player's money balance changes.
	/// </summary>
	public void UpdateAffordabilityDisplay(float satisfaction)
	{

		// 1. Calculate the percentage (clamped safely between 0.0 and 1.0)
		float ratio = Mathf.Clamp(satisfaction, 0.0f, 1.0f);

		// 2. Update the progress bar fill value (0 to 100)
		_borderProgress.Value = ratio * 100.0f;

		if (satisfaction >= 1f)
		{
			AddThemeColorOverride("font_color", ThemePalette.Green);

		}
		else
		{
			AddThemeColorOverride("font_color", ThemePalette.Red);
		}
	}
}