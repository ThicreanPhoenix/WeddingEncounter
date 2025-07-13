using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawnsbury.Mods.Phoenix.WeddingEncounter;

public class LoadWeddingItems
{
    public static void LoadItems()
    {
        ModManager.RegisterNewItemIntoTheShop("BellMace", (itemName) =>
        {
            return new Item(itemName, new ModdedIllustration("PhoenixAssets/BellMace.png"), "bell mace", 3, 60, new Trait[] { Trait.SpecificMagicWeapon, Trait.Simple, Trait.Evocation, Trait.Club, Trait.Shove })
            {
                ItemName = itemName
            }
                .WithDescription("The head of this mace is shaped into the form of a bell, complete with a functioning ringer. You deal 1 extra sonic damage when you hit with this weapon.")
                .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning).WithAdditionalDamage("1", DamageKind.Sonic))
                .WithModificationPlusOne()
                .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons);
        });
    }
}
