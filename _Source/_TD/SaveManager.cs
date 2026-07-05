using Godot;
using Godot.Collections; // Required for Godot.Collections.Dictionary
using RTSGame.Source;
using RTSGame.Units;

public partial class SaveManager : Node
{
	// The path where the save file will live. 
	// "user://" maps to a safe, persistent folder provided by the OS.
	private const string SavePath = "user://savegame.json";

	public TDManager _tdManager;

	// A helper method to pack your current game data into a Godot Dictionary
	public Dictionary<string, Variant> PackGameData(Vector4I money, int portalLimit, int hp, int waveCount, Array<TowerUnit> towers)
	{
		var gameData = new Dictionary<string, Variant>
				{
						{ "Money", money },
						{ "PortalLimit", portalLimit },
			      { "WaveCount", waveCount},
						{ "Hp", hp}
				};

		// We will store the towers as an array of individual tower dictionaries
		var towerList = new Array<Dictionary<string, Variant>>();

		foreach (TowerUnit tower in towers)
		{
			var towerData = new Dictionary<string, Variant>
								{
										{ "Name", tower._internalName },
										{ "GridX", tower._gridLocation.X },
										{ "GridY", tower._gridLocation.Y },
										{ "Level", tower.GetTowerLevel() }
								};
			towerList.Add(towerData);
		}

		gameData.Add("Towers", towerList);
		return gameData;
	}

	public void SaveGame(Vector4I money, int portalLimit, int hp,int waveCount, Array<TowerUnit> currentTowers)
	{
		// Pack the data using your defined method
		Dictionary<string, Variant> dataToSave = PackGameData(money, portalLimit, hp, waveCount, currentTowers);

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

		// Convert the JSON variant back into a proper Godot Vector4I
		Vector4I loadedMoney = VariantToVector4I(gameData["Money"]);

		int loadedPortalLimit = (int)gameData["PortalLimit"];
		int loadedWaveCount = (int)gameData["WaveCount"];
		int loadedHp = (int)gameData["Hp"];

		// Extract the nested tower array
		var loadedTowers = (Array<Dictionary<string, Variant>>)gameData["Towers"];

		// Send everything off to be reconstructed in your main match loop
		ApplyLoadedData(loadedMoney, loadedPortalLimit, loadedHp, loadedWaveCount, loadedTowers);
	}

	// 3. Spawning / Applying Method
	private void ApplyLoadedData(Vector4I money, int portalLimit, int hp, int waveCount, Array<Dictionary<string, Variant>> towers)
	{
		_tdManager.UpdateMoney(money);
		_tdManager.UpdateHp(hp);
		_tdManager.IncreaseSpawnerLimit(portalLimit);
		_tdManager._waveIndex = waveCount;
		_tdManager.UpdateWaveIndexCounter();
		_tdManager.UpdatePortalLimitButtonText(); 

		foreach (var towerData in towers)
		{
			string name = (string)towerData["Name"];
			int gridX = (int)towerData["GridX"];
			int gridY = (int)towerData["GridY"];
			int level = (int)towerData["Level"];

			// Your Spawner/Factory Logic should take over here:
			// e.g., SpawnTowerAtGrid(name, new Vector2I(gridX, gridY), level);
			TowerUnit tower = _tdManager._towerManager.PlaceTower(new(gridX, gridY), name);
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