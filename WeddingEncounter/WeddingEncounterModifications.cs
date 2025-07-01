using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Encounters.Evil_from_the_Stars;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Treasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawnsbury.Mods.Phoenix.WeddingEncounter;

public class WeddingEncounter : Encounter
{
    public WeddingEncounter(string filename) : base("WeddingEncounter.tmx", filename, new List<Item>(), 0)
    {
    }

    public override void ModifyCreatureSpawningIntoTheEncounter(Creature creature)
    {
        if (creature.OwningFaction == creature.Battle.GaiaFriends)
        {
            creature.AddQEffect(new QEffect()
            {
                WhenMonsterDies = async (qf) =>
                {
                    await qf.Owner.Battle.EndTheGame(false, qf.Owner.Name + " has died.");
                }
            });
        }
    }
}
