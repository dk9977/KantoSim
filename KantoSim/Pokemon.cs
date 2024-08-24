using System;
using System.Collections.Generic;
using System.Text;

namespace KantoSim
{
    public class Pokemon
    {
        private class EVSpread
        {
            private readonly ushort _hpIv;
            private readonly ushort _atkIv;
            private readonly ushort _defIv;
            private readonly ushort _spcIv;
            private readonly ushort _speIv;

            public EVSpread(ushort hp, ushort atk, ushort def, ushort spc, ushort spe)
            {
                _hpIv = hp;
                _atkIv = atk;
                _defIv = def;
                _spcIv = spc;
                _speIv = spe;
            }

            public ushort Hp => _hpIv;
            public ushort Atk => _atkIv;
            public ushort Def => _defIv;
            public ushort Spc => _spcIv;
            public ushort Spe => _speIv;
        }
        private class IVSpread
        {
            private const int hpIvSection = 0b00000001111100000000000000000000;
            private const int atkIvSection = 0b00000000000011111000000000000000;
            private const int defIvSection = 0b00000000000000000111110000000000;
            private const int spcIvSection = 0b00000000000000000000001111100000;
            private const int speIvSection = 0b00000000000000000000000000011111;
            private const byte hpOff = 20;
            private const byte atkOff = 15;
            private const byte defOff = 10;
            private const byte spcOff = 5;
            private const byte speOff = 0;

            private readonly int _value;

            public IVSpread(byte hp, byte atk, byte def, byte spc, byte spe)
            {
                _value = hp << hpOff;
                _value += atk << atkOff;
                _value += def << defOff;
                _value += spc << spcOff;
                _value += spe << speOff;
            }

            public int Hp => (_value & hpIvSection) >> hpOff;
            public int Atk => (_value & atkIvSection) >> atkOff;
            public int Def => (_value & defIvSection) >> defOff;
            public int Spc => (_value & spcIvSection) >> spcOff;
            public int Spe => (_value & speIvSection) >> speOff;
        }

        private readonly Species _species;
        private ushort _currentHp;
        private NonVolatileStatus _status;
        private readonly Move _m1;
        private readonly Move _m2;
        private readonly Move _m3;
        private readonly Move _m4;
        // XP?
        private readonly EVSpread _evs;
        private readonly IVSpread _ivs;
        private byte _pp1;
        private byte _pp2;
        private byte _pp3;
        private byte _pp4;
        private byte _level;
        private readonly ushort _maxHp;
        private readonly ushort _atk;
        private readonly ushort _def;
        private readonly ushort _spc;
        private readonly ushort _spe;

        private Pokemon(Species species, byte level, Move m1, Move m2, Move m3, Move m4, ushort hpev, ushort atkev, ushort defev, ushort spcev, ushort speev, byte hpiv, byte atkiv, byte defiv, byte spciv, byte speiv)
        {
            _species = species;
            _level = level;
            _m1 = m1;
            _m2 = m2;
            _m3 = m3;
            _m4 = m4;
            _evs = new EVSpread(hpev, atkev, defev, spcev, speev);
            _ivs = new IVSpread(hpiv, atkiv, defiv, spciv, speiv);
            _maxHp = (ushort)(((_species.Hp + _ivs.Hp) * 2 + (int)Math.Sqrt(_evs.Hp) / 4) * _level / 100 + _level + 10);
            _atk = (ushort)(((_species.Atk + _ivs.Atk) * 2 + (int)Math.Sqrt(_evs.Atk) / 4) * _level / 100 + 5);
            _def = (ushort)(((_species.Def + _ivs.Def) * 2 + (int)Math.Sqrt(_evs.Def) / 4) * _level / 100 + 5);
            _spc = (ushort)(((_species.Spc + _ivs.Spc) * 2 + (int)Math.Sqrt(_evs.Spc) / 4) * _level / 100 + 5);
            _spe = (ushort)(((_species.Spe + _ivs.Spe) * 2 + (int)Math.Sqrt(_evs.Spe) / 4) * _level / 100 + 5);
            _currentHp = _maxHp;
            _status = null;
        }

        public ushort CurrentHp
        {
            get => _currentHp;
            private set => _currentHp = Math.Min(Math.Max((ushort)0, value), _maxHp);
        }
        public NonVolatileStatus Status
        {
            get => _status;
            private set => _status = value;
        }
        public Type Type0 { get => _species.Type0; }
        public Type Type1 { get => _species.Type1; }
        public Move M1 { get => _m1; }
        public Move M2 { get => _m2; }
        public Move M3 { get => _m3; }
        public Move M4 { get => _m4; }
        public byte PP1 { get => _pp1; }
        public byte PP2 { get => _pp2; }
        public byte PP3 { get => _pp3; }
        public byte PP4 { get => _pp4; }
        public byte Level { get => _level; }
        public ushort MaxHp { get => _maxHp; }
        public ushort Atk { get => _atk; }
        public ushort Def { get => _def; }
        public ushort Spc { get => _spc; }
        public ushort Spe { get => _spe; }

        public bool Damage(ushort hp)
        {
            CurrentHp -= hp;
            return CurrentHp != 0;
        }
        public void Heal(ushort hp)
        {
            CurrentHp += hp;
        }
        public bool SetStatus(NonVolatileStatus status)
        {
            if (Status != null)
                return false;
            Status = status;
            return true;
        }
    }
}
