using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace RTSGame.Source;

public partial class TDManager : Node
{
	[Signal]
	public delegate void NewWaveEventHandler();

	[Signal]
	public delegate void GlobalPlacedTowerEventHandler(TowerUnit tower);

	[Signal]
	public delegate void GlobalBeforeDamageEventHandler(DamageContext damageContext);

	[Signal]
	public delegate void GlobalAddEffectEventHandler(AddEffectContext addEffectContext);

	[Signal]
	public delegate void GlobalUnitKilledEventHandler(DamageContext damageContext);

	[Signal]
	public delegate void GlobalExplosionEventHandler(ExplosionContext explosionContext);

	[Signal]
	public delegate void GlobalBossSpawnedEventHandler();

	[Export]
	public Array<PackedScene> _allMaps;

	[Export]
	public Array<string> _allTowerList;

	public string MenuPath = "res://_Content/_Scenes/StartScene.tscn";

	public Array<string> _availTowerList;

	[Export]
	public Array<SpawnerDataResource> _earlyBosses;

	[Export]
	public Array<SpawnerDataResource> _midBosses;

	[Export]
	public Array<SpawnerDataResource> _lateBosses;

	[Export]
	public Array<SpawnerDataResource> _inspectionBosses;

	[Export]
	public Array<SpawnerDataResource> _finalBosses;

	[Export]
	public Vector4I _startingMoney = new Vector4I(50, 0, 0, 0);

	[Export]
	private int _startingSpawnerLimit = 5;

	[Export]
	private int _midBossStartsAt = 2;

	[Export]
	private int _lateBossStartsAt = 4;

	[Export]
	public Array<GlobalEffectResource> _allGlobalEffects { get; private set; }

	public static Array<int> _inspectionRequirements = [450, 800, 1500, 3000];

	public LevelResource _level;

	public InvaderStatsIncreaseResource _finalBossBuff;

	public InvaderStatsIncreaseResource _miniBossBuff;

	public float _bossHpMultiplier = 1.3f;

	/// <summary>
	/// Time between successive enemy spawns from entrance in the same wave. 
	/// </summary>
	public const float SpawnInterval = 1f; 
	public const float TileSize = 120f;

	public Godot.Collections.Dictionary<int, int> _inspectionList = [];
	public Godot.Collections.Dictionary<int, Array<InvaderStatsIncreaseResource>> _waveList = [];
	public Godot.Collections.Dictionary<int, Array<RewardManager.RewardType>> _rewardList = [];
	public Array<InvaderStatsIncreaseResource> _finalBoss;
	public int _waveIndex = 0;

	private UnitManager _unitManager;
	public TDTowerManager _towerManager;
	private TutorialManager _tutorialManager;
	private SaveManager _saveManager;
	private RewardManager _rewardManager;

	public VBoxContainer _rightPanel;
	public UnitInfoPanel _infoPanel;
	private Label _hpLabel;
	private TooltipRichTextLabel _yellowLabel;
	private TooltipRichTextLabel _redLabel;
	private TooltipRichTextLabel _blueLabel;
	private TooltipRichTextLabel _greenLabel;
	private Grid _grid;
	private Label _waveCounter;
	private TextureProgressBar _progressionBar;
	private StripeManager _progressionBarStripes;
	private Label _bossWaveLabel;
	private TooltipRichTextLabel _spawnerLimitLabel;
	public UpgradeButton _spawnerLimitIncreaseButton;
	private TooltipRichTextLabel _spawnerLimitIncreaseButtonText;
	private CanvasLayer _fullscreenOverlay;
	private GridContainer _globalEffectsContainer;
	private CanvasLayer _inspectionLayer;
	private TooltipRichTextLabel _inspectionRequirementLabel;
	private TooltipRichTextLabel _inspectionSummaryLabel;
	private Button _inspectionDoneButton;
	private UpgradeButton _challengeButton;
	private TooltipRichTextLabel _challengeButtonLabel;
	private Label _inspectionLabel;
	private TextureProgressBar _inspectionProgressBar;

	private Exit _exit;

	private int _aliveInvaderCount;
	private int _leakedInvaderCount;
	private bool _waveEndProcessed = false;
	private bool _inspectionProcessed = false;

	private Array<SpawnerDataResource> _remainingBosses = [];

	public Array<GlobalEffectResource> _globalEffects = [];

	public GameGlobals.GameMode _gameMode;
	public int _hp;
	public Vector4I _money;
	private int _spawnerLimit;
	private int _spawnerCount;
	public int _challengeCount;
	public int _inspectionFailedCount;
	public Array<InvaderStatsIncreaseResource> _nextChallengeUnits;
	public Array<RewardManager.RewardType> _currentWaveRewards;
	public bool _inspectionWaveOnGoing;

	public override void _Ready()
	{
		_rightPanel = GetParent().GetNode("RightPanelCanvasLayer").GetNode("RightPanel").GetNode<VBoxContainer>("VBoxContainer").GetNode<HBoxContainer>("HBoxContainer").GetNode<VBoxContainer>("VBoxContainer");

		_yellowLabel = GetParent().GetNode("RightPanelCanvasLayer").GetNode("RightPanel").GetNode<VBoxContainer>("VBoxContainer").GetNode<TooltipRichTextLabel>("GridContainer/Yellow");
		_redLabel = GetParent().GetNode("RightPanelCanvasLayer").GetNode("RightPanel").GetNode<VBoxContainer>("VBoxContainer").GetNode<TooltipRichTextLabel>("GridContainer/Red");
		_blueLabel = GetParent().GetNode("RightPanelCanvasLayer").GetNode("RightPanel").GetNode<VBoxContainer>("VBoxContainer").GetNode<TooltipRichTextLabel>("GridContainer/Blue");
		_greenLabel = GetParent().GetNode("RightPanelCanvasLayer").GetNode("RightPanel").GetNode<VBoxContainer>("VBoxContainer").GetNode<TooltipRichTextLabel>("GridContainer/Green");

		_hpLabel = _rightPanel.GetNode<Label>("HpLabel");
		_waveCounter = _rightPanel.GetNode<Label>("WaveCounter");
		_bossWaveLabel = _rightPanel.GetNode<Label>("BossWaveLabel");
		_spawnerLimitLabel = _rightPanel.GetNode("HBoxContainer").GetNode<TooltipRichTextLabel>("SpawnerLimitLabel");
		_spawnerLimitIncreaseButton = _rightPanel.GetNode("HBoxContainer").GetNode<UpgradeButton>("Button");
		_spawnerLimitIncreaseButtonText = _spawnerLimitIncreaseButton.GetNode<TooltipRichTextLabel>("RichTextLabel");
		_spawnerLimitIncreaseButton.MouseEntered += () => _spawnerLimitIncreaseButtonText.AddThemeColorOverride("default_color", ThemePalette.White);
		_spawnerLimitIncreaseButton.MouseExited += () => _spawnerLimitIncreaseButtonText.AddThemeColorOverride("default_color",  GetSpawnerLimitIncreaseButtonTextColor());
		_globalEffectsContainer = _rightPanel.GetNode<GridContainer>("Passives/GridContainer");
		_challengeButton = _rightPanel.GetNode<UpgradeButton>("Challenge");
		_challengeButton.MouseEntered += () => _challengeButtonLabel.AddThemeColorOverride("default_color", ThemePalette.White);
		_challengeButton.MouseExited += () => _challengeButtonLabel.AddThemeColorOverride("default_color", GetSpawnerLimitIncreaseButtonTextColor());
		_challengeButtonLabel = _rightPanel.GetNode<TooltipRichTextLabel>("Challenge/RichTextLabel");

		_inspectionLayer = GetParent().GetNode<CanvasLayer>("InspectionLayer");
		_inspectionRequirementLabel = _inspectionLayer.GetNode("PanelContainer").GetNode<VBoxContainer>("VBoxContainer").GetNode<TooltipRichTextLabel>("Requirement");
		_inspectionSummaryLabel = _inspectionLayer.GetNode("PanelContainer").GetNode<VBoxContainer>("VBoxContainer").GetNode<TooltipRichTextLabel>("Summary");
		_inspectionDoneButton = _inspectionLayer.GetNode("PanelContainer").GetNode<VBoxContainer>("VBoxContainer").GetNode<Button>("Button");

		_inspectionProgressBar = _rightPanel.GetNode<TextureProgressBar>("InspectionProgression");
		_inspectionLabel = _rightPanel.GetNode<Label>("Inspection");

		_progressionBar = GetParent().GetNode("RightPanelCanvasLayer").GetNode("RightPanel").GetNode<VBoxContainer>("VBoxContainer").GetNode<HBoxContainer>("HBoxContainer").GetNode<TextureProgressBar>("Progression");
		_progressionBarStripes = _progressionBar.GetNode<StripeManager>("StripeManager");

		_fullscreenOverlay = GetParent().GetNode<CanvasLayer>("FullscreenOverlay");
		_infoPanel = GetParent().GetNode<UnitInfoPanel>("UnitInfoPanel");
	}

	public void Initialize(GameGlobals.GameMode mode, LevelResource level)
	{
		_unitManager = GetParent().GetNode<UnitManager>("UnitManager");
		_towerManager = GetParent().GetNode<TDTowerManager>("TowerManager");
		_level = level;
		InitializeLevel();
		_grid = GetParent().GetNode<Grid>("TileMapLayer");

		_towerManager.Initialize(_unitManager);

		_exit = (Exit)(_unitManager.SpawnUnit(_grid.GetExitLocation(), 0, "Exit"));
		_exit._tdManager = this;
		_exit._radius = TDManager.TileSize / (float)Math.Sqrt(2);
		_exit.SetSize();
		_saveManager = SaveManager.Instance;
		_saveManager._tdManager = this;

		_gameMode = mode;

		_remainingBosses = _earlyBosses.Duplicate();
		_remainingBosses.AddRange(_midBosses.Duplicate());
		_remainingBosses.AddRange(_lateBosses.Duplicate());

		_finalBossBuff = new();
		_miniBossBuff = new();

		_rewardManager = GetParent().GetNode<RewardManager>("RewardLayer");

		Connect(SignalName.GlobalPlacedTower, Callable.From<TowerUnit>(OnTowerPlaced));

		switch (mode)
		{
			case GameGlobals.GameMode.Debug:
				UpdateHp(1000);
				IncreaseSpawnerLimit(_startingSpawnerLimit);

				_availTowerList = _allTowerList;
				
				_rewardManager._noStartingReward = true;
				UpdatePortalLimitButtonText();
				UpdateChallengeButton();
				
				_rightPanel.GetNode<Button>("GetPassive").Show();
				_rightPanel.GetNode<Button>("GetAnyPassive").Show();
				InitializeFinalBoss();
				InitializeInspectionWaves();

				UpdateWaveIndexCounter();

				UpdateMoney(new Vector4I(99999, 99999, 99999, 99999));
				

				break;
			case GameGlobals.GameMode.Tutorial:
				UpdateHp(20);
				IncreaseSpawnerLimit(_startingSpawnerLimit);
				
				UpdatePortalLimitButtonText();
				UpdateChallengeButton();

				_tutorialManager = GetParent().GetNode<TutorialManager>("TutorialLayer/TutorialManager");
				_tutorialManager.Initialize(this, _grid, _unitManager);
				_availTowerList = _allTowerList;
				_towerManager.InitializeTowersPanel(TowerUnit.TowerType.Defense);
				InitializeFinalBoss();
				InitializeInspectionWaves();

				UpdateWaveIndexCounter();
				UpdateMoney(_startingMoney);
				
				break;
			case GameGlobals.GameMode.Continue:
				_saveManager.LoadGame();
				_rewardManager._noStartingReward = true;
				if (_gameMode == GameGlobals.GameMode.Debug)
				{
					_rightPanel.GetNode<Button>("GetPassive").Show();
					_rightPanel.GetNode<Button>("GetAnyPassive").Show();
				}
				UpdatePortalLimitButtonText();
				UpdateChallengeButton();

				InitializeFinalBoss();
				InitializeInspectionWaves();

				UpdateWaveIndexCounter();
				break;
			case GameGlobals.GameMode.Rogue:
				UpdateHp(20);
				IncreaseSpawnerLimit(_startingSpawnerLimit);
				
				
				UpdatePortalLimitButtonText();
				UpdateChallengeButton();

				_availTowerList = [];
				_rewardManager._noStartingReward = false;
				InitializeFinalBoss();
				InitializeInspectionWaves();

				UpdateWaveIndexCounter();
				UpdateMoney(_startingMoney);
				break;
			case GameGlobals.GameMode.Normal:
			default:
				UpdateHp(20);
				IncreaseSpawnerLimit(_startingSpawnerLimit);
				_availTowerList = _allTowerList;

				_rewardManager._noStartingReward = false;

				UpdatePortalLimitButtonText();
				UpdateChallengeButton();

				InitializeFinalBoss();
				InitializeInspectionWaves();

				UpdateWaveIndexCounter();
				UpdateMoney(_startingMoney);
				break;
		}

		if (mode != GameGlobals.GameMode.Continue)
		{
			foreach (GlobalEffectResource resource in MetaManager.Instance._obtainedMetaUpgrades)
			{
				ApplyGlobalEffect(resource, false);
			}
		}

		_rewardManager.Initialize();

		InitializeWaveProgressionBar();

		_towerManager.InitializeTowersPanel(TowerUnit.TowerType.Defense);
	}

	private void InitializeLevel()
	{
		if (!_level._inspectionEnabled)
		{
			_inspectionLayer.Hide();
		}
		if (!_level._challengeEnabled)
		{
			_challengeButton.Hide();
		}
		if (!_level._portalsEnabled)
		{
			GetParent().GetNode<HBoxContainer>("UnitInfoPanel/PanelContainer/Towers/HBoxContainer").Hide();
		}
		TileMapLayer mapLayer = _allMaps[_level._mapID].Instantiate<TileMapLayer>();
		GetParent().AddChild(mapLayer);
		GetParent().MoveChild(mapLayer, 0);
	}

	public void SpawnNextWave()
	{
		_saveManager.SaveGame(_gameMode, _money, _spawnerLimit, _hp, _waveIndex, _towerManager._towersOnField, _availTowerList, _globalEffects, _finalBoss, _level, _challengeCount, _inspectionFailedCount, _nextChallengeUnits);
		_waveIndex++;
		_leakedInvaderCount = 0;
		_waveEndProcessed = false;
		EmitSignal(SignalName.NewWave);
		UpdateWaveIndexCounter();
		GetTree().CreateTimer(0.5f, processAlways: false).Timeout += () =>
		{
			if (CheckWaveFinished() && !_waveEndProcessed)
			{
				OnWaveEnded();
			}
		};
	}

	public async void SpawnNextChallenge()
	{
		_currentWaveRewards = [RewardManager.RewardType.Passive, RewardManager.RewardType.Tower];
		Array<InvaderStatsIncreaseResource> challengeUnitsCopy = _nextChallengeUnits.Duplicate();
		int challengeCountCopy = _challengeCount;
		_challengeCount++;
		_nextChallengeUnits = null;
		_leakedInvaderCount = 0;
		_waveEndProcessed = false;
		await SpawnMiniBossWave(challengeUnitsCopy, challengeCountCopy);
	}

	public async void SpawnInspection()
	{
		_currentWaveRewards = [RewardManager.RewardType.Tower];
		_inspectionWaveOnGoing = true;
		_waveEndProcessed = false;
		if (_inspectionFailedCount< _inspectionBosses.Count)
		{
			await SpawnMiniBossWave(_inspectionBosses[_inspectionFailedCount]._units, 0);
		}
		else
		{
			await SpawnMiniBossWave(_inspectionBosses[_inspectionBosses.Count - 1]._units, 0);
		}
	}

	private async System.Threading.Tasks.Task SpawnMiniBossWave(Array<InvaderStatsIncreaseResource> units, int challengeCount)
	{
		Array<InvaderStatsIncreaseResource> enemyList = units.Duplicate();
		for (int i = 0; i < enemyList.Count; i++)
		{
			(InvaderStatsIncreaseResource enemy, float delay) = (enemyList[i], SpawnInterval * (float)Math.Pow(0.9f, enemyList.Count));
			InvaderStatsIncreaseResource enemyCopy = (InvaderStatsIncreaseResource)enemy.Duplicate();
			InvaderUnit unit = SpawnEnemyAtEntrance(enemyCopy._unitName);
			InvaderStatsIncreaseResource buff = new InvaderStatsIncreaseResource();
			_miniBossBuff.MergeWithOld(buff, []);
			float test = GetBossHpMultiplier(challengeCount);
			buff._percentHpBuff += GetBossHpMultiplier(challengeCount);
			buff.MergeWithOld(enemyCopy, []);
			unit.AddEffect(enemyCopy);
			if (delay > 0)
			{
				await ToSignal(GetTree().CreateTimer(delay, processAlways: false), SceneTreeTimer.SignalName.Timeout);
			}
		}
		EmitSignal(SignalName.GlobalBossSpawned);
	}

	public async void SpawnFinalBossWave()
	{
		Array<InvaderStatsIncreaseResource> enemyList = _finalBoss;
		for (int i = 0; i < enemyList.Count; i++)
		{
			(InvaderStatsIncreaseResource enemy, float delay) = (enemyList[i], SpawnInterval);
			InvaderUnit unit = SpawnEnemyAtEntrance(enemy._unitName);
			_finalBossBuff.MergeWithOld(enemy, []);
			unit.AddEffect(enemy);
			if (delay > 0)
			{
				await ToSignal(GetTree().CreateTimer(delay, processAlways: false), SceneTreeTimer.SignalName.Timeout);
			}
		}
		EmitSignal(SignalName.GlobalBossSpawned);
	}

	private void MakeInspectionWindow(int requirement)
	{
		_inspectionLayer.Show();
		_inspectionRequirementLabel.Text = $"Total Resources due: {requirement}";
		_inspectionSummaryLabel.Text = $"Liquid Capitol: {Utils.MakeMoneyText(_money)} = {Utils.VectorSum(_money)} \n";
		Vector4I towerCost = new Vector4I(0,0,0,0);
		foreach (TowerUnit tower in _towerManager._towersOnField)
		{
			towerCost += tower.GetTotalCost();
		}
		_inspectionSummaryLabel.Text += $"Invested Capitol: {Utils.MakeMoneyText(towerCost)} = {Utils.VectorSum(towerCost)} \n";
		int total = Utils.VectorSum(_money) + Utils.VectorSum(towerCost);
		if (total >= requirement)
		{
			_inspectionSummaryLabel.Text += $"Inspection Passed! Proceed to the next Term.";
			_inspectionDoneButton.Pressed += () =>
			{
				UpdateInspectionCounter();
				UpdateInspectionProgress();
				_inspectionLayer.Hide();
			};
		}
		else
		{
			_inspectionSummaryLabel.Text += $"Inspection Failed! Prepare for termination.";
			_inspectionDoneButton.Pressed += () =>
			{
				SpawnInspection();
				_inspectionFailedCount++;
				_inspectionLayer.Hide();
			};
		}
	}

	public bool CheckWaveFinished()
	{
		if (_aliveInvaderCount == 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public void AddRewardNow(RewardManager.RewardType reward)
	{
		_rewardManager._choicesQueue.Add(reward);
		_rewardManager.MakeRewardPrompt(RewardManager.RewardSource.GlobalEffect);
	}

	public void OnWaveEnded()
	{
		if (!_inspectionProcessed && GetNextInspectionWave() == _waveIndex)
		{
			_inspectionProcessed = true;
			MakeInspectionWindow(GetNextInspectionRequirement());
		}
		else if (_inspectionWaveOnGoing)
		{
			UpdateInspectionCounter();
			UpdateInspectionProgress();
		}
		_waveEndProcessed = true;
		if (_tutorialManager is not null)
		{
			_tutorialManager.NextWave();
			_tutorialManager.Show();
		}
		if (_rewardManager is not null)
		{
			if (_currentWaveRewards is not null && _currentWaveRewards.Count != 0 && _leakedInvaderCount == 0)
			{
				foreach (RewardManager.RewardType type in _currentWaveRewards)
				{
					_rewardManager._choicesQueue.Add(type);
				}
				_rewardManager.MakeRewardPrompt(RewardManager.RewardSource.Boss);
				_currentWaveRewards = [];
			}
		}
	}

	public void UpdateWaveIndexCounter()
	{
		_waveCounter.Text = "Cycle " + _waveIndex;
		UpdateInspectionCounter();
		UpdateWaveProgressionBar();
	}

	public void UpdateInspectionCounter()
	{
		if (GetNextInspectionWave() == -1)
		{
			_inspectionLabel.Hide();
			return;
		}
		int delta = GetNextInspectionWave() - _waveIndex;
		if (delta > 0)
		{
			_inspectionLabel.Text = $"Next Inspection in {GetNextInspectionWave() - _waveIndex} cycles.\nFulfillment: ";
		}
		else if (!_inspectionProcessed)
		{
			_inspectionLabel.Text = $"Inspection after this Cycle!\nFulfillment: ";
		}
		else
		{
			_inspectionLabel.Text = $"Next Inspection in {GetNextInspectionWave(true) - _waveIndex} cycles.\nFulfillment: ";
		}
	}

	public void InitializeWaveProgressionBar()
	{
		_progressionBar.MaxValue = _level._finalWave;
		_progressionBar.TintProgress = ThemePalette.Red;
		UpdateWaveProgressionBar();
	}

	public async void UpdateWaveProgressionBar()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		foreach (Node node in _progressionBarStripes.GetChildren())
		{
			node.QueueFree();
		}
		_progressionBarStripes._largeStripeLocations = [];

		_progressionBar.Value = _waveIndex;
		float width = _progressionBar.Size.Y;
		for (int i = 0; i < _level._finalWave + 1; i++)
		{
			float yPos = (float)i / (float)_level._finalWave * width;
			if (i == _level._finalWave)
			{
				_progressionBarStripes._largeStripeLocations.Add(yPos);

				Label indexLabel = new Label();

				// 2. Apply the font override
				indexLabel.AddThemeFontOverride("font", ThemePalette.EditUndo);

				indexLabel.Text = $"{i}";

				indexLabel.AddThemeColorOverride("font_outline_color", Colors.Black);

				// 2. Set the thickness/size of the border in pixels
				indexLabel.AddThemeConstantOverride("outline_size", 8);

				_progressionBarStripes.AddChild(indexLabel);

				Vector2 contentSize = indexLabel.GetMinimumSize();

				// Position top-right corner at the target location
				indexLabel.Position = new Vector2(0, _progressionBar.Size.Y - yPos);

				TooltipRichTextLabel rtl = new TooltipRichTextLabel();

				rtl.Size = new Vector2(18, 18);

				rtl.Position = new Vector2(_progressionBar.Size.X, _progressionBar.Size.Y - yPos - rtl.Size.Y / 2);

				string displayName = Spawner.GetGroupEnemyNames(_finalBoss);

				rtl.Text = $"[url={displayName + "\n" + StringDB.Entries["FinalBoss"]}]::finalboss::[/url]";

				rtl.PivotOffset = new Vector2(0, rtl.Size.Y / 2f);

				rtl.Scale = new Vector2(2, 2);

				_progressionBarStripes.AddChild(rtl);
			}
			else if (_inspectionList.Keys.Contains(i))
			{
				_progressionBarStripes._largeStripeLocations.Add(yPos);

				Label indexLabel = new Label();

				// 2. Apply the font override
				indexLabel.AddThemeFontOverride("font", ThemePalette.EditUndo);

				indexLabel.Text = $"{i}";

				indexLabel.AddThemeColorOverride("font_outline_color", Colors.Black);

				// 2. Set the thickness/size of the border in pixels
				indexLabel.AddThemeConstantOverride("outline_size", 8);

				_progressionBarStripes.AddChild(indexLabel);

				Vector2 contentSize = indexLabel.GetMinimumSize();

				// Position top-right corner at the target location
				indexLabel.Position = new Vector2(0, _progressionBar.Size.Y - yPos);

				TooltipRichTextLabel rtl = new TooltipRichTextLabel();

				rtl.Size = new Vector2(18, 18);

				rtl.Position = new Vector2(_progressionBar.Size.X, _progressionBar.Size.Y - yPos - rtl.Size.Y / 2);

				rtl.Text = $"[url={$"Inspection! \n Amount due: {GetInspectionRequirement(i)}"}]::inspection::[/url]";

				rtl.PivotOffset = new Vector2(0, rtl.Size.Y / 2f);

				rtl.Scale = new Vector2(2, 2);

				_progressionBarStripes.AddChild(rtl);
			}
			else
			{
				_progressionBarStripes._smallStripeLocations.Add(yPos);
			}
		}

		_progressionBarStripes.QueueRedraw();
	}

	private void UpdateChallengeButton()
	{
		if (_nextChallengeUnits == null)
		{
			SpawnerDataResource units;
			if (_challengeCount < _midBossStartsAt)
			{
				units = Utils.GetRandomElements<SpawnerDataResource>(_earlyBosses, 1)[0];
			}
			else if (_challengeCount < _lateBossStartsAt)
			{
				units = Utils.GetRandomElements<SpawnerDataResource>(_midBosses, 1)[0];
			}
			else
			{
				units = Utils.GetRandomElements<SpawnerDataResource>(_lateBosses, 1)[0];
			}
			_nextChallengeUnits = units._units;
		}
		_challengeButtonLabel.Text = $"Next Challenge: \n{Spawner.GetGroupEnemyNames(_nextChallengeUnits)}{Utils.MakeMoneyText(new Vector4I(GetChallengeCost(),0,0,0))}";
		_challengeButtonLabel.AddThemeColorOverride("default_color", GetChallengeButtonTextColor());
	}

	//public void AddRandomBossToWave(int wave)
	//{
	//	if (_remainingBosses.Count == 0)
	//	{
	//		return;
	//	}

	//	Array<SpawnerDataResource> boss;
	//	if (_waveList.Count >= _lateBossStartsAt)
	//	{
	//		Array<SpawnerDataResource> remainingLateBosses = [];
	//		foreach (SpawnerDataResource invader in _remainingBosses)
	//		{
	//			if (_lateBosses.Contains(invader))
	//			{
	//				remainingLateBosses.Add(invader);
	//			}
	//		}
	//		boss = Utils.GetRandomElements<SpawnerDataResource>(remainingLateBosses, 1);
	//	}
	//	else if (_waveList.Count >= _midBossStartsAt)
	//	{
	//		Array<SpawnerDataResource> remainingMidBosses = [];
	//		foreach (SpawnerDataResource invader in _remainingBosses)
	//		{
	//			if (_midBosses.Contains(invader))
	//			{
	//				remainingMidBosses.Add(invader);
	//			}
	//		}
	//		boss = Utils.GetRandomElements<SpawnerDataResource>(remainingMidBosses, 1);
	//	}
	//	else
	//	{
	//		Array<SpawnerDataResource> remainingEarlyBosses = [];
	//		foreach (SpawnerDataResource invader in _remainingBosses)
	//		{
	//			if (_earlyBosses.Contains(invader))
	//			{
	//				remainingEarlyBosses.Add(invader);
	//			}
	//		}
	//		boss = Utils.GetRandomElements<SpawnerDataResource>(remainingEarlyBosses, 1);
	//	}

			
	//	_remainingBosses.Remove(boss[0]);
	//	_waveList.Add(wave, boss[0]._units);
	//	if (_rewardManager is not null)
	//	{
	//		_rewardList.Add(wave, [RewardManager.RewardType.Tower, RewardManager.RewardType.Passive]);
	//	}
	//}

	/// <summary>
	/// Adds one of the _bosses or _finalBosses to _waveList. 
	/// </summary>
	/// <param name="wave"></param>
	/// <param name="boss">
	/// _internalNames of the boss wave to be added. 
	/// </param>
	/// <exception cref="Exception"></exception>
	public void AddBossToWave(int wave, Array<string> boss)
	{
		if (wave != _level._finalWave)
		{
			bool match = false;
			foreach (SpawnerDataResource remainingBoss in _remainingBosses)
			{
				Array<string> copiedWave = [];
				foreach (InvaderStatsIncreaseResource unit in remainingBoss._units)
				{
					copiedWave.Add(unit._unitName);
				}
				if (copiedWave.SequenceEqual(boss))
				{
					match = true;
					_remainingBosses.Remove(remainingBoss);
					_waveList.Add(wave, remainingBoss._units);
					break;
				}
			}
			if (!match)
			{
				throw new Exception("boss already used");
			}
			
		}
		else
		{
			bool match = false;
			foreach (SpawnerDataResource remainingBoss in _finalBosses)
			{
				Array<string> copiedWave = [];
				foreach (InvaderStatsIncreaseResource unit in remainingBoss._units)
				{
					copiedWave.Add(unit._unitName);
				}
				if (copiedWave.SequenceEqual(boss))
				{
					match = true;
					_finalBoss = remainingBoss._units;
					break;
				}
			}
			if (!match)
			{
				throw new Exception("boss already used");
			}
			
		}	
	}

	//public void AddRewardToWave(int wave, Array<RewardManager.RewardType>reward)
	//{
	//	_rewardList.Add(wave, reward);
	//}

	public void InitializeFinalBoss()
	{
		if (_level._randomizeFinalBoss)
		{
			Array<SpawnerDataResource> boss = Utils.GetRandomElements<SpawnerDataResource>(_finalBosses, 1);
			_finalBoss = boss[0]._units;
			SpawnerDataResource resource = new SpawnerDataResource();
			resource._units = _finalBoss;
			_level._finalBoss = resource;
		}
		else
		{
			_finalBoss = _level._finalBoss._units;
		}
	}

	public void InitializeInspectionWaves()
	{
		if (!_level._inspectionEnabled)
		{
			return;
		}
		for (int i = 1; i < _level._finalWave; i++)
		{
			if (i % _level._inspectionInterval == 0)
			{
				_inspectionList.Add(i, GetInspectionRequirement(i));
			}
		}
	}

	//public void SpeedUpBosses(int n)
	//{
	//	int m = _waveList.Keys.Max();
	//	if (_finalWave - m >= BossInterval - 1)
	//	{
	//		AddRandomBossToWave(m + BossInterval);
	//	}

	//	var newWaveList = new Godot.Collections.Dictionary<int, Array<InvaderStatsIncreaseResource>>();

	//	foreach (var (key, value) in _waveList)
	//	{
	//		if (key > _waveIndex)
	//		{
	//			newWaveList[key - n] = value;
	//		}
	//		else
	//		{
	//			newWaveList[key] = value;
	//		}
	//	}

	//	_waveList = newWaveList;

	//	var newRewardList = new Godot.Collections.Dictionary<int, Array<RewardManager.RewardType>>();

	//	foreach (var (key, value) in _rewardList)
	//	{
	//		if (key > _waveIndex)
	//		{
	//			newRewardList[key - n] = value;
	//		}
	//		else
	//		{
	//			newRewardList[key] = value;
	//		}
	//	}

	//	_rewardList = newRewardList;

	//	m = _waveList.Keys.Max();
	//	if (_finalWave - m >= BossInterval - 1)
	//	{
	//		AddRandomBossToWave(m + BossInterval);
	//	}
	//	UpdateWaveProgressionBar();
	//}

	public void UpdateSpawnerLimit(int newLimit)
	{
		_spawnerLimit = newLimit;
		_spawnerLimitLabel.Text = "Portals: " + _spawnerCount + "/" + _spawnerLimit;
	}

	public void IncreaseSpawnerLimit(int change)
	{
		UpdateSpawnerLimit(_spawnerLimit + change);
	}

	public void UpdateSpawnerCount(int newCount)
	{
		_spawnerCount = newCount;
		_spawnerLimitLabel.Text = "Portals: " + _spawnerCount + "/" + _spawnerLimit;
	}

	public void IncreaseSpawnerCount(int change)
	{
		UpdateSpawnerCount(_spawnerCount + change);
	}

	public bool CanBuildExtraSpawner()
	{
		return _spawnerCount < _spawnerLimit;
	}

	public void BuyExtraPortalLimit()
	{
		int cost = GetPortalLimitUpgradeCost();
		if (_money[0] < cost)
		{
			return;
		}
		IncreaseSpawnerLimit(1);
		SpendMoney(new Vector4I(cost, 0, 0, 0));
		UpdatePortalLimitButtonText();
	}

	public void UpdatePortalLimitButtonText()
	{
		int cost = GetPortalLimitUpgradeCost();
		_spawnerLimitIncreaseButtonText.Text = "Portal Capacity +1\n" + Utils.MakeMoneyText(new Vector4I(cost, 0, 0, 0));
		_spawnerLimitIncreaseButtonText.AddThemeColorOverride("default_color", GetSpawnerLimitIncreaseButtonTextColor());
	}

	public int GetPortalLimitUpgradeCost()
	{
		return 20 * (int)Math.Pow(2, (double)(_spawnerLimit - _startingSpawnerLimit) / 2f);
	}

	public void BuyChallenge()
	{
		int cost = GetChallengeCost();
		if (_money[0] < cost)
		{
			return;
		}
		SpawnNextChallenge();
		SpendMoney(new Vector4I(cost, 0, 0, 0));
		UpdateChallengeButton();
	}

	public int GetChallengeCost()
	{
		return 20 * (int)Math.Pow(2, (double)_challengeCount / 2f);
	}

	public float GetBossHpMultiplier(int challengeCount)
	{
		int i = challengeCount;
		if (challengeCount >= _lateBossStartsAt)
		{
			i -= _lateBossStartsAt;
		}
		else if (challengeCount >= _midBossStartsAt)
		{
			i -= _midBossStartsAt;
		}
		return (float)Math.Pow(_bossHpMultiplier, i) - 1f;

	}

	public void AddMiniBossBuff(InvaderStatsIncreaseResource buff)
	{
		buff.MergeWithOld(_miniBossBuff, []);
	}

	public void AddFinalBossBuff(InvaderStatsIncreaseResource buff)
	{
		buff.MergeWithOld(_finalBossBuff, []);
	}

	public void UpdateInspectionProgress()
	{
		if (GetNextInspectionWave() == -1)
		{
			_inspectionProgressBar.Hide();
			return;
		}
		_inspectionProgressBar.TintProgress = ThemePalette.Green;
		_inspectionProgressBar.MaxValue = GetNextInspectionRequirement(_inspectionProcessed);
		Vector4I towerCost = new Vector4I(0, 0, 0, 0);
		foreach (TowerUnit tower in _towerManager._towersOnField)
		{
			towerCost += tower.GetTotalCost();
		}
		int total = Utils.VectorSum(_money) + Utils.VectorSum(towerCost);
		_inspectionProgressBar.Value = total;

		foreach(var node in _inspectionProgressBar.GetChildren())
		{
			node.QueueFree();
		}

		float xPos = (float)(total / _inspectionProgressBar.MaxValue * _inspectionProgressBar.Size.X);

		Label indexLabel = new Label();

		indexLabel.Text = $"{total}";

		_inspectionProgressBar.AddChild(indexLabel);

		Vector2 size = indexLabel.GetMinimumSize();

		// Position top-right corner at the target location
		indexLabel.Position = new Vector2(xPos - size.X / 2, _inspectionProgressBar.Size.Y);

		Label targetLabel = new Label();

		targetLabel.Text = $"{_inspectionProgressBar.MaxValue}";
		targetLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);

		_inspectionProgressBar.AddChild(targetLabel);

		size = targetLabel.GetMinimumSize();

		// Position top-right corner at the target location
		targetLabel.Position = new Vector2(_inspectionProgressBar.Size.X - size.X / 2, _inspectionProgressBar.Size.Y);
	}

	/// <summary>
	/// Get required amount to pass the current inspection.
	/// </summary>
	/// <returns></returns>
	public int GetInspectionRequirement(int n)
	{
		return _inspectionRequirements[(int)Math.Floor(n / (double)(_level._inspectionInterval)) - 1];
	}

	public int GetNextInspectionWave(bool ignoreCurrentWave = false)
	{
		for (int i = _waveIndex; i < _level._finalWave; i++)
		{
			if (i == 0 || ignoreCurrentWave)
			{
				return i + _level._inspectionInterval;
			}
			else if (i % _level._inspectionInterval == 0)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetNextInspectionRequirement(bool ignoreCurrentWave = false)
	{
		return GetInspectionRequirement(GetNextInspectionWave(ignoreCurrentWave));
	}

	public void FlashMoney()
	{
		//_moneyLabel.FlashRed();
	}

	public void FlashPortalLimit()
	{
		_spawnerLimitLabel.FlashRed();
	}

	private InvaderUnit SpawnEnemyAtEntrance(string name)
	{
		Unit unit = _unitManager.SpawnUnit(_grid.GetEntrancePosition(), 1, name, true);

		List<Vector2> waypoints = _grid.GetPath(_grid.GetEntrancePosition(), _grid.GetExitLocation());
		if (unit is InvaderUnit invader)
		{
			invader.SetRandomPathOffset();
			invader.SetPathToExit(new Godot.Collections.Array<Vector2>(waypoints));
		}
		unit.Connect(Unit.SignalName.Died, Callable.From(() => OnUnitDied(unit)));
		_aliveInvaderCount++;
		return (InvaderUnit)unit;
	}

	public InvaderUnit SpawnEnemyAtGlobalPosition(string name, Vector2 position)
	{
		Vector2 offset = position - _grid.MapToGlobal((_grid.LocalToMap(_grid.ToLocal(position))));
		Unit unit = _unitManager.SpawnUnit(position, 1, name, true);
		List<Vector2> waypoints = _grid.GetPath(position, _grid.GetExitLocation());
		if (unit is InvaderUnit invader)
		{
			invader.SetPathOffset(offset);
			invader.SetPathToExit(new Godot.Collections.Array<Vector2>(waypoints));
		}
		unit.Connect(Unit.SignalName.Died, Callable.From(() => OnUnitDied(unit)));
		_aliveInvaderCount++;
		return (InvaderUnit)unit;
	}

	public InvaderUnit SpawnEnemyFromTower(string name, Vector2 position)
	{
		Unit unit = _unitManager.SpawnUnit(position, 1, name, true);
		List<Vector2> waypoints = _grid.GetPath(position, _grid.GetExitLocation());
		if (unit is InvaderUnit invader)
		{
			invader.SetRandomPathOffset();
			invader.SetPathToExit(new Godot.Collections.Array<Vector2>(waypoints));
		}
		unit.Connect(Unit.SignalName.Died, Callable.From(() => OnUnitDied(unit)));
		_aliveInvaderCount++;
		return (InvaderUnit)unit;
	}

	public InvaderUnit SpawnEnemyFromTower(string name, Vector2I gridPosition)
	{
		Unit unit = _unitManager.SpawnUnit(_grid.MapToGlobal(gridPosition), 1, name, true);
		List<Vector2> waypoints = _grid.GetPath(gridPosition, _grid.GetExitLocation());
		if (unit is InvaderUnit invader)
		{
			invader.SetRandomPathOffset();
			invader.SetPathToExit(new Godot.Collections.Array<Vector2>(waypoints));
		}
		unit.Connect(Unit.SignalName.Died, Callable.From(() => OnUnitDied(unit)));
		_aliveInvaderCount++;
		return (InvaderUnit)unit;
	}

	public static InvaderUnit GetEnemy(string name, bool setUnit)
	{
		InvaderUnit unit = (InvaderUnit) UnitManager.GetUnit(name, setUnit);
		return unit;
	}

	public Unit SpawnAllyFromTower(string name, Vector2 position)
	{
		Unit unit = _unitManager.SpawnUnit(position, 0, name, true);
		GD.Print(name + " Spawned");
		unit.Connect(Unit.SignalName.Died, Callable.From(() => OnUnitDied(unit)));
		return (Unit)unit;
	}

	public void AddEnemyToQueue(InvaderStatsIncreaseResource unit)
	{
		int nextWave = GetNextBossWave();
		if (nextWave == -1)
		{
			return;
		}
		else
		{
			_waveList[nextWave].Add(unit);
		}
	}

	public void UpdateHp(int newHp)
	{
		_hp = newHp;
		_hpLabel.Text = "Hp: " + _hp.ToString();
		if (_hp <= 0)
		{
			LoseTD();
		}
	}

	public void IncreaseHp(int change)
	{
		UpdateHp(_hp + change);
	}

	public void UnlockTower(string name)
	{
		_availTowerList.Add(name);
		_towerManager.UpdateTowersPanel();
	}

	private void Quit()
	{
		if (CheckWaveFinished())
		{
			_waveList.Add(_level._finalWave, _finalBoss);
			_saveManager.SaveGame(_gameMode, _money, _spawnerLimit, _hp, _waveIndex, _towerManager._towersOnField, _availTowerList, _globalEffects, _finalBoss, _level, _challengeCount, _inspectionFailedCount, _nextChallengeUnits);
		}
		NormalSpeed();
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, MenuPath);
	}

	private void LoseTD()
	{
		GetTree().Paused = true;
		_saveManager.DeleteSave();
		MetaManager.Instance.IncreaseLosses(1);
		MakeEndTDWindow(false);
	}

	private void WinTD()
	{
		GetTree().Paused = true;
		_saveManager.DeleteSave();
		MetaManager.Instance.IncreaseWins(1);
		MakeEndTDWindow(true);
	}

	private void MakeEndTDWindow(bool win)
	{
		GetParent().GetNode<CanvasLayer>("EndLayer").Show();
		string mainText;
		int rewardAmount = 0;
		if (win)
		{
			mainText = StringDB.Entries["WinWindow"] + "\n";
		}
		else
		{
			mainText = StringDB.Entries["LossWindow"] + "\n";
		}
		switch (_gameMode)
		{
			case GameGlobals.GameMode.Normal:
				mainText += $"${(_waveIndex - 1) * 5} -- Survived {(_waveIndex - 1)} waves \n";
				mainText += $"${GetMiniBossDefeated() * 5} -- Defeated {GetMiniBossDefeated()} mini bosses \n";
				if (win)
				{
					mainText += $"$20 -- Defeated final boss \n";
					rewardAmount = (_waveIndex - 1) * 5 + GetMiniBossDefeated() * 5 + 20;
				}
				else
				{
					rewardAmount = (_waveIndex - 1) * 5 + GetMiniBossDefeated() * 5;
				}
				break;
			case GameGlobals.GameMode.Rogue:
				mainText += $"${(_waveIndex - 1) * 5} -- Survived {(_waveIndex - 1)} waves \n";
				mainText += $"${_availTowerList.Count * 5} -- Obtained {_availTowerList.Count} towers \n";
				mainText += $"${_globalEffects.Count * 5} -- Obtained {_globalEffects.Count} passives \n";
				mainText += $"${GetMiniBossDefeated() * 5} -- Defeated {GetMiniBossDefeated()} mini bosses \n";
				if (win)
				{
					mainText += $"$20 -- Defeated final boss \n";
					rewardAmount = (_waveIndex - 1) * 5 + _availTowerList.Count * 5 + _globalEffects.Count * 5 + GetMiniBossDefeated() * 5 + 20;
				}
				else
				{
					rewardAmount = (_waveIndex - 1) * 5 + _availTowerList.Count * 5 + _globalEffects.Count * 5 + GetMiniBossDefeated() * 5;
				}
				break;
			case GameGlobals.GameMode.Debug:
				mainText += $"$Survived {(_waveIndex - 1)} waves \n";
				mainText += $"$Defeated {GetMiniBossDefeated()} mini bosses \n";
				break;
		}
		

		MetaManager.Instance.IncreaseMoney(rewardAmount);

		GetParent().GetNode<Button>("EndLayer/PanelContainer/VBoxContainer/Button").Text = $"Claim ${rewardAmount} and return to menu";

		GetParent().GetNode<TooltipRichTextLabel>("EndLayer/PanelContainer/VBoxContainer/RichTextLabel").Text = mainText;
	}

	private void OnCompleteButtonPressed()
	{
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, MenuPath);
		GetTree().Paused = false;
	}

	private void SwitchPauseState()
	{
		if (GetTree().Paused)
		{
			UnpauseTD();
		}
		else
		{
			PauseTD();
		}
	}

	private void PauseTD()
	{
		GetTree().Paused = true;
		_fullscreenOverlay.GetNode<Label>("Paused").Visible = true;
	}

	private void UnpauseTD()
	{
		GetTree().Paused = false;
		_fullscreenOverlay.GetNode<Label>("Paused").Visible = false;
	}

	private void ToggleDoubleSpeed(bool toggle)
	{
		if (toggle)
		{
			DoubleSpeed();
		}
		else
		{
			NormalSpeed();
		}
	}

	private void DoubleSpeed()
	{
		Engine.TimeScale = 2.0;
	}

	private void NormalSpeed()
	{
		Engine.TimeScale = 1.0;
	}

	private void FastForward()
	{
		GetParent().GetNode<CheckButton>("TopLeftOptions/VBoxContainer/SpeedUp").ButtonPressed = false;
		Engine.TimeScale = 10.0;
	}

	public void UpdateMoney(Vector4I newMoney)
	{
		_money = newMoney;
		_yellowLabel.Text = Utils.MakeMoneyText(new(_money[0],0,0,0), big: true);
		_redLabel.Text = Utils.MakeMoneyText(new(0, _money[1], 0, 0), big: true);
		_blueLabel.Text = Utils.MakeMoneyText(new(0, 0, _money[2], 0), big: true);
		_greenLabel.Text = Utils.MakeMoneyText(new(0, 0, 0, _money[3]), big: true);
		if (_infoPanel.GetSelectedUnit() is not null)
		{
			_infoPanel.GetSelectedUnit().EmitSignal(Unit.SignalName.UpdateUpgradeButton);
		}
		UpdateInspectionProgress();
		_spawnerLimitIncreaseButton.UpdateAffordabilityDisplay(Utils.VectorDivision(newMoney, new(GetPortalLimitUpgradeCost(),0,0,0)));
		_spawnerLimitIncreaseButtonText.AddThemeColorOverride("default_color", GetSpawnerLimitIncreaseButtonTextColor());
		_challengeButton.UpdateAffordabilityDisplay(Utils.VectorDivision(newMoney, new(GetChallengeCost(), 0, 0, 0)));
		_challengeButtonLabel.AddThemeColorOverride("default_color", GetChallengeButtonTextColor());
	}

	private Color GetSpawnerLimitIncreaseButtonTextColor()
	{
		if (Utils.VectorDivision(_money, new(GetPortalLimitUpgradeCost(), 0, 0, 0)) >= 1f)
		{
			return ThemePalette.Green;
		}
		else
		{
			return ThemePalette.Red;
		}
	}

	private Color GetChallengeButtonTextColor()
	{
		if (Utils.VectorDivision(_money, new(GetChallengeCost(), 0, 0, 0)) >= 1f)
		{
			return ThemePalette.Green;
		}
		else
		{
			return ThemePalette.Red;
		}
	}

	public void GainMoney(Vector4I gain)
	{
		UpdateMoney(_money + gain);
	}

	public void SpendMoney(Vector4I cost)
	{
		UpdateMoney(_money - cost);
	}
	
	public void OnTowerPlaced(TowerUnit tower)
	{
		UpdateInspectionProgress();
	}

	private void OnUnitDied(Unit unit)
	{
		if (unit is InvaderUnit invader)
		{
			GainMoney(invader.GetSelfMoneyDropped());
			_aliveInvaderCount--;
			GetTree().CreateTimer(0.5f, processAlways: false).Timeout += () =>
			{
				if (CheckWaveFinished() && !_waveEndProcessed)
				{
					OnWaveEnded();
				}
			};
		}
	}

	public void UnitExited(InvaderUnit unit)
	{
		UpdateMoney(_money - unit._moneyDeducted);
		UpdateHp(_hp - unit._hpDeducted);
		_aliveInvaderCount--;
		_leakedInvaderCount++;
		GetTree().CreateTimer(0.5f, processAlways: false).Timeout += () =>
		{
			if (CheckWaveFinished() && !_waveEndProcessed)
			{
				OnWaveEnded();
			}
		};
	}

	public int GetNextBossWave()
	{
		List<int> waves = _waveList.Keys.ToList();
		int? result = waves.Where(x => x > _waveIndex)
									.Cast<int?>() // Allows returning null if none found
									.Min();
		if (result is not null)
		{
			return (int)result;
		}
		else
		{
			return (int)waves.Max() + _level._inspectionInterval;
		}
	}

	public int GetMiniBossDefeated()
	{
		int count = 0;
		for (int i = 0; i < _waveIndex; i++)
		{
			if (_waveList.Keys.Contains(i))
			{
				count ++;
			}
		}
		return count;
	}

	public void GetNewPassive()
	{
		if (_rewardManager is null)
		{
			return;
		}
		_rewardManager._choicesQueue.Add(RewardManager.RewardType.Passive);
		_rewardManager.MakeRewardPrompt(RewardManager.RewardSource.GlobalEffect);
	}

	public void GetNewAnyPassive()
	{
		if (_rewardManager is null)
		{
			return;
		}
		_rewardManager._choicesQueue.Add(RewardManager.RewardType.PassiveAll);
		_rewardManager.MakeRewardPrompt(RewardManager.RewardSource.GlobalEffect);
	}

	public void ApplyGlobalEffect(GlobalEffectResource resource, bool noImmediateEffect)
	{
		GlobalEffectManager.Apply(resource, this, noImmediateEffect);
		_globalEffects.Add(resource);
		AddGlobalEffectToContainer(resource);
	}

	public void ApplyGlobalEffect(int index, bool noImmediateEffect)
	{
		ApplyGlobalEffect(_allGlobalEffects[index], noImmediateEffect);
	}

	public void AddGlobalEffectToContainer(GlobalEffectResource resource)
	{
		if (_globalEffects.Count > 0)
		{
			_rightPanel.GetNode<PanelContainer>("Passives").Show();
		}
		_globalEffectsContainer.AddChild(resource.MakeEffectTooltip(true));
	}
}
