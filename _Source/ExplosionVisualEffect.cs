using Godot;

public partial class ExplosionVisualEffect : Node2D
{
	public override void _Ready()
	{
		// Automatically delete the visual after 1 second
		GetTree().CreateTimer(0.2f).Timeout += () => QueueFree();
	}
}