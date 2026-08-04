using Godot;
using Godot.Collections; // Required for Godot.Collections.Dictionary
using RTSGame.Source;
using RTSGame.Units;
using System;
using System.Linq;
using System.Reflection;

public partial class SaveManager : Node
{
	// The path where the save file will live. 
	// "user://" maps to a safe, persistent folder provided by the OS.
	private const string SavePath = "user://savegame.json";

	public static SaveManager Instance = new SaveManager();

	public TDManager _tdManager;

	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
			return;
		}
	}

	// A helper method to pack your current game data into a Godot Dictionary
	public Dictionary<string, Variant> PackGameData(GameGlobals.GameMode gameMode,Vector4I money, int portalLimit, int hp, int waveCount, Array<TowerUnit> towers, Array<string> unlockedTowers, Array<GlobalEffectResource> globalEffects, Array<InvaderStatsIncreaseResource> finalBoss, LevelResource level, int challengeCount, int inspectionFailedCount, Array<InvaderStatsIncreaseResource> nextChallengeUnits)
	{

		var gameData = new Dictionary<string, Variant>
				{
						{ "GameMode", (int)gameMode },
						{ "Money", money },
						{ "PortalLimit", portalLimit },
			      { "WaveCount", waveCount},
						{ "Hp", hp},
						{ "FinalWave", level._finalWave},
						{ "Map", level._mapID},
						{ "InspectionFailedCount", inspectionFailedCount},
						{ "ChallengeCount", challengeCount},
						{ "InspectionInterval", level._inspectionInterval},

						{ "ChallengeEnabled", level._challengeEnabled},
						{ "InspectionEnabled", level._inspectionEnabled},
						{ "PortalsEnabled", level._portalsEnabled},
				};

		var finalBossNameLevel = new Array<Array<Variant>>();

		foreach (InvaderStatsIncreaseResource unit in finalBoss)
		{
			Array<Variant> array = new Array<Variant>();
			array.Add(unit._unitName);
			array.Add(unit._level);
			finalBossNameLevel.Add(array);
		}

		gameData.Add("FinalBoss", finalBossNameLevel);

		var nextChallengeUnitsNameLevel = new Array<Array<Variant>>();
		foreach (InvaderStatsIncreaseResource unit in nextChallengeUnits)
		{
			Array<Variant> array = new Array<Variant>();
			array.Add(unit._unitName);
			array.Add(unit._level);
			nextChallengeUnitsNameLevel.Add(array);
		}

		gameData.Add("NextChallenge", nextChallengeUnitsNameLevel);

		// We will store the towers as an array of individual tower dictionaries
		var towerList = new Array<Dictionary<string, Variant>>();

		foreach (TowerUnit tower in towers)
		{
			var towerData = new Dictionary<string, Variant>
								{
										{ "Name", tower._internalName },
										{ "GridX", tower._gridLocation.X },
										{ "GridY", tower._gridLocation.Y },
										{ "Level", tower.GetTowerLevel() },
										{ "Cost0", tower._cost[0]},
										{ "Cost1", tower._cost[1]},
										{ "Cost2", tower._cost[2]},
										{ "Cost3", tower._cost[3]},
										{ "Priority", (int)tower._targetPriority },
								};
			towerList.Add(towerData);
		}

		gameData.Add("Towers", towerList);

		var effectsList = new Array<int>();

		foreach (GlobalEffectResource effect in globalEffects)
		{
			effectsList.Add(_tdManager._allGlobalEffects.IndexOf(effect));
		}

		gameData.Add("GlobalEffects", effectsList);

		gameData.Add("UnlockedTowers", unlockedTowers);

		return gameData;
	}

	public void SaveGame(GameGlobals.GameMode gameMode, Vector4I money, int portalLimit, int hp,int waveCount, Array<TowerUnit> currentTowers, Array<string> unlockedTowers, Array<GlobalEffectResource> globalEffects, Array<InvaderStatsIncreaseResource>finalBoss, LevelResource level, int challengeCount, int inspectionFailedCount, Array<InvaderStatsIncreaseResource> nextChallengeUnits)
	{
		// Pack the data using your defined method
		Dictionary<string, Variant> dataToSave = PackGameData(gameMode, money, portalLimit, hp, waveCount, currentTowers, unlockedTowers, globalEffects, finalBoss, level, challengeCount, inspectionFailedCount, nextChallengeUnits);
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

	public void DeleteSave()
	{
		if (FileAccess.FileExists(SavePath))
		{
			using var dir = DirAccess.Open("user://");

			if (dir != null)
			{
				Error result = dir.Remove("savegame.json");

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

	public void LoadGame()
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

		GameGlobals.GameMode gameMode = (GameGlobals.GameMode)(int)gameData["GameMode"];

		// Convert the JSON variant back into a proper Godot Vector4I
		Vector4I loadedMoney = VariantToVector4I(gameData["Money"]);

		int loadedPortalLimit = (int)gameData["PortalLimit"];
		int loadedWaveCount = (int)gameData["WaveCount"];
		int loadedHp = (int)gameData["Hp"];
		int loadedMap = (int)gameData["Map"];
		int loadedFinalWave = (int)gameData["FinalWave"];
		int loadedInspectionFailedCount = (int)gameData["InspectionFailedCount"];
		int loadedChallengeCount = (int)gameData["ChallengeCount"];

		bool loadedChallengeEnabled = (bool)gameData["ChallengeEnabled"];
		bool loadedInspectionEnabled = (bool)gameData["InspectionEnabled"];
		bool loadedPortalsEnabled = (bool)gameData["PortalsEnabled"];

		var loadedFinalBoss = (Array<Array<Variant>>)gameData["FinalBoss"];
		var loadedNextChallenge = (Array<Array<Variant>>)gameData["NextChallenge"];

		// Extract the nested tower array
		var loadedTowers = (Array<Dictionary<string, Variant>>)gameData["Towers"];

		var loadedUnlockedTowers = (Array<string>)gameData["UnlockedTowers"];

		var loadedGlobalEffects = (Array<int>)gameData["GlobalEffects"];


		// Send everything off to be reconstructed in your main match loop
		// Send everything off to be reconstructed in your main match loop
		ApplyLoadedData(gameMode, loadedMoney, loadedPortalLimit, loadedHp, loadedMap, loadedWaveCount, loadedTowers, loadedUnlockedTowers, loadedGlobalEffects, loadedFinalWave, loadedInspectionFailedCount, loadedChallengeCount, loadedChallengeEnabled, loadedInspectionEnabled, loadedPortalsEnabled, loadedFinalBoss, loadedNextChallenge);
	}

	public bool HasSavedGame()
	{
		return FileAccess.FileExists(SavePath);
	}

	public LevelResource GetLoadedLevelResource()
	{
		if (!FileAccess.FileExists(SavePath))
		{
			throw new Exception("No Save Found");
		}

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();

		Json json = new Json();
		Error error = json.Parse(jsonString);

		if (error != Error.Ok)
		{
			throw new Exception("Failed to parse");
		}

		var gameData = (Dictionary<string, Variant>)json.Data;

		int loadedMap = (int)gameData["Map"];
		int loadedFinalWave = (int)gameData["FinalWave"];
		int loadedInspectionInterval = (int)gameData["InspectionInterval"];

		bool loadedChallengeEnabled = (bool)gameData["ChallengeEnabled"];
		bool loadedInspectionEnabled = (bool)gameData["InspectionEnabled"];
		bool loadedPortalsEnabled = (bool)gameData["PortalsEnabled"];

		var loadedFinalBoss = (Array<Array<Variant>>)gameData["FinalBoss"];

		LevelResource levelResource = new LevelResource();
		levelResource._finalBoss = new();
		levelResource._finalBoss._units = [];
		foreach (Array<Variant> nameLevel in loadedFinalBoss)
		{
			InvaderStatsIncreaseResource unit = new();
			unit._unitName = (string)nameLevel[0];
			unit._level = (int)nameLevel[1];
			levelResource._finalBoss._units.Add(unit);
		}
		levelResource._finalWave = loadedFinalWave;
		levelResource._inspectionInterval = loadedInspectionInterval;
		levelResource._mapID = loadedMap;

		levelResource._challengeEnabled = loadedChallengeEnabled;
		levelResource._portalsEnabled = loadedPortalsEnabled;
		levelResource._inspectionEnabled = loadedInspectionEnabled;
		return levelResource;
	}

	// 3. Spawning / Applying Method
	private void ApplyLoadedData(
		GameGlobals.GameMode gameMode,
		Vector4I money,
		int portalLimit,
		int hp,
		int map, 
		int waveCount,
		Array<Dictionary<string, Variant>> towers,
		Array<string> loadedUnlockedTowers,
		Array<int> loadedGlobalEffects,
		int finalWave,
		int inspectionFailedCount,
		int challengeCount,
		bool challengeEnabled,
		bool inspectionEnabled,
		bool portalsEnabled,
		Array<Array<Variant>> finalBoss,
		Array<Array<Variant>> nextChallenge
	)
	{
		_tdManager._gameMode = gameMode;
		_tdManager.UpdateMoney(money);
		_tdManager.UpdateHp(hp);
		_tdManager.IncreaseSpawnerLimit(portalLimit);
		_tdManager._waveIndex = waveCount;
		_tdManager.UpdateWaveIndexCounter();
		_tdManager.UpdatePortalLimitButtonText(); 
		_tdManager._availTowerList = loadedUnlockedTowers;
		_tdManager._challengeCount = challengeCount;
		_tdManager._inspectionFailedCount = inspectionFailedCount;

		_tdManager._nextChallengeUnits = [];
		foreach (Array<Variant> nameLevel in nextChallenge)
		{
			InvaderStatsIncreaseResource unit = new();
			unit._unitName = (string)nameLevel[0];
			unit._level = (int)nameLevel[1];
			_tdManager._nextChallengeUnits.Add(unit);
		}

		foreach (var towerData in towers)
		{
			string name = (string)towerData["Name"];
			int gridX = (int)towerData["GridX"];
			int gridY = (int)towerData["GridY"];
			int level = (int)towerData["Level"];
			int cost0 = (int)towerData["Cost0"];
			int cost1 = (int)towerData["Cost1"];
			int cost2 = (int)towerData["Cost2"];
			int cost3 = (int)towerData["Cost3"];
			TowerUnit.TargetPriority priority = (TowerUnit.TargetPriority)(int)towerData["Priority"];

			// Your Spawner/Factory Logic should take over here:
			// e.g., SpawnTowerAtGrid(name, new Vector2I(gridX, gridY), level);
			TowerUnit tower = _tdManager._towerManager.PlaceTower(new(gridX, gridY), name);

			tower._cost = new Vector4I(cost0, cost1, cost2, cost3);

			tower._targetPriority = priority;

			if (level == 2)
			{
				tower.UpgradeFirst();
			}
			else if (level == 3)
			{
				tower.UpgradeFirst();
				tower.UpgradeSecond();
			}
			else if (level == 4)
			{
				tower.UpgradeFirst();
				tower.UpgradeSecond();
				tower.UpgradeThird();
			}
		}

		foreach (int resourceIndex in loadedGlobalEffects)
		{
			_tdManager.ApplyGlobalEffect(resourceIndex, true);
		}
	}

	// Helper method to safely convert JSON-parsed strings/variants back into Vector4I
	private Vector4I VariantToVector4I(Variant variantValue)
	{
		if (variantValue.VariantType == Variant.Type.String)
		{
			// Converts string format like "(100, 50, 0, 10)" safely back to vector values
			string cleaned = ((string)variantValue).Replace("(", "").Replace(")", "").Replace(" ", "");
			string[] components = cleaned.Split(',');

			if (components.Length == 4)
			{
				return new Vector4I(
						components[0].ToInt(),
						components[1].ToInt(),
						components[2].ToInt(),
						components[3].ToInt()
				);
			}
		}

		// Return default/zeroed vector if format is unexpected or broken
		return Vector4I.Zero;
	}
}