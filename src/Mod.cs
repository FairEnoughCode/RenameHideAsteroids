using HarmonyLib;
using KMod;
using UnityEngine;

namespace RenameHideAsteroids
{
    public class Mod : UserMod2
    {
        public const string LogTag = "RenameHideAsteroids";
        public static string ModPath;

        public override void OnLoad(Harmony harmony)
        {
            ModPath = path;
            base.OnLoad(harmony);
            ModSettings.Register(this);
        }
    }

    /// <summary>Applies translations/&lt;language&gt;.po to STRINGS once the game loads its language.</summary>
    [HarmonyPatch(typeof(Localization), "Initialize")]
    public static class Localization_Initialize_Patch
    {
        public static void Postfix()
        {
            ModLocalization.Init(typeof(STRINGS), Mod.ModPath, Mod.LogTag);
        }
    }

    /// <summary>
    /// Gives every asteroid the two mod components: renaming (the game's pencil in the
    /// details title) and the "hidden from the list" mark. Rocket interiors are not
    /// asteroids and never get them (rockets already have their own renaming).
    /// </summary>
    [HarmonyPatch(typeof(AsteroidConfig), nameof(AsteroidConfig.CreatePrefab))]
    public static class AsteroidConfig_CreatePrefab_Patch
    {
        public static void Postfix(GameObject __result)
        {
            if (__result == null) return;

            // If Klei ever add renaming to asteroids themselves, keep theirs and add nothing.
            if (__result.GetComponent<UserNameable>() == null)
                __result.AddComponent<AsteroidNameable>();
            else
                Debug.Log("[" + Mod.LogTag + "] asteroids already have the game's renaming, mod renaming skipped");

            __result.AddOrGet<AsteroidListState>();
        }
    }
}
