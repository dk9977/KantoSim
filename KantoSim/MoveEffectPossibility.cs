using System;

namespace KantoSim
{
    public readonly struct MoveEffectPossibility
    {
        public readonly int Scale;
        public readonly double Chance;

        public MoveEffectPossibility(int scale, double chance)
        {
            Scale = scale;
            Chance = chance;
        }

        public static MoveEffectPossibility[] Single(int scale) => new MoveEffectPossibility[] {
            new MoveEffectPossibility(scale, 1.0)
        };

        public static MoveEffectPossibility[] SingleChance(int scale, double chance)
        {
            if (chance > 1.0 || chance <= 0.0)
                throw new ArgumentOutOfRangeException("Chance is out of range!", "chance");
            return new MoveEffectPossibility[]
            {
                new MoveEffectPossibility(scale, chance),
                new MoveEffectPossibility(0, 1 - chance)
            };
        }
    }
}
