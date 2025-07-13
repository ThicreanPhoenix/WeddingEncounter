using Dawnsbury.Audio;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawnsbury.Mods.Phoenix.WeddingEncounter;

public class LoadWeddingCreatures
{
    public static CreatureId HungryDemonId = ModManager.RegisterEnumMember<CreatureId>("HungryDemonId");
    public static CreatureId WeddingCakeId = ModManager.RegisterEnumMember<CreatureId>("WedddingCakeId");
    public static CreatureId UnbreakableFriendshipAcolyteId = ModManager.RegisterEnumMember<CreatureId>("UnbreakableFriendshipAcolyteId");
    public static CreatureId RainbowElementalId = ModManager.RegisterEnumMember<CreatureId>("RainbowElementalId");
    public static CreatureId BrideId = ModManager.RegisterEnumMember<CreatureId>("BrideId");
    public static QEffectId CakeEatActor = ModManager.RegisterEnumMember<QEffectId>("CakeEatActor");
    public static QEffectId CakeEatTarget = ModManager.RegisterEnumMember<QEffectId>("CakeEatTarget");
    public static Creature CreateHungryDemon()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/HungryDemon.png"),
            "Hungry Demon",
            new Trait[] { Trait.Chaotic, Trait.Demon, Trait.Evil, Trait.Fiend },
            2, 5, 4,
            new Defenses(17, 11, 8, 5),
            36,
            new Abilities(3, 1, 4, -1, 0, 1),
            new Skills(athletics: 7))
                .WithCreatureId(HungryDemonId)
                .WithTactics(Tactic.Standard)
                .WithCharacteristics(speaksCommon: false, hasASkeleton: true)
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .WithUnarmedStrike(new Item(IllustrationName.Jaws, "jaws", new Trait[] { Trait.Weapon, Trait.Unarmed })
                    .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Piercing)))
                .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 3))
                .AddQEffect(QEffect.MonsterGrab())
                .AddQEffect(new QEffect("Delusional Nourishment", "When a creature adjacent to the hungry demon drinks a potion or elixir, the demon can use its reaction to grant itself the benefits of the item consumed.")
                {
                    StateCheck = async (qf) =>
                    {
                        foreach (Creature c in qf.Owner.Battle.AllCreatures)
                        {
                            if (c.IsAdjacentTo(qf.Owner))
                            {
                                c.AddQEffect(new QEffect()
                                {
                                    AfterYouTakeAction = async delegate (QEffect qf2, CombatAction action)
                                    {
                                        if (action.ActionId == ActionId.Drink && action.Item != null)
                                        {
                                            if ((!qf.Owner.HasEffect(QEffectId.Sickened)) && await qf.Owner.AskToUseReaction("A creature is drinking a " + action.Item.Name + "! Use Delusional Nourishment to gain benefits?"))
                                            {
                                                action.Item!.WhenYouDrink.Invoke(CombatAction.CreateSimple(qf.Owner, "Delusional Nourishment"), qf.Owner);
                                            }
                                        }
                                    },
                                    ExpiresAt = ExpirationCondition.Ephemeral
                                });
                            }
                        }
                    }
                })
                .AddQEffect(new QEffect()
                {
                    ProvideMainAction = delegate (QEffect qf)
                    {
                        return new ActionPossibility(new CombatAction(qf.Owner, IllustrationName.StinkingCloud, "Burp", new Trait[] { Trait.Olfactory }, "The demon burps grossly. Creatures in a 15-foot cone must succeed on a DC 18 Fortitude save or secome sickened 1 (sickened 2 on a critical failure). The demon can't Burp again for 1d4 rounds.", Target.FifteenFootCone())
                            .WithActionCost(2)
                            .WithSavingThrow(new SavingThrow(Defense.Fortitude, 18))
                            .WithProjectileCone(new VfxStyle(4, ProjectileKind.Cone, IllustrationName.StinkingCloud))
                            .WithSoundEffect(SfxName.AcidSplash)
                            .WithGoodness((tg, self, foe) => self.AI.Sicken(foe))
                            .WithEffectOnSelf(async (Creature self) =>
                            {
                                self.AddQEffect(QEffect.Recharging("Burp"));
                            })
                            .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                            {
                                switch (result)
                                {
                                    case CheckResult.Failure:
                                        target.AddQEffect(QEffect.Sickened(1, 18));
                                        break;
                                    case CheckResult.CriticalFailure:
                                        target.AddQEffect(QEffect.Sickened(2, 18));
                                        break;
                                }
                            }));
                    }
                });
    }
    public static Creature CreateWeddingCake()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/WeddingCake.png"), "Wedding Cake", new List<Trait>
            {
                Trait.Indestructible,
                Trait.Object
            }, 2, 0, 0, new Defenses(0, 0, 0, 0), 1, new Abilities(0, 0, 0, 0, 0, 0), new Skills()).WithHardness(1000).WithEntersInitiativeOrder(entersInitiativeOrder: false).WithTactics(Tactic.DoNothing)
            .WithCreatureId(WeddingCakeId)
            .AddQEffect(new QEffect
            {
                PreventTargetingBy = (CombatAction ca) => (!ca.HasTrait(Trait.Interact)) ? "Interact-only" : null
            })
            .AddQEffect(QEffect.OutOfCombat(null, null, true))
            .WithSpawnAsGaia()
            .AddQEffect(new QEffect().AddAllowActionOnSelf(CakeEatActor, CakeEatTarget, (creature) =>
            {
                return new ActionPossibility(new CombatAction(creature, IllustrationName.Heal, "Eat a Slice", new Trait[]
                {
                        Trait.BypassesOutOfCombat,
                        Trait.Manipulate,
                        Trait.Basic
                }, "Have a slice of cake, restoring 3d8+10 Hit Points. Can only be used once in the encounter.", Target.Touch())
                    .WithActionCost(1)
                    .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                    {
                        target.RemoveAllQEffects((QEffect qf) => qf.Id == CakeEatTarget);
                        await caster.HealAsync(DiceFormula.FromText("3d8+10", "Wedding Cake"), spell);
                    })).WithPossibilityGroup("Interactions");
            }, (self, target) =>
            {
                Item item = Items.CreateNew(ItemName.ModerateHealingPotion);
                string text3 = item.CannotDrinkBecause?.Invoke(self);
                return (text3 == null) ? Usability.Usable : self.HasEffect(QEffectId.Sickened) ? Usability.NotUsable("You're sickened.") : Usability.NotUsable(text3);
            }));
    }

    public static Creature CreateUnbreakableFriendshipAcolyte()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/UnbreakableFriendshipAcolyte.png"),
            "Acolyte of the Unbreakable Friendship",
            new Trait[] { Trait.Cleric, Trait.Good, Trait.Human, Trait.Humanoid },
            1, 7, 5,
            new Defenses(15, 2, 5, 9),
            16, new Abilities(1, 2, -1, 2, 4, 1),
            new Skills(crafting: 5, diplomacy: 4, society: 5, occultism: 5, religion: 7))
                {
                    SpawnAsFriends = true
                }
                .WithCreatureId(UnbreakableFriendshipAcolyteId)
                .WithBasicCharacteristics()
                .WithTactics(Tactic.Standard)
                .WithProficiency(Trait.Weapon, Proficiency.Trained)
                .AddHeldItem(Items.CreateNew(ItemName.Club))
                .AddHeldItem(Items.CreateNew(ItemName.SteelShield))
                .WithSpellProficiencyBasedOnSpellAttack(9, Ability.Wisdom)
                .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Cleric, Ability.Wisdom, Trait.Divine)
                .WithSpells(new SpellId[]
                {
                    SpellId.ChillTouch,
                    SpellId.Shield,
                    SpellId.Friendfetch,
                    SpellId.Heal,
                    SpellId.Heal,
                    SpellId.Harm
                })
                .Done();
    }

    public static Creature CreateRainbowElemental()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/RainbowElemental.png"),
            "Rainbow Elemental",
            new Trait[] { Trait.Elemental, Trait.Light, Trait.NoPhysicalUnarmedAttack, Trait.Small, Trait.Water },
            1, 3, 5,
            new Defenses(16, 4, 8, 7),
            16,
            new Abilities(1, 3, 0, 0, 0, 3),
            new Skills(nature: 5, thievery: 8))
                .WithCreatureId(RainbowElementalId)
                .WithCharacteristics(speaksCommon: false, hasASkeleton: false)
                .WithTactics(Tactic.Standard)
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Bleed))
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Poison))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed))
                .AddQEffect(QEffect.TraitImmunity(Trait.Sleep))
                .AddQEffect(QEffect.Flying())
                .AddQEffect(new QEffect()
                {
                    ProvideMainAction = delegate (QEffect qf)
                    {
                        return new ActionPossibility(qf.Owner.CreateStrike(new Item(IllustrationName.ChromaticRay, "light ray", new Trait[] { Trait.Agile, Trait.Finesse, Trait.Light, Trait.Magical, Trait.Ranged })
                                .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Force)
                                    .WithMaximumRange(6)))
                                .WithSoundEffect(SfxName.FireRay)
                                .WithProjectileCone(new VfxStyle(20, ProjectileKind.Ray, IllustrationName.Light)));
                    }
                })
                .AddQEffect(new QEffect()
                {
                    ProvideMainAction = delegate (QEffect qf)
                    {
                        return new ActionPossibility(new CombatAction(qf.Owner, IllustrationName.ColorSpray, "Unifying Light", new Trait[] { Trait.Evocation, Trait.Light, Trait.Visual }, "The elemental glows brightly and encouragingly, giving allies in a 30-foot emanation 2 temporary Hit Points. The elemental can't use Unifying Light again for 1d4 rounds.",
                                Target.AlliesOnlyEmanation(3))
                            .WithActionCost(2)
                            .WithSoundEffect(SfxName.MinorHealing)
                            .WithProjectileCone(new VfxStyle(20, ProjectileKind.ColorSpray, IllustrationName.ColorSpray))
                            .WithGoodness((tg, self, foe) => foe.FriendOfAndNotSelf(self) ? 0 : 2)
                            .WithEffectOnEachTarget(async (spell, caster, target, result) =>
                            {
                                target.GainTemporaryHP(2);
                            })
                            .WithEffectOnSelf(async (self) =>
                            {
                                self.AddQEffect(QEffect.Recharging("Unifying Light"));
                            }));
                    }
                })
                .AddMonsterInnateSpellcasting(5, Trait.Arcane, new SpellId[] { SpellId.ColorSpray });
    }

    public static Creature CreateBride()
    {
        return new Creature(new ModdedIllustration("PhoenixAssets/Bride.png"),
            "Bride",
            new Trait[] { Trait.Good, Trait.Human, Trait.Humanoid },
            0, 3, 5,
            new Defenses(13, 3, 5, 6),
            14,
            new Abilities(0, 1, 1, 0, 2, 2),
            new Skills(acrobatics: 5, performance: 4, thievery: 5))
                .WithCreatureId(BrideId)
                .With((Creature self) =>
                {
                    self.SpawnAsFriends = true;
                    self.Characteristics.DeathSoundEffect = SfxName.FemaleDeath;
                })
                .WithBasicCharacteristics()
                .WithTactics(Tactic.PackAttack)
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .AddHeldItem(new Item(IllustrationName.FlourishingFlora, "Bouquet", new Trait[] { Trait.Plant, Trait.Club, Trait.Agile, Trait.Finesse })
                    .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning)).WithMonsterWeaponSpecialization(2))
                .AddQEffect(new QEffect("Matrimonial Unity", "If the bride is next to another bride, both recieve a +1 circumstance bonus to attack and damage rolls.")
                {
                    BonusToDamage = delegate (QEffect qf, CombatAction action, Creature target)
                    {
                        if (qf.Owner.Battle.AllCreatures.Count((Creature c) => c.IsAdjacentTo(qf.Owner) && c.FriendOfAndNotSelf(qf.Owner) && (c.CreatureId == BrideId)) >= 1)
                        {
                            return new Bonus(1, BonusType.Circumstance, "Matrimonial Unity");
                        }
                        else return null;
                    },
                    BonusToAttackRolls = delegate (QEffect qf, CombatAction action, Creature defender)
                    {
                        if (action.HasTrait(Trait.Attack) && (qf.Owner.Battle.AllCreatures.Count((Creature c) => c.IsAdjacentTo(qf.Owner) && c.FriendOfAndNotSelf(qf.Owner) && (c.CreatureId == BrideId)) >= 1))
                        {
                            return new Bonus(1, BonusType.Circumstance, "Matrimonial Unity");
                        }
                        else return null;
                    }
                });
    }
    public static void LoadCreatures()
    {
        ModManager.RegisterNewCreature("HungryDemon", (encounter) =>
        {
            return CreateHungryDemon();
        });
        ModManager.RegisterNewCreature("WeddingCake", (encounter) =>
        {
            return CreateWeddingCake();
        });
        ModManager.RegisterNewCreature("UnbreakableFriendshipAcolyte", (encounter) =>
        {
            return CreateUnbreakableFriendshipAcolyte();
        });
        ModManager.RegisterNewCreature("RainbowElemental", (encounter) =>
        {
            return CreateRainbowElemental();
        });
        ModManager.RegisterNewCreature("Bride", (encounter) =>
        {
            return CreateBride();
        });
    }
}