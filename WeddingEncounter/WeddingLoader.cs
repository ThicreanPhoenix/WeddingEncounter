using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Phoenix.WeddingEncounter;

public class LoadMod
{
    [DawnsburyDaysModMainMethod]

    public static void Load()
    {
        LoadWeddingCreatures.LoadCreatures();
        LoadWeddingItems.LoadItems();
        ModManager.RegisterEncounter<WeddingEncounter>("WeddingEncounter.tmx");
    }
}