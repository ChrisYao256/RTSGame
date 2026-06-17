using Godot;
using System;

public partial class StripeManager : Control
{
	private TextureProgressBar _healthBar;

	// How much HP does each vertical segment represent?
	[Export] public float HpPerSmallStripe { get; set; } = 100.0f;

	[Export] public float HpPerBigStripe { get; set; } = 500.0f;
	[Export] public Color StripeColor { get; set; } = new Color(0, 0, 0, 0.4f); // Semi-transparent black
	[Export] public float StripeWidth { get; set; } = 2.0f;

	[Export] public float BigStripeWidth { get; set; } = 4.0f;

	private float MinimumSmallIntervalWidth = 5;

	public override void _Ready()
	{
		// Get the parent TextureProgressBar
		_healthBar = GetParent<TextureProgressBar>();

		if (_healthBar != null)
		{
			// Recalculate stripes whenever the health bar's max_value or layout changes
			_healthBar.Changed += QueueRedraw;
		}
	}

	public override void _Draw()
	{
		if (_healthBar == null) return;

		double maxHp = _healthBar.MaxValue;
		float barWidth = _healthBar.Size.X;
		float barHeight = _healthBar.Size.Y;

		// Calculate how many stripes we need
		double test = Math.Floor(maxHp / HpPerSmallStripe);

		int totalStripes = (int)Math.Floor(maxHp / HpPerSmallStripe);

		// If max HP is too low for even one stripe, don't draw anything
		if (totalStripes <= 0) return;

		// Find out exactly how many pixels represent 1 HP on the screen
		float pixelsPerHp = barWidth / (float)maxHp;

		// Don't draw small stripes if they are too dense
		if (pixelsPerHp * HpPerSmallStripe > MinimumSmallIntervalWidth)
		{
			// Loop and draw each vertical line
			for (int i = 1; i < totalStripes + 1; i++)
			{
				float hpMilestone = i * HpPerSmallStripe;
				float xPos = hpMilestone * pixelsPerHp;

				// Draw a line from the top of the bar to the bottom
				Vector2 startPoint = new Vector2(xPos, 0);
				Vector2 endPoint = new Vector2(xPos, barHeight / 2);

				DrawLine(startPoint, endPoint, StripeColor, StripeWidth);
			}
		}



		int totalBigStripes = (int)Math.Floor(maxHp / HpPerBigStripe);

		// If max HP is too low for even one stripe, don't draw anything
		if (totalBigStripes <= 0) return;

		// Loop and draw each vertical line
		for (int i = 1; i < totalBigStripes + 1; i++)
		{
			float hpMilestone = i * HpPerBigStripe;
			float xPos = hpMilestone * pixelsPerHp;

			// Draw a line from the top of the bar to the bottom
			Vector2 startPoint = new Vector2(xPos, 0);
			Vector2 endPoint = new Vector2(xPos, barHeight);

			DrawLine(startPoint, endPoint, StripeColor, BigStripeWidth);
		}
	}

	// Clean up event subscription when the node leaves the scene tree
	public override void _ExitTree()
	{
		if (_healthBar != null)
		{
			_healthBar.Changed -= QueueRedraw;
		}
	}
}