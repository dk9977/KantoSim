using System;
using System.Linq;

namespace KantoSim
{
    public readonly struct MoveEffect
    {
        public readonly bool OnUser;
        public readonly object Specifier;
        public readonly MoveEffectPossibility[] Possibilities;

        public MoveEffect(bool onUser, object specifier, MoveEffectPossibility[] possibilities)
        {
            OnUser = onUser;
            Specifier = specifier;
            Possibilities = possibilities;
            if (Possibilities.Length > 0 && Possibilities.Sum(p => p.Chance) != 1.0)
                throw new ArgumentException("The chances of the possibilities do not sum to 100%!", "possibilities");
        }

        public static readonly MoveEffect None = new MoveEffect(false, null, new MoveEffectPossibility[0]);

        public static MoveEffect Single(bool onUser, object specifier, int scale)
        {
            return new MoveEffect(onUser, specifier, MoveEffectPossibility.Single(scale));
        }

        public static MoveEffect SingleChance(bool onUser, object specifier, int scale, double chance)
        {
            return new MoveEffect(onUser, specifier, MoveEffectPossibility.SingleChance(scale, chance));
        }
    }
}
