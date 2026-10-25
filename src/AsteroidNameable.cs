using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using KSerialization;

namespace RenameHideAsteroids
{
    /// <summary>
    /// The game's own renaming component, adapted for asteroids.
    ///
    /// Having a UserNameable is what makes the details panel show the pencil in its title
    /// (DetailsScreen looks for it), and the game itself calls SetName when the player
    /// confirms a new name. After that this component writes the name into the asteroid
    /// (AsteroidGridEntity), the field the game saves, so the name stays even if the mod
    /// is disabled later.
    /// </summary>
    [SerializationConfig(MemberSerialization.OptIn)]
    public class AsteroidNameable : UserNameable
    {
        // Characters that are not allowed in a Windows folder name. The game creates a
        // timelapse screenshot folder named after the asteroid, so a name with them would fail there.
        private static readonly char[] Forbidden = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        private int nameChangedHandle = -1;
        private bool applying;

        protected override void OnSpawn()
        {
            // base.OnSpawn() is deliberately not called: UserNameable would re-apply its own
            // saved copy of the name. The real name lives in the asteroid itself and the game
            // already applies it on spawn (ClusterGridEntity.OnSpawn).
            nameChangedHandle = Subscribe((int)GameHashes.NameChanged, OnNameChanged);
        }

        protected override void OnCleanUp()
        {
            if (nameChangedHandle != -1) Unsubscribe(nameChangedHandle);
            nameChangedHandle = -1;
            base.OnCleanUp();
        }

        private void OnNameChanged(object data)
        {
            if (applying) return;

            AsteroidGridEntity asteroid = GetComponent<AsteroidGridEntity>();
            if (asteroid == null) return;

            applying = true;
            try
            {
                string requested = data as string;
                string clean = Clean(requested);
                if (string.IsNullOrEmpty(clean)) clean = asteroid.Name; // nothing usable typed: keep the old name

                // Fixes the selection name and the title if characters had to be removed.
                if (clean != requested) SetName(clean);

                WriteIntoAsteroid(asteroid, clean);

                // Refresh what shows the name: the label on the starmap and the asteroid list
                // (the list listens to the same event the game sends when a rocket is renamed).
                if (ClusterNameDisplayScreen.Instance != null)
                    ClusterNameDisplayScreen.Instance.UpdateName(asteroid);
                if (ClusterManager.Instance != null)
                    ClusterManager.Instance.Trigger((int)GameHashes.WorldRenamed, clean);
                // The title bar writes the typed text back into the title right after this
                // event, so the cleaned name is put there one frame later.
                if (clean != requested && isActiveAndEnabled)
                    StartCoroutine(RefreshTitleNextFrame());
            }
            catch (Exception e)
            {
                Debug.LogWarning("[" + Mod.LogTag + "] asteroid not renamed: " + e);
            }
            finally
            {
                applying = false;
            }
        }

        private IEnumerator RefreshTitleNextFrame()
        {
            yield return null;
            if (DetailsScreen.Instance != null && DetailsScreen.Instance.target == gameObject)
                DetailsScreen.Instance.UpdateTitle();
        }

        /// <summary>
        /// Puts the new name into the asteroid through its public Init method, the same one
        /// the game calls when it creates the asteroid. Location and picture are passed back
        /// unchanged, so only the name changes.
        /// </summary>
        private static void WriteIntoAsteroid(AsteroidGridEntity asteroid, string name)
        {
            string anim = AsteroidGridEntity.DEFAULT_ASTEROID_ICON_ANIM;
            List<ClusterGridEntity.AnimConfig> configs = asteroid.AnimConfigs;
            if (configs != null && configs.Count > 0 && configs[0].animFile != null)
                anim = configs[0].animFile.name;
            asteroid.Init(name, asteroid.Location, anim);
        }

        private static string Clean(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            StringBuilder sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (char.IsControl(c) || Array.IndexOf(Forbidden, c) >= 0) continue;
                sb.Append(c);
            }
            // Windows folder names can't end with a dot or a space either.
            return sb.ToString().Trim().TrimEnd('.').Trim();
        }
    }
}
