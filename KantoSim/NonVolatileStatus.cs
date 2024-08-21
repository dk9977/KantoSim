using System;
using System.Collections.Generic;
using System.Text;

namespace KantoSim
{
    public class NonVolatileStatus
    {
        private double _skipChance;
        private bool _damages;
        private byte _duration;
        private Func<Battler, bool> _extraEffect;

        private NonVolatileStatus(double skipChance, bool damages, byte duration, Func<Battler, bool> extraEffect)
        {
            _skipChance = skipChance;
            _damages = damages;
            _duration = duration;
            _extraEffect = extraEffect;
        }

        public byte Duration { get => _duration; }

        private static bool BurnEffect(Battler battler)
        {
            // divide Attack by 2
            return true;
        }
        private static bool ParalyzeEffect(Battler battler)
        {
            // divide Speed by 4
            return true;
        }

        public static NonVolatileStatus Burn = new NonVolatileStatus(0, true, 0, (b) => BurnEffect(b));
        public static NonVolatileStatus Freeze = new NonVolatileStatus(1, false, 0, null);
        public static NonVolatileStatus Paralysis = new NonVolatileStatus(0.25, false, 0, (b) => ParalyzeEffect(b));
        public static NonVolatileStatus Poison = new NonVolatileStatus(0, true, 0, null);
        public static NonVolatileStatus Sleep = new NonVolatileStatus(1, true, 7, null);
    }
}
