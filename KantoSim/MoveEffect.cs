using System;
using System.Linq;

namespace KantoSim
{
    public readonly struct MoveEffect
    {
        public readonly bool OnUser;
        public readonly MoveEffectPossibility[] Possibilities;

        public MoveEffect(bool onUser, MoveEffectPossibility[] possibilities)
        {
            OnUser = onUser;
            Possibilities = possibilities;
            if (Possibilities.Length > 0 && Possibilities.Sum(p => p.Chance) != 1.0)
                throw new ArgumentException("The chances of the possibilities do not sum to 100%!", "possibilities");
        }

        public static readonly MoveEffect None = new MoveEffect(false, new MoveEffectPossibility[0]);

        public static MoveEffect Single(bool onUser, object specifier, int scale)
        {
            return new MoveEffect(onUser, MoveEffectPossibility.Single(specifier, scale));
        }

        public static MoveEffect SingleChance(bool onUser, object specifier, int scale, double chance)
        {
            return new MoveEffect(onUser, MoveEffectPossibility.SingleChance(specifier, scale, chance));
        }
    }
}
