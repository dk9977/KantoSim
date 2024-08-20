using System;
using System.Collections.Generic;
using System.Text;

namespace KantoSim
{
    public sealed class Species
    {
        private readonly string _name;
        private readonly Type _type0;
        private readonly Type _type1;
        private readonly byte _hp;
        private readonly byte _atk;
        private readonly byte _def;
        private readonly byte _spc;
        private readonly byte _spe;

        public string Name { get => _name; }
        public Type Type0 { get => _type0; }
        public Type Type1 { get => _type1; }
        public byte Hp { get => _hp; }
        public byte Atk { get => _atk; }
        public byte Def { get => _def; }
        public byte Spc { get => _spc; }
        public byte Spe { get => _spe; }

        private Species(string name, Type type0, Type type1, byte hp, byte atk, byte def, byte spc, byte spe)
        {
            _name = name;
            _type0 = type0;
            _type1 = type1;
            _hp = hp;
            _atk = atk;
            _def = def;
            _spc = spc;
            _spe = spe;
        }

        private readonly Species[] pokedex = {
            new Species("Bulbasaur", Type.Grass, Type.Poison, 45, 49, 49, 65, 45),
            new Species("Ivysaur", Type.Grass, Type.Poison, 60, 62, 63, 80, 60),
            new Species("Venusaur", Type.Grass, Type.Poison, 80, 82, 83, 100, 80),
            new Species("Charmander", Type.Fire, Type.None, 39, 52, 43, 50, 65),
            new Species("Charmeleon", Type.Fire, Type.None, 58, 64, 58, 65, 80),
            new Species("Charizard", Type.Fire, Type.Flying, 78, 84, 78, 85, 100),
            new Species("Squirtle", Type.Water, Type.None, 44, 48, 65, 50, 43),
            new Species("Wartortle", Type.Water, Type.None, 59, 63, 80, 65, 58),
            new Species("Blastoise", Type.Water, Type.None, 79, 83, 100, 85, 78),
            new Species("Caterpie", Type.Bug, Type.None, 45, 30, 35, 20, 45),
            new Species("Metapod", Type.Bug, Type.None, 50, 20, 55, 25, 30),
            new Species("Butterfree", Type.Bug, Type.Flying, 60, 45, 50, 80, 70),
            new Species("Weedle", Type.Bug, Type.Poison, 40, 35, 30, 20, 50),
            new Species("Kakuna", Type.Bug, Type.Poison, 45, 25, 50, 25, 35),
            new Species("Beedrill", Type.Bug, Type.Poison, 65, 80, 40, 45, 75),
            new Species("Pidgey", Type.Normal, Type.Flying, 40, 45, 40, 35, 56),
            new Species("Pidgeotto", Type.Normal, Type.Flying, 63, 60, 55, 50, 71),
            new Species("Pidgeot", Type.Normal, Type.Flying, 83, 80, 75, 70, 91),
            new Species("Rattatta", Type.Normal, Type.None, 30, 56, 35, 25, 72),
            new Species("Raticate", Type.Normal, Type.None, 55, 81, 60, 50, 97),
            new Species("Spearow", Type.Normal, Type.Flying, 40, 60, 30, 31, 70),
            new Species("Fearow", Type.Normal, Type.Flying, 65, 90, 65, 61, 100),
            new Species("Ekans", Type.Poison, Type.None, 35, 60, 44, 40, 55),
            new Species("Arbok", Type.Poison, Type.None, 60, 85, 69, 65, 80),
            new Species("Pikachu", Type.Electric, Type.None, 35, 55, 30, 50, 90),
            new Species("Raichu", Type.Electric, Type.None, 60, 90, 55, 90, 100),
            new Species("Sandshrew", Type.Ground, Type.None, 50, 75, 85, 30, 40),
            new Species("Sandslash", Type.Ground, Type.None, 75, 100, 110, 55, 65),
            new Species("Nidoran-F", Type.Poison, Type.None, 55, 47, 52, 40, 41),
            new Species("Nidorina", Type.Poison, Type.None, 70, 62, 67, 55, 56),
            new Species("Nidoqueen", Type.Poison, Type.Ground, 90, 82, 87, 75, 76),
            new Species("Nidoran-M", Type.Poison, Type.None, 46, 57, 40, 40, 50),
            new Species("Nidorino", Type.Poison, Type.None, 61, 72, 57, 55, 65),
            new Species("Nidoking", Type.Poison, Type.Ground, 81, 92, 77, 75, 85),
            new Species("Clefairy", Type.Normal, Type.None, 70, 45, 48, 60, 35),
            new Species("Clefable", Type.Normal, Type.None, 95, 70, 73, 85, 60),
            new Species("Vulpix", Type.Fire, Type.None, 38, 41, 40, 65, 65),
            new Species("Ninetales", Type.Fire, Type.None, 73, 76, 75, 100, 100),
            new Species("Jigglypuff", Type.Normal, Type.None, 115, 45, 20, 25, 20),
            new Species("Wigglytuff", Type.Normal, Type.None, 140, 70, 45, 50, 45),
            new Species("Zubat", Type.Poison, Type.Flying, 40, 45, 35, 40, 55),
            new Species("Golbat", Type.Poison, Type.Flying, 75, 80, 70, 75, 90),
            new Species("Oddish", Type.Grass, Type.Poison, 45, 50, 55, 75, 30),
            new Species("Gloom", Type.Grass, Type.Poison, 60, 65, 70, 85, 40),
            new Species("Vileplume", Type.Grass, Type.Poison, 75, 80, 85, 100, 50),
            new Species("Paras", Type.Bug, Type.Grass, 35, 70, 55, 55, 25),
            new Species("Parasect", Type.Bug, Type.Grass, 60, 95, 80, 80, 30),
            new Species("Venonat", Type.Bug, Type.Poison, 60, 55, 50, 40, 45),
            new Species("Venomoth", Type.Bug, Type.Poison, 70, 65, 60, 90, 90),
            new Species("Diglett", Type.Ground, Type.None, 10, 55, 25, 45, 95),
            new Species("Dugtrio", Type.Ground, Type.None, 35, 80, 50, 70, 120),
            new Species("Meowth", Type.Normal, Type.None, 40, 45, 35, 40, 90),
            new Species("Persian", Type.Normal, Type.None, 65, 70, 60, 65, 115),
            new Species("Psyduck", Type.Water, Type.None, 50, 52, 48, 50, 55),
            new Species("Golduck", Type.Water, Type.None, 80, 82, 78, 80, 85),
            new Species("Mankey", Type.Fighting, Type.None, 40, 80, 35, 35, 70),
            new Species("Primeape", Type.Fighting, Type.None, 65, 105, 60, 60, 95),
            new Species("Growlithe", Type.Fire, Type.None, 55, 70, 45, 50, 60),
            new Species("Arcanine", Type.Fire, Type.None, 90, 110, 80, 80, 95),
            new Species("Poliwag", Type.Water, Type.None, 40, 50, 40, 40, 90),
            new Species("Poliwhirl", Type.Water, Type.None, 65, 65, 65, 50, 90),
            new Species("Poliwrath", Type.Water, Type.Fighting, 90, 85, 95, 70, 70),
            new Species("Abra", Type.Psychic, Type.None, 25, 20, 15, 105, 90),
            new Species("Kadabra", Type.Psychic, Type.None, 40, 35, 30, 120, 105),
            new Species("Alakazam", Type.Psychic, Type.None, 55, 50, 45, 135, 120),
            new Species("Machop", Type.Fighting, Type.None, 70, 80, 50, 35, 35),
            new Species("Machoke", Type.Fighting, Type.None, 80, 100, 70, 50, 45),
            new Species("Machamp", Type.Fighting, Type.None, 90, 130, 80, 65, 55),
            new Species("Bellsprout", Type.Grass, Type.Poison, 50, 75, 35, 70, 40),
            new Species("Weepinbell", Type.Grass, Type.Poison, 65, 90, 50, 85, 55),
            new Species("Victreebel", Type.Grass, Type.Poison, 80, 105, 65, 100, 70),
            new Species("Tentacool", Type.Water, Type.Poison, 40, 40, 35, 100, 70),
            new Species("Tentacruel", Type.Water, Type.Poison, 80, 70, 65, 120, 100),
            new Species("Geodude", Type.Rock, Type.Ground, 40, 80, 100, 30, 20),
            new Species("Graveler", Type.Rock, Type.Ground, 55, 95, 115, 45, 35),
            new Species("Golem", Type.Rock, Type.Ground, 80, 110, 130, 55, 45),
            new Species("Ponyta", Type.Fire, Type.None, 50, 85, 55, 65, 90),
            new Species("Rapidash", Type.Fire, Type.None, 65, 100, 70, 80, 105),
            new Species("Slowpoke", Type.Water, Type.Psychic, 90, 65, 65, 40, 15),
            new Species("Slowbro", Type.Water, Type.Psychic, 95, 75, 110, 80, 30),
            new Species("Magnemite", Type.Electric, Type.None, 25, 35, 70, 95, 45),
            new Species("Magneton", Type.Electric, Type.None, 50, 60, 95, 120, 70),
            new Species("Farfetch'd", Type.Normal, Type.Flying, 52, 65, 55, 58, 60),
            new Species("Doduo", Type.Normal, Type.Flying, 35, 85, 45, 35, 75),
            new Species("Dodrio", Type.Normal, Type.Flying, 60, 110, 70, 60, 100),
            new Species("Seel", Type.Water, Type.None, 65, 45, 55, 70, 45),
            new Species("Dewgong", Type.Water, Type.Ice, 90, 70, 80, 95, 70),
            new Species("Grimer", Type.Poison, Type.None, 80, 80, 50, 40, 25),
            new Species("Muk", Type.Poison, Type.None, 105, 105, 75, 65, 50),
            new Species("Shellder", Type.Water, Type.None, 30, 65, 100, 45, 40),
            new Species("Cloyster", Type.Water, Type.Ice, 50, 95, 180, 85, 70),
            new Species("Gastly", Type.Ghost, Type.Poison, 30, 35, 30, 100, 80),
            new Species("Haunter", Type.Ghost, Type.Poison, 45, 50, 45, 115, 95),
            new Species("Gengar", Type.Ghost, Type.Poison, 60, 65, 60, 130, 110),
            new Species("Onix", Type.Rock, Type.Ground, 35, 45, 160, 30, 70),
            new Species("Drowzee", Type.Psychic, Type.None, 60, 48, 45, 90, 42),
            new Species("Hypno", Type.Psychic, Type.None, 85, 73, 70, 115, 67),
            new Species("Krabby", Type.Water, Type.None, 30, 105, 90, 25, 50),
            new Species("Kingler", Type.Water, Type.None, 55, 130, 115, 50, 75),
            new Species("Voltorb", Type.Electric, Type.None, 40, 30, 50, 55, 100),
            new Species("Electrode", Type.Electric, Type.None, 60, 50, 70, 80, 140),
            new Species("Exeggcute", Type.Grass, Type.Psychic, 60, 40, 80, 60, 40),
            new Species("Exeggutor", Type.Grass, Type.Psychic, 95, 95, 85, 125, 55),
            new Species("Cubone", Type.Ground, Type.None, 50, 50, 95, 40, 35),
            new Species("Marowak", Type.Ground, Type.None, 60, 80, 110, 50, 45),
            new Species("Hitmonlee", Type.Fighting, Type.None, 50, 120, 53, 35, 87),
            new Species("Hitmonchan", Type.Fighting, Type.None, 50, 105, 79, 35, 76),
            new Species("Lickitung", Type.Normal, Type.None, 90, 55, 75, 60, 30),
            new Species("Koffing", Type.Poison, Type.None, 40, 65, 95, 60, 35),
            new Species("Weezing", Type.Poison, Type.None, 65, 90, 120, 85, 60),
            new Species("Rhyhorn", Type.Ground, Type.Rock, 80, 85, 95, 30, 25),
            new Species("Rhydon", Type.Ground, Type.Rock, 105, 130, 120, 45, 40),
            new Species("Chansey", Type.Normal, Type.None, 250, 5, 5, 105, 50),
            new Species("Tangela", Type.Grass, Type.None, 65, 55, 115, 100, 60),
            new Species("Kangaskhan", Type.Normal, Type.None, 105, 95, 80, 40, 90),
            new Species("Horsea", Type.Water, Type.None, 30, 40, 70, 70, 60),
            new Species("Seadra", Type.Water, Type.None, 55, 65, 95, 95, 85),
            new Species("Goldeen", Type.Water, Type.None, 45, 67, 60, 50, 63),
            new Species("Seaking", Type.Water, Type.None, 80, 92, 65, 80, 68),
            new Species("Staryu", Type.Water, Type.None, 30, 45, 55, 70, 85),
            new Species("Starmie", Type.Water, Type.Psychic, 60, 75, 85, 100, 115),
            new Species("Mr. Mime", Type.Psychic, Type.None, 40, 45, 65, 100, 90),
            new Species("Scyther", Type.Bug, Type.Flying, 70, 110, 80, 55, 105),
            new Species("Jynx", Type.Ice, Type.Psychic, 65, 50, 35, 95, 95),
            new Species("Electabuzz", Type.Electric, Type.None, 65, 83, 57, 85, 105),
            new Species("Magmar", Type.Fire, Type.None, 65, 95, 57, 85, 93),
            new Species("Pinsir", Type.Bug, Type.None, 65, 125, 100, 55, 85),
            new Species("Tauros", Type.Normal, Type.None, 75, 100, 95, 70, 110),
            new Species("Magikarp", Type.Water, Type.None, 20, 10, 55, 20, 80),
            new Species("Gyarados", Type.Water, Type.Flying, 95, 125, 79, 100, 81),
            new Species("Lapras", Type.Water, Type.Ice, 130, 85, 80, 95, 60),
            new Species("Ditto", Type.Normal, Type.None, 48, 48, 48, 48, 48),
            new Species("Eevee", Type.Normal, Type.None, 55, 55, 50, 65, 55),
            new Species("Vaporeon", Type.Water, Type.None, 130, 65, 60, 110, 65),
            new Species("Jolteon", Type.Electric, Type.None, 65, 65, 60, 110, 130),
            new Species("Flareon", Type.Fire, Type.None, 65, 130, 60, 110, 65),
            new Species("Porygon", Type.Normal, Type.None, 65, 60, 70, 75, 40),
            new Species("Omanyte", Type.Rock, Type.Water, 35, 40, 100, 90, 35),
            new Species("Omastar", Type.Rock, Type.Water, 70, 60, 125, 115, 55),
            new Species("Kabuto", Type.Rock, Type.Water, 30, 80, 90, 45, 55),
            new Species("Kabutops", Type.Rock, Type.Water, 60, 115, 105, 70, 80),
            new Species("Aerodactyl", Type.Rock, Type.Flying, 80, 105, 65, 60, 130),
            new Species("Snorlax", Type.Normal, Type.None, 160, 110, 65, 65, 30),
            new Species("Articuno", Type.Ice, Type.Flying, 90, 85, 100, 125, 85),
            new Species("Zapdos", Type.Electric, Type.Flying, 90, 90, 85, 125, 100),
            new Species("Moltres", Type.Fire, Type.Flying, 90, 100, 90, 125, 90),
            new Species("Dratini", Type.Dragon, Type.None, 41, 64, 45, 50, 50),
            new Species("Dragonair", Type.Dragon, Type.None, 61, 84, 65, 70, 70),
            new Species("Dragonite", Type.Dragon, Type.Flying, 91, 134, 95, 100, 80),
            new Species("Mewtwo", Type.Psychic, Type.None, 106, 110, 90, 154, 130),
            new Species("Mew", Type.Psychic, Type.None, 100, 100, 100, 100, 100)
        };

        public Species GetPokedexIndex(byte index) => index > 0 && index <= pokedex.Length ? pokedex[index - 1] : null;
    }
}
