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
		{ "YellowMoneyDescription", "Biofuel. Dropped by most enemies."},
		{ "RedMoneyDescription", "Metals. Typically dropped by tough enemies."},
		{ "BlueMoneyDescription", "Water. Typically dropped by enemies that weld magic."},
		{ "GreenMoneyDescription", "Gas. Typically dropped by fast enemies."},

		{ "TargetPriorityFirst", "Target the enemy closest to the exit"},
		{ "TargetPriorityLast", "Target the enemy furthest to the exit"},
		{ "TargetPriorityClosest", "Target the enemy closest to this"},
		{ "TargetPriorityStrongest", "Target the enemy with the most max Hp"},
		{ "TargetPriorityScannerWeapon", "Same as First, but prioritize enemies that don't have Analyzed."},
	};
}
