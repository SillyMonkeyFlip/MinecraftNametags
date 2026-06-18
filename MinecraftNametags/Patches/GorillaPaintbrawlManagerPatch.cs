using HarmonyLib;
using MinecraftNametags.Behaviours;

namespace MinecraftNametags.Patches;

[HarmonyPatch(typeof(GorillaPaintbrawlManager))]
public class GorillaPaintbrawlManagerPatch
{
    [HarmonyPatch("UpdatePlayerStatus")]
    public static void Postfix(GorillaPaintbrawlManager __instance)
    {
        Nametag.UpdateAllPaintbrawl();
    }
}
