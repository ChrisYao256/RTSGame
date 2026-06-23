using Godot;
using System;

public partial class FloatingText : RichTextLabel
{
	public FloatingText() : base()
	{
		CustomMinimumSize = new Vector2(300, 0);
	}

	public void StartFloatingAnimation()
	{
		// Create the tween *after* GlobalPosition has been finalized by the spawner
		Tween tween = GetTree().CreateTween();
		tween.SetParallel(true);

		// Use global_position instead of local position to avoid root-node coordinate shifts
		tween.TweenProperty(this, "global_position:y", GlobalPosition.Y - 50, 1.0f)
				 .SetTrans(Tween.TransitionType.Cubic)
				 .SetEase(Tween.EaseType.Out);

		// Fade out
		tween.TweenProperty(this, "modulate:a", 0.0f, 1.0f);

		// Queue Free
		tween.SetParallel(false);
		tween.Chain().TweenCallback(Callable.From(QueueFree));
	}
}