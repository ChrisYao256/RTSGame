using RTSGame.Units;
using System;

namespace RTSGame._Source.Units;

public interface IStackable
{
	EffectResource MultiplyEffect(int n);
}
