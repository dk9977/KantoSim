using System;
using System.Collections.Generic;
using System.Text;

namespace KantoSim
{
    public sealed class Type
    {
        private static readonly uint[] chart = {
            0b10101010101010101010101010101010, // None
            0b10101010101010101010101010010010, // Normal
            0b10100101101111101010101011011001, // Fire
            0b10101101100110101011101010111001, // Water
            0b10101011010110101000111010101001, // Electric
            0b10100111100110100111011001111001, // Grass
            0b10101001101101101011111010101011, // Ice
            0b10111010101011100110010101110010, // Fighting
            0b10101010101110100101101011010110, // Poison
            0b10101110110110101110001001111010, // Ground
            0b10101010011110111010101011011010, // Flying
            0b10101010101010111110100110101010, // Psychic
            0b10100110101110011110011110101010, // Bug
            0b10101110101011011001111011101010, // Rock
            0b10001010101010101010100010101110, // Ghost
            0b10101010101010101010101010101011  // Dragon
        };
        private const uint majorFlagNone = 0b10000000000000000000000000000000;
        private const uint minorFlagNone = 0b01000000000000000000000000000000;
        private const double ineffective = 0.0;
        private const double notVeryEffective = 0.5;
        private const double effective = 1.0;
        private const double superEffective = 2.0;

        private readonly byte _index;
        private readonly string _name;
        private readonly Category _damageCategory;

        public string Name { get => _name; }
        public Category DamageCategory { get => _damageCategory; }

        private Type(byte index, string name, Category damageCategory)
        {
            _index = index;
            _name = name;
            _damageCategory = damageCategory;
        }

        public static Type None = new Type(0, "none", Category.None);
        public static Type Normal = new Type(1, "normal", Category.Physical);
        public static Type Fire = new Type(2, "fire", Category.Special);
        public static Type Water = new Type(3, "water", Category.Special);
        public static Type Electric = new Type(4, "electric", Category.Special);
        public static Type Grass = new Type(5, "grass", Category.Special);
        public static Type Ice = new Type(6, "ice", Category.Special);
        public static Type Fighting = new Type(7, "fighting", Category.Physical);
        public static Type Poison = new Type(8, "poison", Category.Physical);
        public static Type Ground = new Type(9, "ground", Category.Physical);
        public static Type Flying = new Type(10, "flying", Category.Physical);
        public static Type Psychic = new Type(11, "psychic", Category.Special);
        public static Type Bug = new Type(12, "bug", Category.Physical);
        public static Type Rock = new Type(13, "rock", Category.Physical);
        public static Type Ghost = new Type(14, "ghost", Category.Physical);
        public static Type Dragon = new Type(15, "dragon", Category.Special);

        public double EffectivenessMultiplier(Type d)
        {
            uint row = chart[_index];
            int i2 = d._index * 2;
            uint major = majorFlagNone >> i2;
            uint minor = minorFlagNone >> i2;
            return (row & major) == 0
                ? (row & minor) == 0 ? ineffective : notVeryEffective
                : (row & minor) == 0 ? effective : superEffective;
        }

        public double EffectivenessMultiplier(Type d0, Type d1) => EffectivenessMultiplier(d0) * EffectivenessMultiplier(d1);

        public enum Effectiveness
        {
            Ineffective,
            NotVeryEffective,
            Effective,
            SuperEffective
        }

        public Effectiveness EffectivenessAgainst(Type t) => (Effectiveness)((chart[_index] >> (t._index << 1)) & 0b11);
    }
}
