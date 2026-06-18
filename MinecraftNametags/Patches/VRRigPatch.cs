using HarmonyLib;
using MinecraftNametags.Behaviours;

namespace MinecraftNametags.Patches;

[HarmonyPatch(typeof(VRRig))]
public class VRRigPatch
{
    [HarmonyPatch("SetCosmeticsActive")]
    [HarmonyPatch("EnablePaintbrawlCosmetics")]
    public static void Postfix(VRRig __instance)
    {
        Nametag.UpdateAllPaintbrawl();
    }
}
