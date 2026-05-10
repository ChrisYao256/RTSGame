using Godot;
using System;

public partial class FloatingText : Label
{
	public override void _Ready()
	{
		// 1. Create a tween for the "Pop" effect
		Tween tween = GetTree().CreateTween();

		// Parallel makes the movement and fading happen at the same time
		tween.SetParallel(true);

		// 2. Float upwards (Move -50 pixels on Y axis)
		tween.TweenProperty(this, "position:y", Position.Y - 50, 1.0f)
				 .SetTrans(Tween.TransitionType.Cubic)
				 .SetEase(Tween.EaseType.Out);

		// 3. Fade out
		tween.TweenProperty(this, "modulate:a", 0.0f, 1.0f);

		// 4. Delete the node once the animation is done
		tween.SetParallel(false); // Wait for the above to finish
		tween.Chain().TweenCallback(Callable.From(QueueFree));
	}
}