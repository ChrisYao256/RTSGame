using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame.Units;
public class StringDB
{
	public static Dictionary<string, string> Entries = new()
	{
		{ "YellowMoneyDescription", "Electricity"},
		{ "RedMoneyDescription", "Metals"},
		{ "BlueMoneyDescription", "Water"},
		{ "GreenMoneyDescription", "Gas"},
		{ "UnknownMoneyDescription", "Undetermined type"},

		{ "ProjectileWeapon", "Projectile"},
		{ "LaserWeapon", "Laser"},
		{ "FlameWeapon", "Flame"},
		{ "ScannerWeapon", "Scanner"},
		{ "BallisticWeapon", "Ballistic"},
		{ "ElectricWeapon", "Electric"},

		{ "TargetPriorityFirst", "Target the enemy closest to the exit"},
		{ "TargetPriorityLast", "Target the enemy furthest to the exit"},
		{ "TargetPriorityClosest", "Target the enemy closest to this"},
		{ "TargetPriorityStrongest", "Target the enemy with the most max Hp"},
		{ "TargetPriorityScannerWeapon", "Same as First, but prioritize enemies that don't have Analyzed."},
		{ "TargetPrioritySniperHitscanWeapon", "Target the enemy with the lowest Hp."},

		{ "TowerChoice", "Pick a defense or portal to unlock this run."},
		{ "DefenseChoice", "Pick a defense to unlock this run."},
		{ "PortalChoice", "Pick a portal to unlock this run."},
		{ "PassiveChoice", "Pick a passive to get this run."},

		{ "MiniBoss", "A mini boss. Defeat it to get a tower and a passive. "},
		{ "FinalBoss", "The final boss of this run."},

		{ "WinWindow", "Run complete!"},
		{ "LossWindow", "Run failed!"},
	};
}
