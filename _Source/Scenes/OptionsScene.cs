using Godot;
using RTSGame.Units;
using System;
using System.Net.Sockets;
using static Godot.DisplayServer;

public partial class OptionsScene : Control
{
	[Export]
	public string MainMenuPath = "res://_Content/_Scenes/StartScene.tscn";

	public override void _Ready()
	{
		if (SettingsManager.Instance.WindowMode == WindowMode.Fullscreen)
		{
			GetNode<OptionButton>("VBoxContainer/FullscreenOptions").Selected = 0;
		}
		else if (SettingsManager.Instance.WindowMode == WindowMode.Windowed)
		{
			GetNode<OptionButton>("VBoxContainer/FullscreenOptions").Selected = 1;
		}
		else if (SettingsManager.Instance.WindowMode == WindowMode.ExclusiveFullscreen)
		{
			GetNode<OptionButton>("VBoxContainer/FullscreenOptions").Selected = 2;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public void SetFullscreen()
	{
		SettingsManager.Instance.WindowMode = DisplayServer.WindowMode.Fullscreen;
		SettingsManager.Instance.ApplySettings();
		SettingsManager.Instance.SaveSettings();
	}

	// Switch to standard Windowed mode
	public void SetWindowed()
	{
		SettingsManager.Instance.WindowMode = DisplayServer.WindowMode.Windowed;
		SettingsManager.Instance.ApplySettings();
		SettingsManager.Instance.SaveSettings();
	}

	// Switch to Borderless Fullscreen (often preferred for seamless alt-tabbing)
	public void SetBorderlessFullscreen()
	{
		SettingsManager.Instance.WindowMode = DisplayServer.WindowMode.ExclusiveFullscreen;
		SettingsManager.Instance.ApplySettings();
		SettingsManager.Instance.SaveSettings();
	}

	private void OnWindowModeSelected(long id)
	{
		switch (id)
		{
			case 0:
				SetFullscreen();
				break;

			case 1:
				SetWindowed();
				break;

			case 2:
				SetBorderlessFullscreen();
				break;
		}
	}

	public void OnReturnButtonPressed()
	{
		Error result = GetTree().ChangeSceneToFile(MainMenuPath);

		if (result != Error.Ok)
		{
			GD.PrintErr("Failed to load scene: " + MainMenuPath);
		}
	}
}
