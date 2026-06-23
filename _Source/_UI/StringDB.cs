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
	};
}
