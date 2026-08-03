using Godot;
using Godot.Collections;
using RTSGame.Units;
using System;
using System.Collections.Generic;
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
	public string MenuPath = "res://_Content/_Scenes/StartScene.tscn";

	[Export]
	public Array<string> _allTowerList;

	public Array<string> _availTowerList;

	[Export]
	public Array<SpawnerDataResource> _earlyBosses;

	[Export]
	public Array<SpawnerDataResource> _midBosses;

	[Export]
	public Array<SpawnerDataResource> _lateBosses;

	[Export]
	public Array<SpawnerDataResource> _finalBosses;

	[Export]
	public Vector4I _sandboxStartingMoney = new Vector4I(100, 0, 0, 0);

	[Export]
	public Vector4I _rogueStartingMoney = new Vector4I(50, 0, 0, 0);

	[Export]
	private int _startingSpawnerLimit = 5;

	[Export]
	private int _finalWave = 10;

	[Export]
	private int _midBossStartsAt = 2;

	[Export]
	private int _lateBossStartsAt = 4;

	[Export]
	public int BossInterval = 5;

	[Export]
	public Array<GlobalEffectResource> _allGlobalEffects { get; private set; }

	public InvaderStatsIncreaseResource _finalBossBuff;

	public InvaderStatsIncreaseResource _miniBossBuff;

	public float _bossHpMultiplier = 1.3f;

	/// <summary>
	/// Time between successive enemy spawns from entrance in the same wave. 
	/// </summary>
	public const float SpawnInterval = 1f; 
	public const float TileSize = 120f;

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
	private TooltipRichTextLabel _moneyLabel;
	private Grid _grid;
	private Label _waveCounter;
	private TextureProgressBar _progressionBar;
	private StripeManager _progressionBarStripes;
	private Label _bossWaveLabel;
	private TooltipRichTextLabel _spawnerLimitLabel;
	public UpgradeButton _spawnerLimitIncreaseButton;
	private TooltipRichTextLabel _spawnerLimitIncreaseButtonText;
	public UpgradeButton _speedUpWaveButton;
	private TooltipRichTextLabel _speedUpWaveButtonText;
	private CanvasLayer _fullscreenOverlay;
	private GridContainer _globalEffectsContainer;

	private Exit _exit;

	private int _invaderCount;
	private bool _waveRewardGiven = false;

	private Array<SpawnerDataResource> _remainingBosses = [];

	public Array<GlobalEffectResource> _globalEffects = [];

	public GameGlobals.GameMode _gameMode;
	public int _hp;
	public Vector4I _money;
	private int _spawnerLimit;
	private int _spawnerCount;
	public int _speedUpWaveCount;

	public override void _Ready()
	{
		_grid = GetParent().GetNode<Grid>("TileMapLayer");

		_rightPanel = GetParent().GetNode("RightPanelCanvasLayer").GetNode("RightPanel").GetNode<VBoxContainer>("VBoxContainer").GetNode<HBoxContainer>("HBoxContainer").GetNode<VBoxContainer>("VBoxContainer");

		_moneyLabel = GetParent().GetNode("RightPanelCanvasLayer").GetNode("RightPanel").GetNode<VBoxContainer>("VBoxContainer").GetNode<TooltipRichTextLabel>("MoneyLabel");

		_hpLabel = _rightPanel.GetNode<Label>("HpLabel");
		_waveCounter = _rightPanel.GetNode<Label>("WaveCounter");
		_bossWaveLabel = _rightPanel.GetNode<Label>("BossWaveLabel");
		_spawnerLimitLabel = _rightPanel.GetNode("HBoxContainer").GetNode<TooltipRichTextLabel>("SpawnerLimitLabel");
		_spawnerLimitIncreaseButton = _rightPanel.GetNode("HBoxContainer").GetNode<UpgradeButton>("Button");
		_spawnerLimitIncreaseButtonText = _spawnerLimitIncreaseButton.GetNode<TooltipRichTextLabel>("RichTextLabel");
		_spawnerLimitIncreaseButton.MouseEntered += () => _spawnerLimitIncreaseButtonText.AddThemeColorOverride("default_color", ThemePalette.White);
		_spawnerLimitIncreaseButton.MouseExited += () => _spawnerLimitIncreaseButtonText.AddThemeColorOverride("default_color",  GetSpawnerLimitIncreaseButtonTextColor());
		_speedUpWaveButton = _rightPanel.GetNode("HBoxContainer2").GetNode<UpgradeButton>("Button");
		_speedUpWaveButtonText = _speedUpWaveButton.GetNode<TooltipRichTextLabel>("RichTextLabel");
		_speedUpWaveButton.MouseEntered += () => _speedUpWaveButtonText.AddThemeColorOverride("default_color", ThemePalette.White);
		_speedUpWaveButton.MouseExited += () => _speedUpWaveButtonText.AddThemeColorOverride("default_color", GetSpeedUpWaveButtonTextColor());
		_globalEffectsContainer = _rightPanel.GetNode<GridContainer>("Passives/GridContainer");

		_progressionBar = GetParent().GetNode("RightPanelCanvasLayer").GetNode("RightPanel").GetNode<VBoxContainer>("VBoxContainer").GetNode<HBoxContainer>("HBoxContainer").GetNode<TextureProgressBar>("Progression");
		_progressionBarStripes = _progressionBar.GetNode<StripeManager>("StripeManager");

		_fullscreenOverlay = GetParent().GetNode<CanvasLayer>("FullscreenOverlay");
		_infoPanel = GetParent().GetNode<UnitInfoPanel>("UnitInfoPanel");
	}

	public void Initialize(GameGlobals.GameMode mode)
	{
		_unitManager = GetParent().GetNode<UnitManager>("UnitManager");
		_towerManager = GetParent().GetNode<TDTowerManager>("TowerManager");
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

		switch (mode)
		{
			case GameGlobals.GameMode.Debug:
				UpdateHp(1000);
				IncreaseSpawnerLimit(_startingSpawnerLimit);
				

				_availTowerList = _allTowerList;
				
				_rewardManager._noStartingReward = true;
				UpdatePortalLimitButtonText();
				
				_rightPanel.GetNode<Button>("GetPassive").Show();
				_rightPanel.GetNode<Button>("GetAnyPassive").Show();
				InitializeFinalBoss();

				InitializeBossesArray();

				UpdateWaveIndexCounter();
				UpdateSpeedUpWaveButtonText();

				UpdateMoney(new Vector4I(99999, 99999, 99999, 99999));
				

				break;
			case GameGlobals.GameMode.Tutorial:
				UpdateHp(20);
				IncreaseSpawnerLimit(_startingSpawnerLimit);
				
				UpdatePortalLimitButtonText();
				

				_tutorialManager = GetParent().GetNode<TutorialManager>("TutorialLayer/TutorialManager");
				_tutorialManager.Initialize(this, _grid, _unitManager);
				_availTowerList = _allTowerList;
				_towerManager.InitializeTowersPanel(TowerUnit.TowerType.Defense);
				InitializeFinalBoss();

				InitializeBossesArray();
				UpdateWaveIndexCounter();
				UpdateSpeedUpWaveButtonText();
				UpdateMoney(_sandboxStartingMoney);
				
				break;
			case GameGlobals.GameMode.Continue:
				_saveManager.LoadGame();
				if (_gameMode == GameGlobals.GameMode.Rogue || _gameMode == GameGlobals.GameMode.Debug)
				{
					_rewardManager._noStartingReward = true;
				}
				else
				{
					_rewardManager._noStartingReward = false;
				}
				if (_gameMode == GameGlobals.GameMode.Debug)
				{
					_rightPanel.GetNode<Button>("GetPassive").Show();
					_rightPanel.GetNode<Button>("GetAnyPassive").Show();
				}
				UpdatePortalLimitButtonText();
				UpdateSpeedUpWaveButtonText();
				break;
			case GameGlobals.GameMode.Rogue:
				UpdateHp(20);
				IncreaseSpawnerLimit(_startingSpawnerLimit);
				
				
				UpdatePortalLimitButtonText();
				
				_availTowerList = [];
				_rewardManager._noStartingReward = false;
				InitializeFinalBoss();

				InitializeBossesArray();
				UpdateWaveIndexCounter();
				UpdateSpeedUpWaveButtonText();
				UpdateMoney(_rogueStartingMoney);
				break;
			case GameGlobals.GameMode.Normal:
			default:
				UpdateHp(20);
				IncreaseSpawnerLimit(_startingSpawnerLimit);
				_availTowerList = _allTowerList;

				_rewardManager._noStartingReward = false;

				UpdatePortalLimitButtonText();
				
				InitializeFinalBoss();

				InitializeBossesArray();
				UpdateWaveIndexCounter();
				UpdateMoney(_sandboxStartingMoney);
				UpdateSpeedUpWaveButtonText();
				break;
		}

		if (mode != GameGlobals.GameMode.Continue)
		{
			foreach (GlobalEffectResource resource in MetaManager.Instance._obtainedMetaUpgrades)
			{
				ApplyGlobalEffect(resource);
			}
		}

		_rewardManager.Initialize();

		InitializeWaveProgressionBar();

		_towerManager.InitializeTowersPanel(TowerUnit.TowerType.Defense);
	}

	public async void SpawnNextWave()
	{
		var waveCopy = _waveList.Duplicate();
		waveCopy.Add(_finalWave, _finalBoss);
		_saveManager.SaveGame(_gameMode, _money, _spawnerLimit, _hp, _waveIndex, _towerManager._allTowers, _availTowerList, _globalEffects, waveCopy, _rewardList, _speedUpWaveCount);
		_waveIndex++;
		_waveRewardGiven = false;
		EmitSignal(SignalName.NewWave);
		UpdateWaveIndexCounter();
		UpdateWaveProgressionBar();
		if (_waveList.Keys.Contains(_waveIndex))
		{
			Array<InvaderStatsIncreaseResource> enemyList = _waveList[_waveIndex];
			for (int i = 0; i < enemyList.Count; i++)
			{
				(InvaderStatsIncreaseResource enemy, float delay) = (enemyList[i], SpawnInterval * (float)Math.Pow(0.9f, enemyList.Count));
				InvaderUnit unit = SpawnEnemyAtEntrance(enemy._unitName);
				InvaderStatsIncreaseResource buff = new InvaderStatsIncreaseResource();
				_miniBossBuff.MergeWithOld(buff, []);
				buff._percentHpBuff += GetBossHpMultiplier(_waveIndex);
				buff.MergeWithOld(enemy, []);
				unit.AddEffect(enemy);
				if (delay > 0)
				{
					await ToSignal(GetTree().CreateTimer(delay, processAlways: false), SceneTreeTimer.SignalName.Timeout);
				}
			}
			EmitSignal(SignalName.GlobalBossSpawned);
		}
		else if (_waveIndex == _finalWave)
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
		else
		{
			OnWaveEnded();
		}
	}

	public bool CheckWaveFinished()
	{
		if (_invaderCount == 0)
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
		_waveRewardGiven = true;
		if (_tutorialManager is not null)
		{
			_tutorialManager.NextWave();
			_tutorialManager.Show();
		}
		if (_rewardManager is not null)
		{
			if (_rewardList.Keys.Contains(_waveIndex))
			{
				foreach (RewardManager.RewardType type in _rewardList[_waveIndex])
				{
					_rewardManager._choicesQueue.Add(type);
				}
				_rewardManager.MakeRewardPrompt(RewardManager.RewardSource.Boss);
			}
		}
	}

	public void UpdateWaveIndexCounter()
	{
		_waveCounter.Text = "Cycle " + _waveIndex;
		UpdateSpeedUpWaveButtonText();
	}

	public void InitializeWaveProgressionBar()
	{
		_progressionBar.MaxValue = _finalWave;
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
		for (int i = 0; i < _finalWave + 1; i++)
		{
			float yPos = (float)i / (float)_finalWave * width;
			if (i == _finalWave)
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
			else if (_waveList.Keys.Contains(i))
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

				string displayName = Spawner.GetGroupEnemyNames(_waveList[i]); ;

				int j = _waveList.Keys.ToList().IndexOf(i);

				if (j >= _lateBossStartsAt)
				{
					rtl.Text = $"[url={displayName + $"\n {GetBossHpMultiplier(i) * 100:F0}% more HP \n" + StringDB.Entries["MiniBoss"]}]::lateminiboss::[/url]";
				}
				else if (j >= _midBossStartsAt)
				{
					rtl.Text = $"[url={displayName + $"\n {GetBossHpMultiplier(i) * 100:F0}% more HP \n" + StringDB.Entries["MiniBoss"]}]::midminiboss::[/url]";
				}
				else
				{
					rtl.Text = $"[url={displayName + $"\n {GetBossHpMultiplier(i) * 100:F0}% more HP \n" + StringDB.Entries["MiniBoss"]}]::miniboss::[/url]";
				}


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

	public void AddRandomBossToWave(int wave)
	{
		if (_remainingBosses.Count == 0)
		{
			return;
		}

		Array<SpawnerDataResource> boss;
		if (_waveList.Count >= _lateBossStartsAt)
		{
			Array<SpawnerDataResource> remainingLateBosses = [];
			foreach (SpawnerDataResource invader in _remainingBosses)
			{
				if (_lateBosses.Contains(invader))
				{
					remainingLateBosses.Add(invader);
				}
			}
			boss = Utils.GetRandomElements<SpawnerDataResource>(remainingLateBosses, 1);
		}
		else if (_waveList.Count >= _midBossStartsAt)
		{
			Array<SpawnerDataResource> remainingMidBosses = [];
			foreach (SpawnerDataResource invader in _remainingBosses)
			{
				if (_midBosses.Contains(invader))
				{
					remainingMidBosses.Add(invader);
				}
			}
			boss = Utils.GetRandomElements<SpawnerDataResource>(remainingMidBosses, 1);
		}
		else
		{
			Array<SpawnerDataResource> remainingEarlyBosses = [];
			foreach (SpawnerDataResource invader in _remainingBosses)
			{
				if (_earlyBosses.Contains(invader))
				{
					remainingEarlyBosses.Add(invader);
				}
			}
			boss = Utils.GetRandomElements<SpawnerDataResource>(remainingEarlyBosses, 1);
		}

			
		_remainingBosses.Remove(boss[0]);
		_waveList.Add(wave, boss[0]._units);
		if (_rewardManager is not null)
		{
			_rewardList.Add(wave, [RewardManager.RewardType.Tower, RewardManager.RewardType.Passive]);
		}
	}

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
		if (wave != _finalWave)
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

	public void AddRewardToWave(int wave, Array<RewardManager.RewardType>reward)
	{
		_rewardList.Add(wave, reward);
	}

	public void InitializeBossesArray()
	{
		int index = 1;
		while (index < _finalWave)
		{
			if (index % BossInterval == 0)
			{
				AddRandomBossToWave(index);
			}
			index++;
		}
	}

	public void InitializeFinalBoss()
	{
		Array<SpawnerDataResource> boss = Utils.GetRandomElements<SpawnerDataResource>(_finalBosses, 1);
		_finalBoss = boss[0]._units;
	}

	public void SpeedUpBosses(int n)
	{
		int m = _waveList.Keys.Max();
		if (_finalWave - m >= BossInterval - 1)
		{
			AddRandomBossToWave(m + BossInterval);
		}

		var newWaveList = new Godot.Collections.Dictionary<int, Array<InvaderStatsIncreaseResource>>();

		foreach (var (key, value) in _waveList)
		{
			if (key > _waveIndex)
			{
				newWaveList[key - n] = value;
			}
			else
			{
				newWaveList[key] = value;
			}
		}

		_waveList = newWaveList;

		var newRewardList = new Godot.Collections.Dictionary<int, Array<RewardManager.RewardType>>();

		foreach (var (key, value) in _rewardList)
		{
			if (key > _waveIndex)
			{
				newRewardList[key - n] = value;
			}
			else
			{
				newRewardList[key] = value;
			}
		}

		_rewardList = newRewardList;

		m = _waveList.Keys.Max();
		if (_finalWave - m >= BossInterval - 1)
		{
			AddRandomBossToWave(m + BossInterval);
		}
		UpdateWaveProgressionBar();
	}

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

	public float GetBossHpMultiplier(int index)
	{
		int i = _waveList.Keys.ToList().IndexOf(index);
		if (i >= _lateBossStartsAt)
		{
			i -= _lateBossStartsAt;
		}
		else if (i >= _midBossStartsAt)
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

	public void BuySpeedUpWave()
	{
		if (_waveList.Keys.Contains(_waveIndex + 1))
		{
			return;
		}
		int cost = GetSpeedUpWaveCost();
		if (_money[0] < cost)
		{
			return;
		}
		_speedUpWaveCount++;
		
		SpeedUpBosses(GetNextBossWave() - _waveIndex - 1);
		SpendMoney(new Vector4I(cost, 0, 0, 0));
		UpdateSpeedUpWaveButtonText();
	}

	public void UpdateSpeedUpWaveButtonText()
	{
		if (!_waveList.Keys.Contains(_waveIndex + 1))
		{
			int cost = GetSpeedUpWaveCost();
			_speedUpWaveButtonText.Text = "Call mini boss now\n" + Utils.MakeMoneyText(new Vector4I(cost, 0, 0, 0));
			_speedUpWaveButtonText.AddThemeColorOverride("default_color", GetSpeedUpWaveButtonTextColor());
		}
		else
		{
			_speedUpWaveButtonText.Text = "Mini boss next wave!\n";
			_speedUpWaveButtonText.AddThemeColorOverride("default_color", GetSpeedUpWaveButtonTextColor());
		}
	}

	public int GetSpeedUpWaveCost()
	{
		if (_waveList.Count > 0)
		{
			return 20 * (int)Math.Pow(2, _speedUpWaveCount) / 4 * (GetNextBossWave() - _waveIndex - 1);
		}
		else
		{
			return 0;
		}
	}

	public void FlashMoney()
	{
		_moneyLabel.FlashRed();
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
		_invaderCount++;
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
		_invaderCount++;
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
		_invaderCount++;
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
		_invaderCount++;
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
			_waveList.Add(_finalWave, _finalBoss);
			_saveManager.SaveGame(_gameMode, _money, _spawnerLimit, _hp, _waveIndex, _towerManager._allTowers, _availTowerList, _globalEffects, _waveList, _rewardList, _speedUpWaveCount);
		}
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
		_moneyLabel.Text = Utils.MakeMoneyText(_money, true);
		if (_infoPanel.GetSelectedUnit() is not null)
		{
			_infoPanel.GetSelectedUnit().EmitSignal(Unit.SignalName.UpdateUpgradeButton);
		}
		_spawnerLimitIncreaseButton.UpdateAffordabilityDisplay(Utils.VectorDivision(newMoney, new(GetPortalLimitUpgradeCost(),0,0,0)));
		_spawnerLimitIncreaseButtonText.AddThemeColorOverride("default_color", GetSpawnerLimitIncreaseButtonTextColor());

		_speedUpWaveButton.UpdateAffordabilityDisplay(Utils.VectorDivision(newMoney, new(GetSpeedUpWaveCost(), 0, 0, 0)));
		_speedUpWaveButtonText.AddThemeColorOverride("default_color", GetSpeedUpWaveButtonTextColor());
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

	private Color GetSpeedUpWaveButtonTextColor()
	{
		if (GetSpeedUpWaveCost() <= 0)
		{
			return ThemePalette.Green;
		}
		if (Utils.VectorDivision(_money, new(GetSpeedUpWaveCost(), 0, 0, 0)) >= 1f)
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
	
	private void OnUnitDied(Unit unit)
	{
		if (unit is InvaderUnit invader)
		{
			GainMoney(invader.GetSelfMoneyDropped());
			_invaderCount--;
			GetTree().CreateTimer(0.5f, processAlways: false).Timeout += () =>
			{
				if (CheckWaveFinished() && !_waveRewardGiven)
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
		_invaderCount--;
		GetTree().CreateTimer(0.5f, processAlways: false).Timeout += () =>
		{
			if (CheckWaveFinished())
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
			return (int)waves.Max() + BossInterval;
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

	public void ApplyGlobalEffect(GlobalEffectResource resource)
	{
		int test = _allGlobalEffects.IndexOf(resource);
		GlobalEffectManager.Apply(resource, this);
		_globalEffects.Add(resource);
		AddGlobalEffectToContainer(resource);
	}

	public void ApplyGlobalEffect(int index)
	{
		ApplyGlobalEffect(_allGlobalEffects[index]);
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
