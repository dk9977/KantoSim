using System;

namespace KantoSim
{
    public readonly struct MoveEffectPossibility
    {
        public readonly object Specifier;
        public readonly int Scale;
        public readonly double Chance;

        public MoveEffectPossibility(object specifier, int scale, double chance)
        {
            Specifier = specifier;
            Scale = scale;
            Chance = chance;
        }

        public static MoveEffectPossibility[] Single(object specifier, int scale) => new MoveEffectPossibility[] {
            new MoveEffectPossibility(specifier, scale, 1.0)
        };

        public static MoveEffectPossibility[] SingleChance(object specifier, int scale, double chance)
        {
            if (chance > 1.0 || chance <= 0.0)
                throw new ArgumentOutOfRangeException("Chance is out of range!", "chance");
            return new MoveEffectPossibility[]
            {
                new MoveEffectPossibility(specifier, scale, chance),
                new MoveEffectPossibility(specifier, 0, 1 - chance)
            };
        }
    }
}
