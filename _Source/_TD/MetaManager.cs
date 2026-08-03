// MetaManager.cs
using Godot;
using Godot.Collections;
using RTSGame.Source;
using RTSGame.Units;
using System;
using System.Linq;
public partial class MetaManager : Node
{
	[Export] public Array<GlobalEffectResource> _allMetaUpgrades { get; set; } = new();

	public static MetaManager Instance { get; private set; }

	public Array<GlobalEffectResource> _obtainedMetaUpgrades = [];

	public Array<GlobalEffectResource> _remainingMetaUpgrades = [];

	public int _metaMoney { get; private set; } = 0;

	public int _wins { get; private set; } = 0;

	public int _losses { get; private set; } = 0;

	private const string SavePath = "user://metasave.json";

	public override void _Ready()
	{
		// Singleton pattern assignment for C# access
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
			return;
		}
		ProcessMode = ProcessModeEnum.Always;
		_remainingMetaUpgrades = _allMetaUpgrades.Duplicate();
		LoadMetaProgress();
	}

	public void UpdateMetaScene()
	{
		if (GetTree().CurrentScene is MetaScene metaScene)
		{
			metaScene.UpdateStats(_metaMoney, _wins, _losses, _remainingMetaUpgrades, _obtainedMetaUpgrades);
		}
	}

	public void UpdateMetaCurrency(int n)
	{
		_metaMoney = n;
		UpdateMetaScene();
		SaveMetaProgress();
	}

	public void IncreaseMoney(int amount)
	{
		UpdateMetaCurrency(_metaMoney + amount);
	}

	public void UpdateWins(int n)
	{
		_wins = n;
		UpdateMetaScene();
		SaveMetaProgress();
	}

	public void IncreaseWins(int n)
	{
		UpdateLosses(_wins + n);
	}

	public void UpdateLosses(int n)
	{
		_losses = n;
		UpdateMetaScene();
		SaveMetaProgress();
	}

	public void IncreaseLosses(int n)
	{
		UpdateLosses(_losses + n);
	}

	public void BuyMetaUpgrade(GlobalEffectResource resource)
	{
		if (_metaMoney >= resource._cost)
		{
			AddMetaUpgrade(resource);
			IncreaseMoney(-resource._cost);
		}
	}

	public void ClearMetaUpgrade()
	{
		_obtainedMetaUpgrades = [];
		_remainingMetaUpgrades = _allMetaUpgrades.Duplicate();
		UpdateMetaScene();
		SaveMetaProgress();
	}

	public void AddMetaUpgrade(GlobalEffectResource upgrade)
	{
		if (!_remainingMetaUpgrades.Contains(upgrade))
		{
			throw new Exception("upgrade not found");
		}
		_obtainedMetaUpgrades.Add(upgrade);
		_remainingMetaUpgrades.Remove(upgrade);
		UpdateMetaScene();
		SaveMetaProgress();
	}

	public void AddMetaUpgrade(int index)
	{
		AddMetaUpgrade(_allMetaUpgrades[index]);
	}

	public void DeleteSave()
	{
		if (FileAccess.FileExists(SavePath))
		{
			using var dir = DirAccess.Open("user://");

			if (dir != null)
			{
				Error result = dir.Remove("metasave.json");
				UpdateMetaCurrency(0);
				UpdateWins(0);
				UpdateLosses(0);
				ClearMetaUpgrade();

				if (result != Error.Ok)
				{
					GD.Print($"Failed to delete save file. Error code {result}");
				}
			}
		}
		else
		{
			GD.Print("No save file exists to delete.");
		}
	}

	// --- SAVE / LOAD LOGIC ---

	public void SaveMetaProgress()
	{
		// Pack the data using your defined method
		Dictionary<string, Variant> dataToSave = PackMetaGameData(_metaMoney, _wins, _losses, _obtainedMetaUpgrades);
		// Convert the dictionary into a clean JSON text string
		string jsonString = Json.Stringify(dataToSave);

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		if (file != null)
		{
			file.StoreString(jsonString);
			GD.Print("Game state successfully saved!");
		}
		else
		{
			GD.PrintErr($"Failed to write save file. Error: {FileAccess.GetOpenError()}");
		}
	}

	private Dictionary<string, Variant> PackMetaGameData(int metaMoney, int wins, int losses, Array<GlobalEffectResource> obtainedUpgrades)
	{
		var gameData = new Dictionary<string, Variant>
				{
						{ "Money", metaMoney },
						{ "Wins", wins },
						{ "Losses", losses },
				};

		var effectsList = new Array<int>();

		foreach (GlobalEffectResource effect in obtainedUpgrades)
		{
			effectsList.Add(_allMetaUpgrades.IndexOf(effect));
		}

		gameData.Add("MetaUpgrades", effectsList);

		return gameData;
	}

	public void LoadMetaProgress()
	{
		if (!FileAccess.FileExists(SavePath))
		{
			GD.Print("No save game data found. Starting fresh layout.");
			return;
		}

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();

		Json json = new Json();
		Error error = json.Parse(jsonString);

		if (error != Error.Ok)
		{
			GD.PrintErr($"JSON Parsing failed on line {json.GetErrorLine()}: {json.GetErrorMessage()}");
			return;
		}

		var gameData = (Dictionary<string, Variant>)json.Data;

		// --- EXTRATING AND CONVERTING THE SAVED DATA ---

		// Convert the JSON variant back into a proper Godot Vector4I
		int loadedMoney = (int)gameData["Money"];
		int loadedWins = (int)gameData["Wins"];
		int loadedLosses = (int)gameData["Losses"];

		var loadedGlobalEffects = (Array<int>)gameData["MetaUpgrades"];

		// Send everything off to be reconstructed in your main match loop
		ApplyLoadedData(loadedMoney, loadedWins, loadedLosses, loadedGlobalEffects);
	}

	public bool HasSavedGame()
	{
		return FileAccess.FileExists(SavePath);
	}

	// 3. Spawning / Applying Method
	private void ApplyLoadedData(int money, int wins, int losses, Array<int> metaUpgrades)
	{
		UpdateMetaCurrency(money);
		UpdateWins(wins);
		UpdateLosses(losses);

		foreach (int resourceIndex in metaUpgrades)
		{
			AddMetaUpgrade(resourceIndex);
		}
	}
}