using System;
using System.Linq;

namespace KantoSim
{
    public abstract class Move
    {
        public string Name { get; }
        public Type Type { get; }
        public bool Damages { get; }
        public double Accuracy { get; }
        public byte MaxPp { get; }
        public byte Pp { get; }
        public sbyte Priority { get; }
        public bool Enabled { get; set; }

        protected Move(string name, Type type, bool damages, double accuracy, byte pp, sbyte priority)
        {
            Name = name;
            Type = type;
            Damages = damages;
            Accuracy = accuracy;
            MaxPp = pp;
            Pp = MaxPp;
            Priority = priority;
            Enabled = true;
        }

        public bool CanUse()
        {
            return Pp > 0 && Enabled;
        }

        public virtual double HitChance(sbyte acc, sbyte eva)
        {
            if (Accuracy == 0.0)
                return 1.0;
            int threshold = Math.Min((int)(Accuracy * 255) * Battler.StageMultipliers[6 + acc] * Battler.StageMultipliers[6 - eva] / 10000, 255);
            return threshold / 256.0;
        }

        public virtual MoveEffect Primary(Battler user, Battler target)
        {
            return MoveEffect.None;
        }

        public virtual MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.None;
        }

        public readonly Move[] movedex = {};
    }

    public abstract class DamagingMove : Move
    {
        public DamagingMove(string name, Type type, double accuracy, byte pp, sbyte priority) : base(name, type, true, accuracy, pp, priority)
        { }

        public override MoveEffect Primary(Battler user, Battler target)
        {
            ushort a, d, ba, bd;
            if (Type.DamageCategory == Category.Physical)
            {
                a = user.Atk;
                d = target.Def;
                ba = user.Identity.Atk;
                bd = target.Identity.Def;
            }
            else
            {
                a = user.Spc;
                d = target.Spc;
                ba = user.Identity.Spc;
                bd = target.Identity.Spc;
            }
            return new MoveEffect(false, null, GetDamageArray(user.Level, a, d, ba, bd, user.Spe, user.VolatileStatuses.Pumped, user.Types, target.Types));
        }

        public abstract MoveEffectPossibility[] GetDamageArray(byte level, ushort a, ushort d, ushort ba, ushort bd, ushort bs, bool fe, Type[] userTypes, Type[] targetTypes);
    }

    public abstract class RegularDamagingMove : DamagingMove
    {
        private static readonly byte[] randomArray = Enumerable.Range(217, 39).Cast<byte>().ToArray();
        public readonly byte Power;
        protected virtual byte CritThresholdMultiplier => 1;
        protected virtual byte DefenseDivisor => 1;

        public RegularDamagingMove(string name, Type type, byte power, double accuracy, byte pp, sbyte priority) : base(name, type, accuracy, pp, priority)
        {
            Power = power;
        }

        public override MoveEffectPossibility[] GetDamageArray(byte userLevel, ushort a, ushort d, ushort ba, ushort bd, ushort bs, bool fe, Type[] userTypes, Type[] targetTypes)
        {
            foreach (Type t in targetTypes)
                if (Type.EffectivenessAgainst(t) == Type.Effectiveness.Ineffective)
                    return MoveEffectPossibility.Single(0);
            d /= DefenseDivisor;
            if (a > 255 || d > 255)
            {
                a /= 4;
                d /= 4;
            }
            int baseDamage = (2 * userLevel / 5 + 2) * Power * a / d / 50 + 2;
            int critDamage = (4 * userLevel / 5 + 2) * Power * ba / bd / 50 + 2;
            if (userTypes.Contains(Type))
            {
                baseDamage += baseDamage / 2;
                critDamage += critDamage / 2;
            }
            foreach (Type t in targetTypes)
            {
                Type.Effectiveness te = Type.EffectivenessAgainst(t);
                if (te == Type.Effectiveness.NotVeryEffective)
                {
                    baseDamage /= 2;
                    critDamage /= 2;
                }
                else if (te == Type.Effectiveness.SuperEffective)
                {
                    baseDamage *= 2;
                    critDamage *= 2;
                }
            }
            ushort[] baseRandomArray = Randomize(baseDamage);
            ushort[] critRandomArray = Randomize(critDamage);
            int threshold = bs / 2;
            if (fe)
                threshold = threshold / 2 * CritThresholdMultiplier / 2;
            else
                threshold *= CritThresholdMultiplier;
            if (threshold > 255)
                threshold = 255;
            double critChance = threshold / 256.0;
            double baseElemChance = (1 - critChance) / baseRandomArray.Length;
            double critElemChance = critChance / critRandomArray.Length;
            return baseRandomArray.Select(d => (d, baseElemChance))
                .Concat(critRandomArray.Select(d => (d, critElemChance)))
                .GroupBy(t => t.d, (d, a) => new MoveEffectPossibility(d, a.Sum(t => t.Item2)))
                .ToArray();
        }

        private ushort[] Randomize(int damage)
        {
            if (damage >= 1)
                return randomArray.Select(r => (ushort)(damage * r / 255)).ToArray();
            return new ushort[] { 0 };
        }
    }

    public abstract class AbsorbingMove : RegularDamagingMove
    {
        public AbsorbingMove(string name, Type type, byte power, double accuracy, byte pp, sbyte priority) : base(name, type, power, accuracy, pp, priority)
        { }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.Single(true, null, lastDamage / -2);
        }
    }

    public sealed class Absorb : AbsorbingMove
    {
        public Absorb() : base("Absorb", Type.Grass, 20, 1.0, 20, 0)
        { }
    }

    public interface IAffects<T>
    {
        public abstract T Affected { get; }
    }

    public interface IAffectsStat : IAffects<Battler.StatMod>
    {
        public abstract sbyte Stages { get; }
    }

    public interface ISecondaryChance
    {
        public abstract double Chance { get; }
    }

    public abstract class StatChanceDamagingMove : RegularDamagingMove, IAffectsStat, ISecondaryChance
    {
        public Battler.StatMod Affected { get; }
        public sbyte Stages { get; }
        public double Chance { get; }

        public StatChanceDamagingMove(string name, Type type, byte power, double accuracy, byte pp, sbyte priority, Battler.StatMod affected, sbyte stages, double chance) : base(name, type, power, accuracy, pp, priority)
        {
            Affected = affected;
            Stages = stages;
            Chance = chance;
        }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.SingleChance(false, Affected, Stages, Chance);
        }
    }

    public sealed class Acid : StatChanceDamagingMove
    {
        public Acid() : base("Acid", Type.Poison, 40, 1.0, 30, 0, Battler.StatMod.Def, -1, 0.33)
        { }
    }

    public abstract class StatusMove : Move
    {
        public StatusMove(string name, Type type, double accuracy, byte pp, sbyte priority) : base(name, type, false, accuracy, pp, priority)
        { }
    }

    public abstract class StatStatusMove : StatusMove, IAffectsStat
    {
        public bool OnUser { get; }
        public Battler.StatMod Affected { get; }
        public sbyte Stages { get; }

        public StatStatusMove(string name, Type type, double accuracy, byte pp, sbyte priority, bool onUser, Battler.StatMod affected, sbyte stages) : base(name, type, accuracy, pp, priority)
        {
            OnUser = onUser;
            Affected = affected;
            Stages = stages;
        }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.Single(OnUser, Affected, Stages);
        }
    }

    public sealed class AcidArmor : StatStatusMove
    {
        public AcidArmor() : base("Acid Armor", Type.Poison, 0.0, 40, 0, true, Battler.StatMod.Def, 2)
        { }
    }

    public sealed class Agility : StatStatusMove
    {
        public Agility() : base("Agility", Type.Psychic, 0.0, 30, 0, true, Battler.StatMod.Spe, 2)
        { }
    }

    public sealed class Amnesia : StatStatusMove
    {
        public Amnesia() : base("Amnesia", Type.Psychic, 0.0, 20, 0, true, Battler.StatMod.Spc, 2)
        { }
    }

    public sealed class AuroraBeam : StatChanceDamagingMove
    {
        public AuroraBeam() : base("Aurora Beam", Type.Ice, 65, 1.0, 20, 0, Battler.StatMod.Atk, -1, 0.33)
        { }
    }

    public abstract class MultipleDamagingMove : RegularDamagingMove
    {
        public MultipleDamagingMove(string name, Type type, byte power, double accuracy, byte pp, sbyte priority) : base(name, type, power, accuracy, pp, priority)
        { }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return new MoveEffect(false, null, new MoveEffectPossibility[] {
                new MoveEffectPossibility(lastDamage, 0.125),
                new MoveEffectPossibility(lastDamage * 2, 0.375),
                new MoveEffectPossibility(lastDamage * 3, 0.375),
                new MoveEffectPossibility(lastDamage * 4, 0.125)
            });
        }
    }

    public sealed class Barrage : MultipleDamagingMove
    {
        public Barrage() : base("Barrage", Type.Normal, 15, 0.85, 20, 0)
        { }
    }

    public sealed class Barrier : StatStatusMove
    {
        public Barrier() : base("Barrier", Type.Psychic, 0.0, 30, 0, true, Battler.StatMod.Def, 2)
        { }
    }

    // public sealed class Bide : DamagingMove
    // { }

    // public sealed class Bind : DamagingMove
    // { }

    public interface IAffectsVolatileStatus : IAffects<Battler.VolatileStatus>
    { }

    public abstract class VolatileStatusChanceDamagingMove : RegularDamagingMove, IAffectsVolatileStatus, ISecondaryChance
    {
        public Battler.VolatileStatus Affected { get; }
        public double Chance { get; }

        public VolatileStatusChanceDamagingMove(string name, Type type, byte power, double accuracy, byte pp, sbyte priority, Battler.VolatileStatus affected, double chance) : base(name, type, power, accuracy, pp, priority)
        {
            Affected = affected;
            Chance = chance;
        }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.SingleChance(false, Affected, 1, Chance);
        }
    }

    public sealed class Bite : VolatileStatusChanceDamagingMove
    {
        public Bite() : base("Bite", Type.Normal, 60, 1.0, 25, 0, Battler.VolatileStatus.Flinching, 0.1)
        { }
    }

    public interface IAffectsNonVolatileStatus : IAffects<NonVolatileStatus>
    { }

    public abstract class NonVolatileStatusChanceDamagingMove : RegularDamagingMove, IAffectsNonVolatileStatus, ISecondaryChance
    {
        public NonVolatileStatus Affected { get; }
        public double Chance { get; }

        public NonVolatileStatusChanceDamagingMove(string name, Type type, byte power, double accuracy, byte pp, sbyte priority, NonVolatileStatus affected, double chance) : base(name, type, power, accuracy, pp, priority)
        {
            Affected = affected;
            Chance = chance;
        }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.SingleChance(false, Affected, 1, Chance);
        }
    }

    public sealed class Blizzard : NonVolatileStatusChanceDamagingMove
    {
        public Blizzard() : base("Blizzard", Type.Ice, 120, 0.9, 5, 0, NonVolatileStatus.Freeze, 0.1)
        { }
    }

    public sealed class BodySlam : NonVolatileStatusChanceDamagingMove
    {
        public BodySlam() : base("Body Slam", Type.Normal, 85, 1.0, 15, 0, NonVolatileStatus.Paralysis, 0.3)
        { }
    }

    public sealed class BoneClub : VolatileStatusChanceDamagingMove
    {
        public BoneClub() : base("Bone Club", Type.Ground, 65, 0.85, 20, 0, Battler.VolatileStatus.Flinching, 0.1)
        { }
    }

    public abstract class DoubleDamagingMove : RegularDamagingMove
    {
        public DoubleDamagingMove(string name, Type type, byte power, double accuracy, byte pp, sbyte priority) : base(name, type, power, accuracy, pp, priority)
        { }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.Single(false, null, lastDamage);
        }
    }

    public sealed class Bonemerang : DoubleDamagingMove
    {
        public Bonemerang() : base("Bonemerang", Type.Ground, 50, 0.9, 10, 0)
        { }
    }

    public sealed class Bubble : StatChanceDamagingMove
    {
        public Bubble() : base("Bubble", Type.Water, 20, 1.0, 30, 0, Battler.StatMod.Spe, -1, 0.33)
        { }
    }

    public sealed class BubbleBeam : StatChanceDamagingMove
    {
        public BubbleBeam() : base("Bubble Beam", Type.Water, 65, 1.0, 20, 0, Battler.StatMod.Spe, -1, 0.33)
        { }
    }

    // public sealed class Clamp
    // { }

    public sealed class CometPunch : MultipleDamagingMove
    {
        public CometPunch() : base("Comet Punch", Type.Normal, 18, 0.85, 15, 0)
        { }
    }

    public abstract class VolatileStatusStatusMove : StatusMove, IAffectsVolatileStatus
    {
        public bool OnUser { get; }
        public Battler.VolatileStatus Affected { get; }

        public VolatileStatusStatusMove(string name, Type type, double accuracy, byte pp, sbyte priority, bool onUser, Battler.VolatileStatus affected) : base(name, type, accuracy, pp, priority)
        {
            OnUser = onUser;
            Affected = affected;
        }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.Single(OnUser, Affected, 1);
        }
    }

    public sealed class ConfuseRay : VolatileStatusStatusMove
    {
        public ConfuseRay() : base("Confuse Ray", Type.Ghost, 1.0, 10, 0, false, Battler.VolatileStatus.Confused)
        { }
    }

    public sealed class Confusion : VolatileStatusChanceDamagingMove
    {
        public Confusion() : base("Confusion", Type.Psychic, 50, 1.0, 25, 0, Battler.VolatileStatus.Confused, 0.1)
        { }
    }

    // public sealed class Constrict : DamagingMove
    // { }

    //public sealed class Conversion : StatusMove
    // { }

    public sealed class Counter : DamagingMove
    {
        public Counter() : base("Counter", Type.Fighting, 1.0, 20, 0)
        { }

        public override MoveEffectPossibility[] GetDamageArray(byte level, ushort a, ushort d, ushort ba, ushort bd, ushort bs, bool fe, Type[] userTypes, Type[] targetTypes)
        {
            return new MoveEffectPossibility[0];
        }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.Single(false, null, lastDamage * 2);
        }
    }

    public abstract class HiCritDamagingMove : RegularDamagingMove
    {
        protected override byte CritThresholdMultiplier => 8;

        public HiCritDamagingMove(string name, Type type, byte power, double accuracy, byte pp, sbyte priority) : base(name, type, power, accuracy, pp, priority)
        { }
    }

    public sealed class Crabhammer : HiCritDamagingMove
    {
        public Crabhammer() : base("Crabhammer", Type.Water, 90, 0.85, 10, 0)
        { }
    }

    public sealed class Cut : RegularDamagingMove
    {
        public Cut() : base("Cut", Type.Normal, 50, 0.95, 30, 0)
        { }
    }

    public sealed class DefenseCurl : StatStatusMove
    {
        public DefenseCurl() : base("Defense Curl", Type.Normal, 0.0, 40, 0, true, Battler.StatMod.Def, 1)
        { }
    }

    // public sealed class Dig : RegularDamagingMove
    // { }

    public sealed class Disable : VolatileStatusStatusMove
    {
        public Disable() : base("Disable", Type.Normal, 0.55, 20, 0, false, Battler.VolatileStatus.Disabled)
        {
            // add Scale for duration of disability
        }
    }

    public sealed class DizzyPunch : RegularDamagingMove
    {
        public DizzyPunch() : base("Dizzy Punch", Type.Normal, 70, 1.0, 10, 0)
        { }
    }

    public sealed class DoubleKick : DoubleDamagingMove
    {
        public DoubleKick() : base("Double Kick", Type.Fighting, 30, 1.0, 30, 0)
        { }
    }

    public sealed class DoubleSlap : MultipleDamagingMove
    {
        public DoubleSlap() : base("Double Slap", Type.Normal, 15, 0.85, 10, 0)
        { }
    }

    public sealed class DoubleTeam : StatStatusMove
    {
        public DoubleTeam() : base("Double Team", Type.Normal, 0.0, 15, 0, true, Battler.StatMod.Eva, 1)
        { }
    }

    public abstract class RecoilDamagingMove : RegularDamagingMove
    {
        public byte RecoilReciprocal { get; }
        public RecoilDamagingMove(string name, Type type, byte power, double accuracy, byte pp, sbyte priority, byte recoilReciprocal) : base(name, type, power, accuracy, pp, priority)
        {
            RecoilReciprocal = recoilReciprocal;
        }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.Single(true, null, lastDamage / RecoilReciprocal);
        }
    }

    public sealed class DoubleEdge : RecoilDamagingMove
    {
        public DoubleEdge() : base("Double-Edge", Type.Normal, 100, 1.0, 15, 0, 4)
        { }
    }

    public sealed class DragonRage : DamagingMove
    {
        public DragonRage() : base("Dragon Rage", Type.Dragon, 1.0, 10, 0)
        { }

        public override MoveEffectPossibility[] GetDamageArray(byte level, ushort a, ushort d, ushort ba, ushort bd, ushort bs, bool fe, Type[] userTypes, Type[] targetTypes)
        {
            return MoveEffectPossibility.Single(40);
        }
    }

    public sealed class DreamEater : AbsorbingMove
    {
        public DreamEater() : base("DreamEater", Type.Psychic, 100, 1.0, 15, 0)
        { }

        // target must be asleep
    }

    public sealed class DrillPeck : RegularDamagingMove
    {
        public DrillPeck() : base("Drill Peck", Type.Flying, 80, 1.0, 20, 0)
        { }
    }

    public sealed class Earthquake : RegularDamagingMove
    {
        public Earthquake() : base("Earthquake", Type.Ground, 100, 1.0, 10, 0)
        { }
    }

    public sealed class EggBomb : RegularDamagingMove
    {
        public EggBomb() : base("Egg Bomb", Type.Normal, 100, 0.75, 10, 0)
        { }
    }

    public sealed class Ember : NonVolatileStatusChanceDamagingMove
    {
        public Ember() : base("Ember", Type.Fire, 40, 1.0, 25, 0, NonVolatileStatus.Burn, 0.1)
        { }
    }

    public abstract class KamikazeDamagingMove : RegularDamagingMove
    {
        protected override byte DefenseDivisor => 2;

        public KamikazeDamagingMove(string name, Type type, byte power, double accuracy, byte pp, sbyte priority) : base(name, type, power, accuracy, pp, priority)
        { }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.Single(true, null, userMaxHp);
        }
    }

    public sealed class Explosion : KamikazeDamagingMove
    {
        public Explosion() : base("Explosion", Type.Normal, 170, 100, 5, 0)
        { }
    }

    public sealed class FireBlast : NonVolatileStatusChanceDamagingMove
    {
        public FireBlast() : base("Fire Blast", Type.Fire, 120, 0.85, 5, 0, NonVolatileStatus.Burn, 0.3)
        { }
    }

    public sealed class FirePunch : NonVolatileStatusChanceDamagingMove
    {
        public FirePunch() : base("Fire Punch", Type.Fire, 75, 1.0, 15, 0, NonVolatileStatus.Burn, 0.1)
        { }
    }

    // public sealed class FireSpin : DamagingMove
    // { }

    public abstract class OHKOMove : DamagingMove
    {
        public OHKOMove(string name, Type type) : base(name, type, 0.3, 5, 0)
        { }

        // fails if user is slower than target

        public override MoveEffectPossibility[] GetDamageArray(byte level, ushort a, ushort d, ushort ba, ushort bd, ushort bs, bool fe, Type[] userTypes, Type[] targetTypes)
        {
            return MoveEffectPossibility.Single(ushort.MaxValue);
        }
    }

    public sealed class Fissure : OHKOMove
    {
        public Fissure() : base("Fissure", Type.Ground)
        { }
    }

    public sealed class Flamethrower : NonVolatileStatusChanceDamagingMove
    {
        public Flamethrower() : base("Flamethrower", Type.Fire, 95, 1.0, 15, 0, NonVolatileStatus.Burn, 0.1)
        { }
    }

    public sealed class Flash : StatStatusMove
    {
        public Flash() : base("Flash", Type.Normal, 0.7, 20, 0, false, Battler.StatMod.Acc, -1)
        { }
    }

    // public sealed class Fly : RegularDamagingMove
    // { }

    public sealed class FocusEnergy : VolatileStatusStatusMove
    {
        public FocusEnergy() : base("Focus Energy", Type.Normal, 0.0, 30, 0, true, Battler.VolatileStatus.Pumped)
        { }
    }

    public sealed class FuryAttack : MultipleDamagingMove
    {
        public FuryAttack() : base("Fury Attack", Type.Normal, 15, 0.85, 20, 0)
        { }
    }

    public sealed class FurySwipes : MultipleDamagingMove
    {
        public FurySwipes() : base("Fury Swipes", Type.Normal, 18, 0.8, 15, 0)
        { }
    }

    public abstract class NonVolatileStatusStatusMove : StatusMove, IAffectsNonVolatileStatus
    {
        public bool OnUser { get; }
        public NonVolatileStatus Affected { get; }

        public NonVolatileStatusStatusMove(string name, Type type, double accuracy, byte pp, sbyte priority, bool onUser, NonVolatileStatus affected) : base(name, type, accuracy, pp, priority)
        {
            OnUser = onUser;
            Affected = affected;
        }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            if (Affected.Duration == 0)
                return MoveEffect.Single(OnUser, Affected, 1);
            return new MoveEffect(OnUser, Affected, Enumerable.Range(1, Affected.Duration).Select(r => new MoveEffectPossibility(r, 1.0 / Affected.Duration)).ToArray());
        }
    }

    public sealed class Glare : NonVolatileStatusStatusMove
    {
        public Glare() : base("Glare", Type.Normal, 0.75, 30, 0, false, NonVolatileStatus.Paralysis)
        { }
    }

    public sealed class Growl : StatStatusMove
    {
        public Growl() : base("Growl", Type.Normal, 1.0, 40, 0, false, Battler.StatMod.Atk, -1)
        { }
    }

    public sealed class Growth : StatStatusMove
    {
        public Growth() : base("Growth", Type.Normal, 0.0, 40, 0, true, Battler.StatMod.Spc, 1)
        { }
    }

    public sealed class Guillotine : OHKOMove
    {
        public Guillotine() : base("Guillotine", Type.Normal)
        { }
    }

    public sealed class Gust : RegularDamagingMove
    {
        public Gust() : base("Gust", Type.Normal, 40, 1.0, 35, 0)
        { }
    }

    public sealed class Harden : StatStatusMove
    {
        public Harden() : base("Harden", Type.Normal, 0.0, 30, 0, true, Battler.StatMod.Def, 1)
        { }
    }

    // public sealed class Haze : StatusMove
    // { }

    public sealed class Headbutt : VolatileStatusChanceDamagingMove
    {
        public Headbutt() : base("Headbutt", Type.Normal, 70, 1.0, 15, 0, Battler.VolatileStatus.Flinching, 0.3)
        { }
    }

    public abstract class JumpDamagingMove : RegularDamagingMove
    {
        public JumpDamagingMove(string name, byte power, double accuracy, byte pp) : base(name, Type.Fighting, power, accuracy, pp, 0)
        { }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            // triggers on a miss
            return MoveEffect.Single(true, null, 1);
        }
    }

    public sealed class HighJumpKick : JumpDamagingMove
    {
        public HighJumpKick() : base("High Jump Kick", 85, 0.9, 20)
        { }
    }

    public sealed class HornAttack : RegularDamagingMove
    {
        public HornAttack() : base("Horn Attack", Type.Normal, 65, 1.0, 25, 0)
        { }
    }

    public sealed class HornDrill : OHKOMove
    {
        public HornDrill() : base("Horn Attack", Type.Normal)
        { }
    }

    public sealed class HydroPump : RegularDamagingMove
    {
        public HydroPump() : base("Hydro Pump", Type.Water, 120, 0.8, 5, 0)
        { }
    }

    public abstract class RechargeDamagingMove : RegularDamagingMove, IAffectsVolatileStatus
    {
        public Battler.VolatileStatus Affected => Battler.VolatileStatus.Recharging;

        public RechargeDamagingMove(string name, Type type, byte power, double accuracy, byte pp) : base(name, type, power, accuracy, pp, 0)
        { }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.Single(true, Affected, 1);
        }
    }

    public sealed class HyperBeam : RechargeDamagingMove
    {
        public HyperBeam() : base("Hyper Beam", Type.Normal, 150, 0.9, 5)
        { }
    }

    public sealed class HyperFang : VolatileStatusChanceDamagingMove
    {
        public HyperFang() : base("Hyper Fang", Type.Normal, 80, 0.9, 15, 0, Battler.VolatileStatus.Flinching, 0.1)
        { }
    }

    public sealed class Hypnosis : NonVolatileStatusStatusMove
    {
        public Hypnosis() : base("Hypnosis", Type.Psychic, 0.6, 20, 0, false, NonVolatileStatus.Sleep)
        { }
    }

    public sealed class IceBeam : NonVolatileStatusChanceDamagingMove
    {
        public IceBeam() : base("Ice Beam", Type.Ice, 95, 1.0, 10, 0, NonVolatileStatus.Freeze, 0.1)
        { }
    }

    public sealed class IcePunch : NonVolatileStatusChanceDamagingMove
    {
        public IcePunch() : base("Ice Punch", Type.Ice, 75, 1.0, 15, 0, NonVolatileStatus.Freeze, 0.1)
        { }
    }

    public sealed class JumpKick : JumpDamagingMove
    {
        public JumpKick() : base("Jump Kick", 70, 0.95, 25)
        { }
    }

    public sealed class KarateChop : HiCritDamagingMove
    {
        public KarateChop() : base("Karate Chop", Type.Normal, 50, 1.0, 25, 0)
        { }
    }

    public sealed class Kinesis : StatStatusMove
    {
        public Kinesis() : base("Kinesis", Type.Psychic, 0.8, 15, 0, false, Battler.StatMod.Acc, -1)
        { }
    }

    public sealed class LeechLife : AbsorbingMove
    {
        public LeechLife() : base("Leech Life", Type.Bug, 20, 1.0, 15, 0)
        { }
    }

    public sealed class LeechSeed : VolatileStatusStatusMove
    {
        public LeechSeed() : base("Leech Seed", Type.Grass, 0.9, 10, 0, false, Battler.VolatileStatus.Seeded)
        { }
    }

    public sealed class Leer : StatStatusMove
    {
        public Leer() : base("Leer", Type.Normal, 1.0, 30, 0, false, Battler.StatMod.Def, -1)
        { }
    }

    public sealed class Lick : NonVolatileStatusChanceDamagingMove
    {
        public Lick() : base("Lick", Type.Ghost, 20, 1.0, 30, 0, NonVolatileStatus.Paralysis, 0.3)
        { }
    }

    // public sealed class LightScreen : StatusMove
    // { }

    public sealed class LovelyKiss : NonVolatileStatusStatusMove
    {
        public LovelyKiss() : base("Lovely Kiss", Type.Normal, 0.75, 10, 0, false, NonVolatileStatus.Sleep)
        { }
    }

    public sealed class LowKick : VolatileStatusChanceDamagingMove
    {
        public LowKick() : base("Low Kick", Type.Fighting, 50, 0.9, 20, 0, Battler.VolatileStatus.Flinching, 0.3)
        { }
    }

    public sealed class Meditate : StatStatusMove
    {
        public Meditate() : base("Meditate", Type.Psychic, 0.0, 40, 0, true, Battler.StatMod.Atk, 1)
        { }
    }

    public sealed class MegaDrain : AbsorbingMove
    {
        public MegaDrain() : base("Mega Drain", Type.Grass, 40, 1.0, 10, 0)
        { }
    }

    public sealed class MegaKick : RegularDamagingMove
    {
        public MegaKick() : base("Mega Kick", Type.Normal, 120, 0.75, 5, 0)
        { }
    }

    public sealed class MegaPunch : RegularDamagingMove
    {
        public MegaPunch() : base("Mega Punch", Type.Normal, 80, 0.85, 20, 0)
        { }
    }

    // public sealed class Metronome : StatusMove
    // { }

    // public sealed class Mimic : StatusMove
    // { }

    public sealed class Minimize : StatStatusMove
    {
        public Minimize() : base("Minimize", Type.Normal, 0.0, 20, 0, true, Battler.StatMod.Eva, 1)
        { }
    }

    // public sealed class MirrorMove : StatusMove
    // { }

    // public sealed class Mist : StatusMove
    // { }

    public sealed class NightShade : DamagingMove
    {
        public NightShade() : base("Night Shade", Type.Ghost, 1.0, 15, 0)
        { }

        public override MoveEffectPossibility[] GetDamageArray(byte level, ushort a, ushort d, ushort ba, ushort bd, ushort bs, bool fe, Type[] userTypes, Type[] targetTypes)
        {
            return MoveEffectPossibility.Single(level);
        }
    }

    public sealed class PayDay : RegularDamagingMove
    {
        public PayDay() : base("Pay Day", Type.Normal, 40, 1.0, 20, 0)
        { }

        // also scatters coins
    }

    public sealed class Peck : RegularDamagingMove
    {
        public Peck() : base("Peck", Type.Flying, 35, 1.0, 35, 0)
        { }
    }

    // public sealed class PetalDance : RegularDamagingMove
    // { }

    public sealed class PinMissile : MultipleDamagingMove
    {
        public PinMissile() : base("Pin Missile", Type.Bug, 14, 0.85, 20, 0)
        { }
    }

    public sealed class PoisonGas : NonVolatileStatusStatusMove
    {
        public PoisonGas() : base("Poison Gas", Type.Poison, 0.55, 40, 0, false, NonVolatileStatus.Poison)
        { }
    }

    public sealed class PoisonPowder : NonVolatileStatusStatusMove
    {
        public PoisonPowder() : base("Poison Powder", Type.Poison, 0.75, 35, 0, false, NonVolatileStatus.Poison)
        { }
    }

    public sealed class PoisonSting : NonVolatileStatusChanceDamagingMove
    {
        public PoisonSting() : base("Poison Sting", Type.Poison, 15, 1.0, 35, 0, NonVolatileStatus.Poison, 0.2)
        { }
    }

    public sealed class Pound : RegularDamagingMove
    {
        public Pound() : base("Pound", Type.Normal, 40, 1.0, 35, 0)
        { }
    }

    public sealed class Psybeam : VolatileStatusChanceDamagingMove
    {
        public Psybeam() : base("Psybeam", Type.Psychic, 65, 1.0, 20, 0, Battler.VolatileStatus.Confused, 0.1)
        { }
    }

    public sealed class Psychic : StatChanceDamagingMove
    {
        public Psychic() : base("Psychic", Type.Psychic, 90, 1.0, 10, 0, Battler.StatMod.Spc, 1, 0.33)
        { }
    }

    public sealed class Psywave : DamagingMove
    {
        public Psywave() : base("Psywave", Type.Psychic, 0.8, 15, 0)
        { }

        public override MoveEffectPossibility[] GetDamageArray(byte level, ushort a, ushort d, ushort ba, ushort bd, ushort bs, bool fe, Type[] userTypes, Type[] targetTypes)
        {
            if (level <= 1)
                throw new ArgumentOutOfRangeException("Pokemon of this level cannot use Psywave!", "level");
            int count = level * 3 / 2 - 1;
            return Enumerable.Range(1, count).Select(d => new MoveEffectPossibility((ushort)d, 1.0 / count)).ToArray();
        }
    }

    public sealed class QuickAttack : RegularDamagingMove
    {
        public QuickAttack() : base("Quick Attack", Type.Normal, 40, 1.0, 30, 1)
        { }
    }

    // public sealed class Rage : RegularDamagingMove
    // { }

    public sealed class RazorLeaf : HiCritDamagingMove
    {
        public RazorLeaf() : base("Razor Leaf", Type.Grass, 55, 0.95, 25, 0)
        { }
    }

    // public sealed class RazorWind : RegularDamagingMove
    // { }

    public abstract class HalfHealStatusMove : StatusMove
    {
        public HalfHealStatusMove(string name, byte pp) : base(name, Type.Normal, 0.0, pp, 0)
        { }

        public override MoveEffect Secondary(ushort lastDamage, ushort userMaxHp)
        {
            return MoveEffect.Single(true, null, userMaxHp / -2);
        }
    }

    public sealed class Recover : HalfHealStatusMove
    {
        public Recover() : base("Recover", 20)
        { }
    }

    // public sealed class Reflect : StatusMove
    // { }

    // public sealed class Rest : StatusMove
    // { }

    public sealed class Roar : StatusMove
    {
        public Roar() : base("Roar", Type.Normal, 1.0, 20, 0)
        { }
    }

    public sealed class RockSlide : RegularDamagingMove
    {
        public RockSlide() : base("Rock Slide", Type.Rock, 75, 0.9, 10, 0)
        { }
    }

    public sealed class RockThrow : RegularDamagingMove
    {
        public RockThrow() : base("Rock Throw", Type.Rock, 50, 0.65, 15, 0)
        { }
    }

    public sealed class RollingKick : VolatileStatusChanceDamagingMove
    {
        public RollingKick() : base("Rolling Kick", Type.Fighting, 60, 0.85, 15, 0, Battler.VolatileStatus.Flinching, 0.3)
        { }
    }

    public sealed class SandAttack : StatStatusMove
    {
        public SandAttack() : base("Sand Attack", Type.Normal, 1.0, 15, 0, false, Battler.StatMod.Acc, 1)
        { }
    }

    public sealed class Scratch : RegularDamagingMove
    {
        public Scratch() : base("Scratch", Type.Normal, 40, 1.0, 35, 0)
        { }
    }

    public sealed class Screech : StatStatusMove
    {
        public Screech() : base("Screech", Type.Normal, 0.85, 40, 0, false, Battler.StatMod.Def, -2)
        { }
    }

    public sealed class SeismicToss : DamagingMove
    {
        public SeismicToss() : base("Seismic Toss", Type.Fighting, 1.0, 20, 0)
        { }

        public override MoveEffectPossibility[] GetDamageArray(byte level, ushort a, ushort d, ushort ba, ushort bd, ushort bs, bool fe, Type[] userTypes, Type[] targetTypes)
        {
            return MoveEffectPossibility.Single(level);
        }
    }

    public sealed class SelfDestruct : KamikazeDamagingMove
    {
        public SelfDestruct() : base("Self-Destruct", Type.Normal, 130, 1.0, 20, 0)
        { }
    }

    public sealed class Sharpen : StatStatusMove
    {
        public Sharpen() : base("Sharpen", Type.Normal, 0.0, 30, 0, true, Battler.StatMod.Atk, 1)
        { }
    }

    public sealed class Sing : NonVolatileStatusStatusMove
    {
        public Sing() : base("Sing", Type.Normal, 0.55, 15, 0, false, NonVolatileStatus.Sleep)
        { }
    }

    // public sealed class SkullBash : RegularDamagingMove
    // { }

    // public sealed class SkyAttack : RegularDamagingMove
    // { }

    public sealed class Slam : RegularDamagingMove
    {
        public Slam() : base("Slam", Type.Normal, 80, 0.75, 20, 0)
        { }
    }

    public sealed class Slash : HiCritDamagingMove
    {
        public Slash() : base("Slash", Type.Normal, 70, 1.0, 20, 0)
        { }
    }

    public sealed class SleepPowder : NonVolatileStatusStatusMove
    {
        public SleepPowder() : base("Sleep Powder", Type.Grass, 0.75, 15, 0, false, NonVolatileStatus.Sleep)
        { }
    }

    public sealed class Sludge : NonVolatileStatusChanceDamagingMove
    {
        public Sludge() : base("Sludge", Type.Poison, 65, 1.0, 20, 0, NonVolatileStatus.Poison, 0.4)
        { }
    }

    public sealed class Smog : NonVolatileStatusChanceDamagingMove
    {
        public Smog() : base("Smog", Type.Poison, 20, 0.7, 20, 0, NonVolatileStatus.Poison, 0.4)
        { }
    }

    public sealed class Smokescreen : StatStatusMove
    {
        public Smokescreen() : base("Smokescreen", Type.Normal, 1.0, 20, 0, false, Battler.StatMod.Acc, -1)
        { }
    }

    public sealed class SoftBoiled : HalfHealStatusMove
    {
        public SoftBoiled() : base("Soft-Boiled", 10)
        { }
    }

    // public sealed class SolarBeam : RegularDamagingMove
    // { }

    public sealed class SonicBoom : DamagingMove
    {
        public SonicBoom() : base("Sonic Boom", Type.Normal, 0.9, 20, 0)
        { }

        public override MoveEffectPossibility[] GetDamageArray(byte level, ushort a, ushort d, ushort ba, ushort bd, ushort bs, bool fe, Type[] userTypes, Type[] targetTypes)
        {
            return MoveEffectPossibility.Single(20);
        }
    }

    public sealed class SpikeCannon : MultipleDamagingMove
    {
        public SpikeCannon() : base("Spike Cannon", Type.Normal, 20, 1.0, 15, 0)
        { }
    }

    public sealed class Splash : StatusMove
    {
        public Splash() : base("Splash", Type.Normal, 0.0, 40, 0)
        { }
    }

    public sealed class Spore : NonVolatileStatusStatusMove
    {
        public Spore() : base("Spore", Type.Grass, 1.0, 15, 0, false, NonVolatileStatus.Sleep)
        { }
    }

    public sealed class Stomp : VolatileStatusChanceDamagingMove
    { 
        public Stomp() : base("Stomp", Type.Normal, 65, 1.0, 20, 0, Battler.VolatileStatus.Flinching, 0.3)
        { }
    }

    public sealed class Strength : RegularDamagingMove
    {
        public Strength() : base("Strength", Type.Normal, 80, 1.0, 15, 0)
        { }
    }

    public sealed class StringShot : StatStatusMove
    {
        public StringShot() : base("String Shot", Type.Bug, 0.95, 40, 0, false, Battler.StatMod.Spe, -1)
        { }
    }

    public sealed class Struggle : RecoilDamagingMove
    {
        public Struggle() : base("Struggle", Type.Normal, 50, 1.0, 10, 0, 2)
        { }
    }

    public sealed class StunSpore : NonVolatileStatusStatusMove
    {
        public StunSpore() : base("Stun Spore", Type.Grass, 0.75, 30, 0, false, NonVolatileStatus.Paralysis)
        { }
    }

    public sealed class Submission : RecoilDamagingMove
    {
        public Submission() : base("Submission", Type.Fighting, 80, 0.8, 25, 0, 4)
        { }
    }

    // public sealed class Substitute : StatusMove
    // { }

    // public sealed class SuperFang : DamagingMove
    // {
    //     public SuperFang() : base("Super Fang", Type.Normal, 0.9, 10, 0)
    //     { }
    // }

    public sealed class Supersonic : VolatileStatusStatusMove
    {
        public Supersonic() : base("Supersonic", Type.Normal, 0.55, 20, 0, false, Battler.VolatileStatus.Confused)
        { }
    }

    public sealed class Surf : RegularDamagingMove
    {
        public Surf() : base("Surf", Type.Water, 95, 1.0, 15, 0)
        { }
    }

    public sealed class Swift : RegularDamagingMove
    {
        public Swift() : base("Swift", Type.Normal, 60, 0.0, 20, 0)
        { }
    }

    public sealed class SwordsDance : StatStatusMove
    {
        public SwordsDance() : base("Swords Dance", Type.Normal, 0.0, 30, 0, true, Battler.StatMod.Atk, 2)
        { }
    }

    public sealed class Tackle : RegularDamagingMove
    {
        public Tackle() : base("Tackle", Type.Normal, 35, 0.95, 35, 0)
        { }
    }

    public sealed class TailWhip : StatStatusMove
    {
        public TailWhip() : base("Tail Whip", Type.Normal, 1.0, 30, 0, false, Battler.StatMod.Def, -1)
        { }
    }

    public sealed class TakeDown : RecoilDamagingMove
    {
        public TakeDown() : base("Take Down", Type.Normal, 90, 0.85, 20, 0, 4)
        { }
    }

    public sealed class Teleport : StatusMove
    {
        public Teleport() : base("Teleport", Type.Psychic, 0.0, 20, 0)
        { }
    }

    // public sealed class Thrash : RegularDamagingMove
    // { }

    public sealed class Thunder : NonVolatileStatusChanceDamagingMove
    {
        public Thunder() : base("Thunder", Type.Electric, 120, 0.7, 10, 0, NonVolatileStatus.Paralysis, 0.1)
        { }
    }

    public sealed class ThunderPunch : NonVolatileStatusChanceDamagingMove
    {
        public ThunderPunch() : base("Thunder Punch", Type.Electric, 75, 1.0, 15, 0, NonVolatileStatus.Paralysis, 0.1)
        { }
    }

    public sealed class ThunderShock : NonVolatileStatusChanceDamagingMove
    {
        public ThunderShock() : base("Thunder Shock", Type.Electric, 40, 1.0, 30, 0, NonVolatileStatus.Paralysis, 0.1)
        { }
    }

    public sealed class ThunderWave : NonVolatileStatusStatusMove
    {
        public ThunderWave() : base("Thunder Wave", Type.Electric, 1.0, 20, 0, false, NonVolatileStatus.Paralysis)
        { }
    }

    public sealed class Thunderbolt : NonVolatileStatusChanceDamagingMove
    {
        public Thunderbolt() : base("Thunderbolt", Type.Electric, 95, 1.0, 15, 0, NonVolatileStatus.Paralysis, 0.1)
        { }
    }

    // public sealed class Toxic : StatusMove
    // { }

    // public sealed class Transform : VolatileStatusStatusMove
    // { }

    public sealed class TriAttack : RegularDamagingMove
    {
        public TriAttack() : base("Tri Attack", Type.Normal, 80, 1.0, 10, 0)
        { }
    }

    public sealed class Twineedle : DoubleDamagingMove
    {
        public Twineedle() : base("Twineedle", Type.Bug, 25, 1.0, 20, 0)
        { }

        // also 20% chance to poison on second hit
    }

    public sealed class ViceGrip : RegularDamagingMove
    {
        public ViceGrip() : base("Vice Grip", Type.Normal, 55, 1.0, 30, 0)
        { }
    }

    public sealed class VineWhip : RegularDamagingMove
    {
        public VineWhip() : base("Vine Whip", Type.Grass, 35, 1.0, 10, 0)
        { }
    }

    public sealed class WaterGun : RegularDamagingMove
    {
        public WaterGun() : base("Water Gun", Type.Water, 40, 1.0, 25, 0)
        { }
    }

    public sealed class Waterfall : RegularDamagingMove
    {
        public Waterfall() : base("Waterfall", Type.Water, 80, 1.0, 15, 0)
        { }
    }

    public sealed class Whirlwind : StatusMove
    {
        public Whirlwind() : base("Whirlwind", Type.Normal, 0.85, 20, 0)
        { }
    }

    public sealed class WingAttack : RegularDamagingMove
    {
        public WingAttack() : base("Wing Attack", Type.Flying, 35, 1.0, 35, 0)
        { }
    }

    public sealed class Withdraw : StatStatusMove
    {
        public Withdraw() : base("Withdraw", Type.Water, 0.0, 40, 0, true, Battler.StatMod.Def, 1)
        { }
    }

    // public sealed class Wrap : RegularDamagingMove
    // { }
}
