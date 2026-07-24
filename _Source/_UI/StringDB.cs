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
		{ "YellowMoneyDescription", "Electricity. Dropped by most enemies."},
		{ "RedMoneyDescription", "Metals. Typically dropped by durable enemies."},
		{ "BlueMoneyDescription", "Water. Typically dropped by enemies with potent magic."},
		{ "GreenMoneyDescription", "Gas. Typically dropped by fast enemies."},
		{ "UnknownMoneyDescription", "Undetermined type. Represents income from certain special effects."},

		{ "TargetPriorityFirst", "Target the enemy closest to the exit"},
		{ "TargetPriorityLast", "Target the enemy furthest to the exit"},
		{ "TargetPriorityClosest", "Target the enemy closest to this"},
		{ "TargetPriorityStrongest", "Target the enemy with the most max Hp"},
		{ "TargetPriorityScannerWeapon", "Same as First, but prioritize enemies that don't have Analyzed."},
		{ "TargetPrioritySniperHitscanWeapon", "Target the enemy with the lowest Hp."},

		{ "TowerChoice", "Pick a tower to unlock this run.\n You are also given the cost of picked tower:)"},
	};
}
