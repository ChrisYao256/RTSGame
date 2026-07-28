using Godot;
using Godot.Collections;
using System;

public partial class StripeManager : Control
{
	protected TextureProgressBar _progressBar;

	[Export] public Color StripeColor { get; set; } = new Color(0, 0, 0);

	[Export] public Color LargeStripeColor = new Color(0, 0, 0);
	[Export] public float StripeWidth { get; set; } = 2.0f;

	[Export] public float BigStripeWidth { get; set; } = 3.0f;

  public Array<float> _smallStripeLocations = [];

	public Array<float> _largeStripeLocations = [];

	protected float MinimumSmallIntervalWidth = 5;
	protected float MinimumBigIntervalWidth = 5;

	public override void _Ready()
	{
		// Get the parent TextureProgressBar
		_progressBar = GetParent<TextureProgressBar>();

		if (_progressBar != null)
		{
			// Recalculate stripes whenever the health bar's max_value or layout changes
			_progressBar.Changed += QueueRedraw;
		}
	}

	public override void _Draw()
	{
		if (_progressBar == null) return;

		foreach (float location in _smallStripeLocations)
		{
			Vector2 startPoint;
			Vector2 endPoint;
			switch ((TextureProgressBar.FillModeEnum)_progressBar.FillMode)
			{
				case TextureProgressBar.FillModeEnum.LeftToRight:
					startPoint = new Vector2(location, 0);
					endPoint = new Vector2(location, _progressBar.Size.Y / 2);
					DrawLine(startPoint, endPoint, StripeColor, StripeWidth);
					break;
				case TextureProgressBar.FillModeEnum.BottomToTop:
					startPoint = new Vector2(0, _progressBar.Size.Y - location);
					endPoint = new Vector2(_progressBar.Size.X / 2, _progressBar.Size.Y - location);
					DrawLine(startPoint, endPoint, StripeColor, StripeWidth);
					break;
			}
		}

		foreach (float location in _largeStripeLocations)
		{
			Vector2 startPoint;
			Vector2 endPoint;
			switch ((TextureProgressBar.FillModeEnum)_progressBar.FillMode)
			{
				case TextureProgressBar.FillModeEnum.LeftToRight:
					startPoint = new Vector2(location, 0);
					endPoint = new Vector2(location, _progressBar.Size.Y);
					DrawLine(startPoint, endPoint, LargeStripeColor, StripeWidth);
					break;
				case TextureProgressBar.FillModeEnum.BottomToTop:
					startPoint = new Vector2(0, _progressBar.Size.Y - location);
					endPoint = new Vector2(_progressBar.Size.X, _progressBar.Size.Y - location);
					DrawLine(startPoint, endPoint, LargeStripeColor, StripeWidth);
					break;
			}
		}
	}

	// Clean up event subscription when the node leaves the scene tree
	public override void _ExitTree()
	{
		if (_progressBar != null)
		{
			_progressBar.Changed -= QueueRedraw;
		}
	}
}