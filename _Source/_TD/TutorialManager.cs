using Godot;
using Godot.Collections;
using RTSGame.Source;
using RTSGame.Units;
using System;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

public partial class TutorialManager : Control
{
	public static Dictionary<int, Array<string>> TutorialTexts = new Dictionary<int, Array<string>>()
	{
		{ 0, [
			"Welcome to your onboarding. Click anywhere on the screen to continue.",

			"Look here. This highlighted portal is projected to deliver two [i]Slime[/i] invaders next cycle. ",

			"They will migrate toward our HQ if left uncontested, resulting in non-budgeted infrastructure degradation. ",

			"As the sole human member of the Project team, the responsibility of defending mission-critical infrastructure falls to you.",

			"Construct a defensive tower by dragging it from the panel on the right onto the field. ",

			"Building defensive towers requires resources. They are displayed on the top right of your interface. ",

			"The Project team recognizes four types of resources, " +
			"[img=36x36]res://_Assets/Electricity.png[/img], " +
			"[img=36x36]res://_Assets/Steel.png[/img], " +
			"[img=36x36]res://_Assets/Water.png[/img], and " +
			"[img=36x36]res://_Assets/Gas.png[/img]. " +
			"Unfortunately, under the current fiscal constraints, we only have 100 [img=36x36]res://_Assets/Electricity.png[/img].",

			"Nonetheless, there are two types of defenses that we can afford: the [i]Gunner[/i] and the [i]Piercer[/i].",

			"Press [i]Next Cycle[/i] once you have constructed a satisfactory number of these defenses.",
			] },

		{1, [
			"Neutralizing the target [i]Slimes[/i] yielded a total of 4 [img=36x36]res://_Assets/Electricity.png[/img].",

			"This rate of revenue generation falls significantly short of our quarter milestones and will not satisfy compliance standards for the upcoming [i]Inspection[/i]",

			"To proactively mitigate this compliance risk, direct your attention to the [i]Portals[/i] tab on the right hand panel. Press it to switch from building defenses to building portals.",

			"By building more portals, you increase the Project's potential income, which you can view here.",

			"Note that it is only through successfully neutralizing invaders that you realize your potential income, so be sure to scale your defenses in accordance with invaders strength.",

			"One last note: our portal logistic grid can only support so many portals. It is, however, possible to increase our portal capacity by investing progressively more [img=36x36]res://_Assets/Electricity.png[/img].",

			"Press [i]Next Cycle[/i] when you reconciled your risk-to-reward ratio.",
			] },

		{2, [
			"Clicking on constructed portals and defenses brings up detailed analytics on the lower panel. You can also upgrade portals and defenses here to improve their effectiveness.",

			"Many defenses and invaders possess unique abilities, which will be displayed on this panel as well.",

			"For your convenience, I have been instructed to give the following advice: ",
			
			"[b]Upgrading defenses will almost always deliver a higher DPS per investment compared to building duplicate defenses. Conversely, building more portals yields a higher income per Hp ratio than upgrading portals.[/b]"

			] },

		{3, [
			"We rapidly approaching our first [i]Inspection[/i], as can be viewed here.",

			"At the start of an [i]Inspection[/i] cycle, an authorized inspector appears at the entry point. They will then attempt to move to our HQ to assess our operational viability . ",

			"Our department's performance rating is strictly determined by the distance the inspector travels before they are neutralized. If an inspector reaches our HQ, our current project would be forced to terminate immediately.",

			"Inspectors all possess extraordinary abilities and endurance. They pose a significant challenge to the continuity of our project. Therefore, we should aggressively maximize our revenue stream early and ensure we have sufficient firepower on [i]Inspection[/i] cycles.",

			] },

		{5, [
			"Congratulations. We have successfully passed our first [i]Inspection[/i]. ",

			"However, there are more [i]Inspections[/i] to come featuring progressively more demanding inspectors. Consequently, we must continue to operate at maximum capacity by constructing or upgrading portals and using reinvesting our revenue into stronger defenses.",
			
			"This concludes your onboarding. ",

			"Disclaimer: this onboarding module is not scoped to address the granular specifications of each invader type or defense type. The user is expected to self-direct their professional development through independent field research.\". "
		] }
	};

	private TooltipRichTextLabel _text;
	private HolePunchOverlay _darkOverlay;

	private TDManager _tdManager;
	private Grid _grid;
	private UnitManager _unitManager;

	private int _localTextIndex;
	private int _localWaveIndex;

	public bool _active = false;

	public void Initialize(TDManager tdManager, Grid grid, UnitManager unitManager)
	{
		_tdManager = tdManager;
		_grid = grid;
		_unitManager = unitManager;
		Visible = true;
		_darkOverlay = GetNode<HolePunchOverlay>("DarkOverlay");
		_text = GetNode<TooltipRichTextLabel>("Panel/MarginContainer/HBoxContainer/RichTextLabel");
		_text.BbcodeEnabled = true;
		_text.FitContent = true;
		_text.CustomMinimumSize = new(1200,0);
		_active = true;

		_tdManager._towerManager.PlaceTower(new Vector2I(2, 2), "SlimeSpawner");

		AdvanceText();
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (!_active) return;

		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			AdvanceText();
			GetViewport().SetInputAsHandled(); // Consume the click so towers aren't clicked
		}
	}

	public void AdvanceText()
	{
		if (!TutorialTexts.ContainsKey(_localWaveIndex) ||_localTextIndex >= TutorialTexts[_localWaveIndex].Count)
		{
			HideTutorial();
			return;
		}
		string text = TutorialTexts[_localWaveIndex][_localTextIndex];
		_text.Text = text;
		_text._Ready();
		SetHighlightArea();
		_localTextIndex++;
	}
	
	public void SetHighlightArea()
	{
		if (_localWaveIndex == 0 && _localTextIndex == 1)
		{
			_darkOverlay.SetHighlightArea(_grid.GetGlobalTileRect(new Vector2I(2,2)));
		}
		else if (_localWaveIndex == 0 && _localTextIndex == 2)
		{
			_darkOverlay.SetHighlightArea(_grid.GetGlobalTileRect(_grid.GetExitLocation()));
		}
		else if (_localWaveIndex == 0 && _localTextIndex == 3)
		{
			_darkOverlay.SetHighlightArea(_grid.GetGlobalTileRect(_grid.GetExitLocation()));
		}
		else if (_localWaveIndex == 0 && _localTextIndex == 4)
		{
			_darkOverlay.SetHighlightArea(_tdManager._rightPanel.GetNode<GridContainer>("Towers").GetGlobalRect());
		}
		else if (_localWaveIndex == 0 && _localTextIndex == 5)
		{
			_darkOverlay.SetHighlightArea(_tdManager._rightPanel.GetNode<RichTextLabel>("MoneyLabel").GetGlobalRect());
		}
		else if (_localWaveIndex == 0 && _localTextIndex == 6)
		{
			_darkOverlay.SetHighlightArea(_tdManager._rightPanel.GetNode<RichTextLabel>("MoneyLabel").GetGlobalRect());
		}
		else if (_localWaveIndex == 0 && _localTextIndex == 7)
		{
			_darkOverlay.SetHighlightArea(_tdManager._rightPanel.GetNode<GridContainer>("Towers").GetGlobalRect());
		}
		else if (_localWaveIndex == 0 && _localTextIndex == 8)
		{
			_darkOverlay.SetHighlightArea(_tdManager._rightPanel.GetNode<Button>("NextWave").GetGlobalRect());
		}
		//else if (_localWaveIndex == 1 && _localTextIndex == 1)
		//{
		//	TowerUnit defensiveTower = _tdManager._towerManager._allTowers.First(o => o._towerType == TowerUnit.TowerType.Defense);
		//	_darkOverlay.SetHighlightArea(_grid.GetGlobalTileRect(defensiveTower._gridLocation));
		//}
		//else if (_localWaveIndex == 1 && _localTextIndex == 1)
		//{
		//	TowerUnit spawner = _tdManager._towerManager._allTowers.First(o => o._towerType == TowerUnit.TowerType.Spawner);
		//	_darkOverlay.SetHighlightArea(_grid.GetGlobalTileRect(spawner._gridLocation));
		//}
		else if (_localWaveIndex == 1 && _localTextIndex == 2)
		{
			_darkOverlay.SetHighlightArea(_tdManager._rightPanel.GetNode<Button>("Tabs/SpawnerTab").GetGlobalRect());
		}
		else if (_localWaveIndex == 1 && _localTextIndex == 3)
		{
			_darkOverlay.SetHighlightArea(_tdManager._rightPanel.GetNode<RichTextLabel>("IncomeLabel").GetGlobalRect());
		}
		else if (_localWaveIndex == 1 && _localTextIndex == 5)
		{
			_darkOverlay.SetHighlightArea(_tdManager._rightPanel.GetNode<VBoxContainer>("HBoxContainer").GetGlobalRect());
		}
		else if (_localWaveIndex == 3 && _localTextIndex == 0)
		{
			_darkOverlay.SetHighlightArea(_tdManager._rightPanel.GetNode<Label>("BossWaveCounter").GetGlobalRect());
		}
		else if (_localWaveIndex == 3 && _localTextIndex == 1)
		{
			_darkOverlay.SetHighlightArea(_grid.GetGlobalTileRect(_grid.GetEntrancePosition()));
		}
		else
		{
			_darkOverlay.SetHighlightArea(null);
		}
	}

	public void NextWave()
	{
		_localWaveIndex++;
		_localTextIndex = 0;
		AdvanceText();
	}

	public void ShowTutorial()
	{
		Visible = true;
	}

	public void HideTutorial()
	{
		Visible = false;
	}
}