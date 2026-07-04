using RTSGame.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame._Source.Units;

public interface IStackable
{
	EffectResource MultiplyEffect(int n);
}
