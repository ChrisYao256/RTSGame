using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;

public partial class InvaderUnit : Unit
{
	[Export]
	public int _moneyDropped;
	[Export] 
	private PackedScene _floatingTextScene;
	[Export]
	public int _hpDeducted = 1;

	public override void _Ready()
	{
		base._Ready();
		_aiControlled = true;
	}

	protected override void Die()
	{
		if (_floatingTextScene == null) return;

		var textNode = _floatingTextScene.Instantiate<FloatingText>();

		// Set the text
		textNode.Text = $"+${_moneyDropped}";
		textNode.AddThemeColorOverride("font_color", Colors.Gold);

		// Set the position to the unit's current global position
		textNode.GlobalPosition = GlobalPosition;

		// VERY IMPORTANT: Add it to the world, not the unit!
		// If you add it to the unit, it will disappear instantly when the unit is freed.
		GetTree().Root.AddChild(textNode);
		base.Die();
	}
}

