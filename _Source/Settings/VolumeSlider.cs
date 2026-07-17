using Godot;

public partial class VolumeSlider : HSlider
{
	// The name of the audio bus this slider controls (e.g., "Master", "Music", "SFX")
	[Export] public string BusName { get; set; } = "Master";

	private int _busIndex;

	public override void _Ready()
	{
		if (SettingsManager.Instance != null)
		{
			Value = SettingsManager.Instance.MasterVolume;
		}

		// 1. Find the target audio bus index
		_busIndex = AudioServer.GetBusIndex(BusName);

		if (_busIndex == -1)
		{
			GD.PrintErr($"Audio bus '{BusName}' not found!");
			return;
		}

		// 2. Set the slider's initial position to match the current engine volume
		float currentDb = AudioServer.GetBusVolumeDb(_busIndex);
		Value = Mathf.DbToLinear(currentDb);

		// 3. Connect the built-in value_changed signal to our method
		ValueChanged += OnVolumeValueChanged;
	}

	private void OnVolumeValueChanged(double value)
	{
		if (_busIndex == -1) return;

		// Convert the linear slider value (0.0 to 1.0) into logarithmic decibels
		float dbValue = Mathf.LinearToDb((float)value);

		// Apply the decibel adjustment to the audio server
		AudioServer.SetBusVolumeDb(_busIndex, dbValue);

		// Optional: Save this setting immediately if integrating with a manager
		SettingsManager.Instance.MasterVolume = (float)value;
		SettingsManager.Instance.SaveSettings();
	}
}