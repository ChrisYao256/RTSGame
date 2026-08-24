using Godot;

public partial class ExplosionVisualEffect : Node2D
{
	[Export]
	public float _time = 0.3f;

	public override void _Ready()
	{
		// Automatically delete the visual after 1 second
		GetTree().CreateTimer(_time).Timeout += () => QueueFree();
	}
}