using Godot;
using Godot.Collections;
using RTSGame.Source;
using RTSGame.Units;
using System;
using System.Net.Sockets;
using System.Resources;

public partial class MetaScene : Node2D
{
	[Export]
	public string MainMenuPath = "res://_Content/_Scenes/StartScene.tscn";

	private TooltipRichTextLabel _moneyLabel;
	private TooltipRichTextLabel _winsLabel;
	private TooltipRichTextLabel _lossesLabel;
	private GridContainer _availablePassivesGrid;
	private GridContainer _obtainedPassivesGrid;

	private SpinBox _moneyInput;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_moneyLabel = GetNode<TooltipRichTextLabel>("PanelContainer/HBoxContainer/Money");
		_winsLabel = GetNode<TooltipRichTextLabel>("PanelContainer/HBoxContainer/Wins");
		_lossesLabel = GetNode<TooltipRichTextLabel>("PanelContainer/HBoxContainer/Losses");

		_availablePassivesGrid = GetNode<GridContainer>("PanelContainer2/VBoxContainer/PanelContainer/VBoxContainer/AvailablePassives");
		_obtainedPassivesGrid = GetNode<GridContainer>("PanelContainer2/VBoxContainer/PanelContainer2/VBoxContainer/ObtainedPassives");

		_moneyInput = GetNode<SpinBox>("SpinBox");

		// Configure for integer input
		_moneyInput.MinValue = 0;
		_moneyInput.MaxValue = 9999;
		_moneyInput.Step = 1;
		_moneyInput.Rounded = true; // Forces integer values

		// Listen for value changes
		_moneyInput.ValueChanged += (double newValue) =>
		{
			MetaManager.Instance.UpdateMetaCurrency((int)newValue);
		};

		MetaManager.Instance.UpdateMetaScene();
	}

	public void UpdateStats(int money, int wins, int losses, Array<GlobalEffectResource> availableUpgrades, Array<GlobalEffectResource> obtainedUpgrades)
	{
		_moneyLabel.Text = $"${money}";
		_winsLabel.Text = $"Wins: {wins}";
		_lossesLabel.Text = $"Losses: {losses}";

		_moneyInput.SetValueNoSignal(money);

		foreach (var node in _availablePassivesGrid.GetChildren())
		{
			node.QueueFree();
		}

		if (availableUpgrades.Count <= 8 && availableUpgrades.Count > 0)
		{
			_availablePassivesGrid.Columns = availableUpgrades.Count;
		}
		else
		{
			_availablePassivesGrid.Columns = 8;
		}
		foreach (GlobalEffectResource resource in availableUpgrades)
		{
			VBoxContainer container = new VBoxContainer();

			resource.SetDescription();

			HoverInfoImage effectButton
				= resource.MakeEffectTooltip(true);
			effectButton.Pressed += (() =>
			{
				MetaManager.Instance.BuyMetaUpgrade(resource);
			});
			effectButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

			container.AddChild(effectButton);

			TooltipRichTextLabel costLabel = new TooltipRichTextLabel();
			costLabel.Text = $"${resource._cost}";
			costLabel.CustomMinimumSize = new(50, 0);
			costLabel.FitContent = true;
			costLabel.HorizontalAlignment = HorizontalAlignment.Center;
			container.AddChild(costLabel);

			_availablePassivesGrid.AddChild(container);
		}

		foreach (var node in _obtainedPassivesGrid.GetChildren())
		{
			node.QueueFree();
		}

		if (obtainedUpgrades.Count <= 8 && obtainedUpgrades.Count > 0)
		{
			_obtainedPassivesGrid.Columns = obtainedUpgrades.Count;
		}
		else
		{
			_obtainedPassivesGrid.Columns = 8;
		}
		foreach (GlobalEffectResource resource in obtainedUpgrades)
		{
			VBoxContainer container = new VBoxContainer();

			resource.SetDescription();

			HoverInfoImage effectButton
				= resource.MakeEffectTooltip(true);
			effectButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

			container.AddChild(effectButton);

			_obtainedPassivesGrid.AddChild(container);
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

	public void OnDeleteButtonPressed()
	{
		MetaManager.Instance.DeleteSave();
	}
}
