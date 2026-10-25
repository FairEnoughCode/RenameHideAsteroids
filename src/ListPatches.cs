using HarmonyLib;

namespace RenameHideAsteroids
{
    // The asteroid list has no official entry point, so the mod hooks five game methods by
    // name. If Klei rename one of them, that part of the mod stops working (an error in the
    // log); the game itself keeps working.

    /// <summary>The game decides which rows are visible here; the mod then hides the hidden ones.</summary>
    [HarmonyPatch(typeof(WorldSelector), "RefreshToggles")]
    public static class WorldSelector_RefreshToggles_Patch
    {
        public static void Postfix(WorldSelector __instance)
        {
            AsteroidList.Refresh(__instance);
        }
    }

    /// <summary>
    /// The game orders the rows here; the mod then moves the link and the hidden asteroids
    /// to the bottom. The game also calls this on its own (when a rocket lands or takes off).
    /// </summary>
    [HarmonyPatch(typeof(WorldSelector), "SortRows")]
    public static class WorldSelector_SortRows_Patch
    {
        public static void Postfix(WorldSelector __instance)
        {
            AsteroidList.Reorder(__instance);
        }
    }

    /// <summary>
    /// After a save loads, the resource panel (where the link is copied from) appears later
    /// than the asteroid list. As soon as it is ready, the list is refreshed once more, so the
    /// link shows up right away.
    /// </summary>
    [HarmonyPatch(typeof(PinnedResourcesPanel), "OnSpawn")]
    public static class PinnedResourcesPanel_OnSpawn_Patch
    {
        public static void Postfix()
        {
            if (WorldSelector.Instance != null && WorldSelector.Instance.worldRows != null)
                AsteroidList.Refresh(WorldSelector.Instance);
        }
    }

    /// <summary>
    /// Every 4 seconds the game checks each asteroid's state (the colored dot) and, when it
    /// gets worse, plays a sound and shakes the row. Right before that, the mod shows the
    /// hidden asteroids with an alert the player wants to see (settings).
    /// </summary>
    [HarmonyPatch(typeof(WorldSelector), "Sim4000ms")]
    public static class WorldSelector_Sim4000ms_Patch
    {
        public static void Prefix(WorldSelector __instance)
        {
            AsteroidList.CheckAlerts(__instance);
        }
    }

    /// <summary>
    /// The game's alert sound and shake for one row. The game plays the sound even for a row
    /// that is not visible; for a hidden asteroid the mod lets it through only once, when
    /// the asteroid surfaces under the link.
    /// (__0 is the asteroid's id: the first argument, whatever Klei name it.)
    /// </summary>
    [HarmonyPatch(typeof(WorldSelector), "TriggerVisualNotification")]
    public static class WorldSelector_TriggerVisualNotification_Patch
    {
        public static bool Prefix(int __0)
        {
            return AsteroidList.AlertSoundAllowed(__0);
        }
    }
}
