using System;
using System.Runtime.CompilerServices;
using KMod;
using Newtonsoft.Json;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace RenameHideAsteroids
{
    /// <summary>
    /// The mod's settings, shown by PLib in the game's mod menu ("Options" button).
    /// Stored in the shared mods config folder, so they survive mod updates.
    /// The property names are the keys in the settings file: do not rename them after release.
    /// PLib shows the groups sorted by their key, so CATEGORY comes before CATEGORY_SHOW;
    /// inside a group the options keep the order below.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(SharedConfigLocation: true)]
    public class Settings
    {
        private const string Key = "RenameHideAsteroids.STRINGS.RENAMEHIDEASTEROIDS.OPTIONS.";

        [Option(Key + "HIDE_ASTEROIDS", Key + "HIDE_ASTEROIDS_TOOLTIP", Key + "CATEGORY")]
        [JsonProperty]
        public bool HideAsteroids { get; set; } = true;

        [Option(Key + "SHOW_ON_RED", Key + "SHOW_ON_RED_TOOLTIP", Key + "CATEGORY_SHOW")]
        [JsonProperty]
        public bool ShowOnRedAlert { get; set; } = true;

        [Option(Key + "SHOW_ON_YELLOW", Key + "SHOW_ON_YELLOW_TOOLTIP", Key + "CATEGORY_SHOW")]
        [JsonProperty]
        public bool ShowOnYellowAlert { get; set; } = false;
    }

    /// <summary>
    /// The settings the mod actually uses. PLib is only touched here, always inside try/catch:
    /// if PLib fails, only the settings window is lost and the mod runs with the defaults.
    /// </summary>
    internal static class ModSettings
    {
        public static bool HideAsteroids = true;
        public static bool ShowOnRedAlert = true;
        public static bool ShowOnYellowAlert = false;

        public static void Register(UserMod2 mod)
        {
            try
            {
                RegisterWithPLib(mod);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[" + Mod.LogTag + "] settings window not available, using defaults: " + e);
            }
        }

        /// <summary>Reads the settings file. Called whenever a game is loaded, so changes made in the mod menu apply.</summary>
        public static void Load()
        {
            try
            {
                ReadWithPLib();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[" + Mod.LogTag + "] settings not read, using defaults: " + e);
            }
        }

        // Kept in separate methods, so a broken PLib fails inside the try above
        // instead of when the calling method is first prepared to run.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RegisterWithPLib(UserMod2 mod)
        {
            PUtil.InitLibrary(false);
            new POptions().RegisterOptions(mod, typeof(Settings));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ReadWithPLib()
        {
            Settings s = POptions.ReadSettings<Settings>();
            if (s == null) return; // no file yet: the defaults stay
            HideAsteroids = s.HideAsteroids;
            ShowOnRedAlert = s.ShowOnRedAlert;
            ShowOnYellowAlert = s.ShowOnYellowAlert;
        }
    }
}
