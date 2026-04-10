using HarmonyLib;
using CruiserXL.Utils;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(SoundManager))]
public class SoundManagerPatches
{
    [HarmonyPatch(nameof(SoundManager.Start))]
    [HarmonyPrefix]
    static void Start_Prefix(SoundManager __instance)
    {
        // create a reference to the vanilla SFX sound mixer, so we can fix
        // our own vehicle audios with it (for TZP effects and such)
        if (References.diageticSFXGroup == null)
        {
            var sfxGroup = __instance.diageticMixer.FindMatchingGroups("Master/SFX")[0];
            References.diageticSFXGroup = sfxGroup;
        }
    }
}
