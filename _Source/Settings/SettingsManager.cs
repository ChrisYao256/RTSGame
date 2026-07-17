using Godot;

public partial class SettingsManager : Node
{
	public static SettingsManager Instance { get; private set; }

	// Settings paths must use the 'user://' directory to ensure write access on all devices
	private const string SettingsFilePath = "user://settings.cfg";
	private readonly ConfigFile _configFile = new();

	// Default configuration values
	public float MasterVolume { get; set; } = 1.0f;
	public DisplayServer.WindowMode WindowMode { get; set; } = DisplayServer.WindowMode.Windowed;

	public override void _Ready()
	{
		Instance = this;
		LoadSettings();
	}

	public void SaveSettings()
	{
		// Store values in [Section] -> Key -> Value formatting
		_configFile.SetValue("Audio", "MasterVolume", MasterVolume);
		_configFile.SetValue("Video", "WindowMode", (int)WindowMode);

		Error err = _configFile.Save(SettingsFilePath);
		if (err != Error.Ok)
		{
			GD.PrintErr($"Failed to save settings: {err}");
		}
	}

	public void LoadSettings()
	{
		Error err = _configFile.Load(SettingsFilePath);

		// If the file doesn't exist yet (first-time launch), apply and save defaults
		if (err != Error.Ok)
		{
			SaveSettings();
			ApplySettings();
			return;
		}

		// Read values with safe fallback defaults
		MasterVolume = (float)_configFile.GetValue("Audio", "MasterVolume", 1.0f);
		WindowMode = (DisplayServer.WindowMode)(int)_configFile.GetValue("Video", "WindowMode", (int)DisplayServer.WindowMode.Windowed);

		ApplySettings();
	}

	public void ApplySettings()
	{
		// 1. Apply Window Mode
		DisplayServer.WindowSetMode(WindowMode);

		// 2. Apply Audio (Assuming you have a 'Master' bus configured)
		int masterBusIndex = AudioServer.GetBusIndex("Master");
		if (masterBusIndex != -1)
		{
			// Convert linear volume (0.0 to 1.0) to decibels
			float db = Mathf.LinearToDb(MasterVolume);
			AudioServer.SetBusVolumeDb(masterBusIndex, db);
		}
	}
}