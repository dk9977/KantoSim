using System;
using System.Collections.Generic;
using System.Text;

namespace KantoSim
{
    public class Battler
    {
        public enum StatMod
        {
            Atk,
            Def,
            Spc,
            Spe,
            Acc,
            Eva
        }

        public enum VolatileStatus
        {
            BadlyPoisoned,
            Biding,
            Binding,
            Bound,
            Charging,
            Confused,
            Disabled,
            Flinching,
            FlyingHigh,
            Minimized,
            Pumped,
            Rampaging,
            Recharging,
            Seeded,
            Substituted,
            Transformed,
            Underground
        }

        private class StatModSpread
        {
            private const int clear      = 0b100010001000100010001000;
            private const int atkSection = 0b111100000000000000000000;
            private const int defSection = 0b000011110000000000000000;
            private const int spcSection = 0b000000001111000000000000;
            private const int speSection = 0b000000000000111100000000;
            private const int accSection = 0b000000000000000011110000;
            private const int evaSection = 0b000000000000000000001111;
            private const int clearOff =  8;
            private const int atkOff   = 20;
            private const int defOff   = 16;
            private const int spcOff   = 12;
            private const int speOff   =  8;
            private const int accOff   =  4;
            private const int evaOff   =  0;

            private int _value;

            public StatModSpread()
            {
                _value = clear;
            }

            public int Atk
            {
                get => ((_value & atkSection) >> atkOff) - clearOff;
                set
                {
                    _value &= ~atkSection;
                    _value |= (value + clearOff) << atkOff;
                }
            }
            public int Def
            {
                get => ((_value & defSection) >> defOff) - clearOff;
                set
                {
                    _value &= ~defSection;
                    _value |= (value + clearOff) << defOff;
                }
            }
            public int Spc
            {
                get => ((_value & spcSection) >> spcOff) - clearOff;
                set
                {
                    _value &= ~spcSection;
                    _value |= (value + clearOff) << spcOff;
                }
            }
            public int Spe
            {
                get => ((_value & speSection) >> speOff) - clearOff;
                set
                {
                    _value &= ~speSection;
                    _value |= (value + clearOff) << speOff;
                }
            }
            public int Acc
            {
                get => ((_value & accSection) >> accOff) - clearOff;
                set
                {
                    _value &= ~accSection;
                    _value |= (value + clearOff) << accOff;
                }
            }
            public int Eva
            {
                get => ((_value & evaSection) >> evaOff) - clearOff;
                set
                {
                    _value &= ~evaSection;
                    _value |= (value + clearOff) << evaOff;
                }
            }

            public bool ModifyStat(StatMod stat, sbyte stages)
            {
                int v;
                switch (stat)
                {
                    case StatMod.Atk:
                        v = Atk + stages;
                        if (v <= 6 && v >= -6)
                        {
                            Atk = v;
                            return true;
                        }
                        return false;
                    case StatMod.Def:
                        v = Def + stages;
                        if (v <= 6 && v >= -6)
                        {
                            Def = v;
                            return true;
                        }
                        return false;
                    case StatMod.Spc:
                        v = Spc + stages;
                        if (v <= 6 && v >= -6)
                        {
                            Spc = v;
                            return true;
                        }
                        return false;
                    case StatMod.Spe:
                        v = Spe + stages;
                        if (v <= 6 && v >= -6)
                        {
                            Spe = v;
                            return true;
                        }
                        return false;
                    case StatMod.Acc:
                        v = Acc + stages;
                        if (v <= 6 && v >= -6)
                        {
                            Acc = v;
                            return true;
                        }
                        return false;
                    case StatMod.Eva:
                        v = Eva + stages;
                        if (v <= 6 && v >= -6)
                        {
                            Eva = v;
                            return true;
                        }
                        return false;
                    default:
                        return false;
                }
            }
        }

        private class VolatileStatusSpread
        {
            public bool BadlyPoisoned { get; private set; }
            public bool Biding { get; private set; }
            public bool Binding { get; private set; }
            public bool Bound { get; private set; }
            public bool Charging { get; private set; }
            public bool Confused { get; private set; }
            public bool Disabled { get; private set; }
            public bool Flinching { get; private set; }
            public bool FlyingHigh { get; private set; }
            public bool Minimized { get; private set; }
            public bool Pumped { get; private set; }
            public bool Rampaging { get; private set; }
            public bool Recharging { get; private set; }
            public bool Seeded { get; private set; }
            public bool SemiInvulnerable { get; private set; }
            public bool Substituted { get; private set; }
            public bool Transformed { get; private set; }
            public bool Underground { get; private set; }
            public byte BideRound { get; private set; }
            public byte BindRound { get; private set; }
            public byte DisabledMove { get; private set; }
            public byte RampageRound { get; private set; }
            public byte SubstituteHealth { get; private set; }
            public byte ToxicN { get; private set; }
        }

        private readonly Pokemon _identity;
        private readonly StatModSpread _statMods;
        private readonly VolatileStatusSpread _volatileStatuses;

        public Battler(Pokemon identity)
        {
            _identity = identity;
            _statMods = new StatModSpread();
        }

        public bool Damage(ushort hp) => _identity.Damage(hp);
        public void Heal(ushort hp) => _identity.Heal(hp);
        public bool ModifyStat(StatMod stat, sbyte stages) => _statMods.ModifyStat(stat, stages);
        
    }
}
