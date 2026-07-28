using Godot;
using System;

public partial class HealthBarStripeManager : StripeManager
{

	// How much HP does each vertical segment represent?
	[Export] public float _valuePerSmallStripe { get; set; } = 100.0f;

	[Export] public float _valuePerBigStripe { get; set; } = 500.0f;

	public void UpdateHealthBar()
	{
		double maxHp = _progressBar.MaxValue;
		float barWidth = _progressBar.Size.X - _progressBar.StretchMarginLeft - _progressBar.StretchMarginRight;
		float barHeight = _progressBar.Size.Y;

		// Calculate how many stripes we need
		double test = Math.Floor(maxHp / _valuePerSmallStripe);

		int totalStripes = (int)Math.Floor(maxHp / _valuePerSmallStripe);

		// If max HP is too low for even one stripe, don't draw anything
		if (totalStripes <= 0) return;

		// Find out exactly how many pixels represent 1 HP on the screen
		float pixelsPerHp = barWidth / (float)maxHp;

		// Don't draw small stripes if they are too dense
		if (pixelsPerHp * _valuePerSmallStripe > MinimumSmallIntervalWidth)
		{
			// Loop and draw each vertical line
			for (int i = 1; i < totalStripes + 1; i++)
			{
				float hpMilestone = i * _valuePerSmallStripe;
				float xPos = _progressBar.StretchMarginLeft + hpMilestone * pixelsPerHp;
				_smallStripeLocations.Add(xPos);
			}
		}

		int totalBigStripes = (int)Math.Floor(maxHp / _valuePerBigStripe);

		// If max HP is too low for even one stripe, don't draw anything
		if (totalBigStripes <= 0) return;

		if (pixelsPerHp * _valuePerBigStripe > MinimumBigIntervalWidth)
		{
			// Loop and draw each vertical line
			for (int i = 1; i < totalBigStripes + 1; i++)
			{
				float hpMilestone = i * _valuePerBigStripe;
				float xPos = _progressBar.StretchMarginLeft + hpMilestone * pixelsPerHp;

				_largeStripeLocations.Add(xPos);
			}
		}
	}

	public override void _Draw()
	{
		UpdateHealthBar();
		base._Draw();
	}
}