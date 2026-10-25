using System;
using System.Collections.Generic;
using UnityEngine;

namespace RenameHideAsteroids
{
    /// <summary>
    /// The asteroid list on the right: which rows are hidden, where they go, and the
    /// "+ Hidden asteroids (N)" link under the list.
    /// </summary>
    internal static class AsteroidList
    {
        private static WorldSelector owner;
        private static MultiToggle link;
        private static LocText linkLabel;
        private static bool expanded;
        // Hidden asteroids shown under the link because of an alert the player wants to see.
        private static HashSet<int> alerted = new HashSet<int>();
        // Hidden asteroids that surfaced in this very check: they get the game's sound and shake once.
        private static readonly HashSet<int> justSurfaced = new HashSet<int>();

        public static void SetHidden(int worldId, bool hidden)
        {
            WorldContainer world = ClusterManager.Instance.GetWorld(worldId);
            AsteroidListState state = world != null ? world.GetComponent<AsteroidListState>() : null;
            if (state == null) return;
            state.hiddenInList = hidden;
            ClusterGridEntity entity = world.GetComponent<ClusterGridEntity>();
            RequestRefresh(entity != null ? entity.Name : "");
        }

        /// <summary>
        /// Asks the list to rebuild itself. WorldRenamed is the event the game sends after a
        /// rocket is renamed; the asteroid list is its only listener and simply refreshes.
        /// Like the game, it always carries a name (never null), so any other listener stays safe.
        /// </summary>
        public static void RequestRefresh(string name)
        {
            if (ClusterManager.Instance != null)
                ClusterManager.Instance.Trigger((int)GameHashes.WorldRenamed, name ?? "");
        }

        public static void Refresh(WorldSelector selector)
        {
            try
            {
                if (selector.worldRows == null) return;
                if (owner != selector)
                {
                    // A new list comes with every loaded game: forget the old one,
                    // and pick up settings changed in the mod menu since the last game.
                    owner = selector;
                    link = null;
                    linkLabel = null;
                    expanded = false;
                    alerted.Clear();
                    justSurfaced.Clear();
                    ModSettings.Load();
                }

                int activeId = CameraController.Instance != null ? CameraController.Instance.cameraActiveCluster : -1;
                int hiddenCount = 0;

                foreach (KeyValuePair<int, MultiToggle> pair in selector.worldRows)
                {
                    int id = pair.Key;
                    WorldContainer world = ClusterManager.Instance.GetWorld(id);
                    if (pair.Value == null || world == null) continue;

                    GameObject go = pair.Value.gameObject;
                    AsteroidRow row = go.AddOrGet<AsteroidRow>();
                    row.Init(id);
                    bool shownByGame = go.activeSelf; // undiscovered asteroids are already hidden by the game

                    if (world.IsModuleInterior)
                    {
                        // A rocket parked on a hidden asteroid hides with it and shows with it
                        // (camera on it, the link opened, or an alert there); a rocket in flight
                        // (its parent is itself) always stays.
                        int parentId = world.ParentWorldId;
                        bool parked = parentId != id && parentId != 255;
                        if (shownByGame && parked && !expanded && id != activeId && parentId != activeId
                            && !alerted.Contains(parentId)
                            && AsteroidListState.IsHidden(ClusterManager.Instance.GetWorld(parentId)))
                            go.SetActive(false);
                        row.SetButtons(false, false);
                        continue;
                    }

                    if (!AsteroidListState.IsHidden(world))
                    {
                        // No "Hide" on the start asteroid, and none on the row the camera is on:
                        // hiding it would change nothing until the camera leaves.
                        row.SetButtons(ModSettings.HideAsteroids && !world.IsStartWorld && id != activeId, false);
                        continue;
                    }

                    if (world.IsDiscovered) hiddenCount++;
                    // A hidden asteroid still shows while the camera is on it, when the link is
                    // open, and during an alert the player chose to see (settings).
                    if (shownByGame && id != activeId && !expanded && !alerted.Contains(id)) go.SetActive(false);
                    row.SetButtons(false, true);
                }

                if (hiddenCount == 0) expanded = false;
                UpdateLink(hiddenCount);
                // The game sorted the rows before this method ran, while all of them were still
                // visible and the link might not exist yet, so the order is fixed up once more.
                Reorder(selector);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[" + Mod.LogTag + "] asteroid list not updated: " + e);
            }
        }

        /// <summary>
        /// Runs right before the game's own check of the asteroids' state (every 4 seconds).
        /// A hidden asteroid with an alert the player wants to see is shown under the link
        /// now, so the game's sound and shake that follow land on a visible row. Once shown,
        /// it stays while it has any alert (a status jumping between red and yellow must not
        /// make it blink) and hides again when the dot is back to normal.
        /// </summary>
        public static void CheckAlerts(WorldSelector selector)
        {
            try
            {
                if (owner != selector || selector.worldRows == null) return;
                justSurfaced.Clear();
                HashSet<int> now = new HashSet<int>();
                ColonyDiagnosticUtility diagnostics = ColonyDiagnosticUtility.Instance;
                if (diagnostics != null && ModSettings.HideAsteroids
                    && (ModSettings.ShowOnRedAlert || ModSettings.ShowOnYellowAlert))
                {
                    foreach (int id in selector.worldRows.Keys)
                    {
                        if (!AsteroidListState.IsHidden(ClusterManager.Instance.GetWorld(id))) continue;
                        ColonyDiagnostic.DiagnosticResult.Opinion opinion = diagnostics.GetWorldDiagnosticResult(id);
                        if (alerted.Contains(id) ? IsAnyAlert(opinion) : AlertShows(opinion))
                            now.Add(id);
                    }
                }
                foreach (int id in now)
                    if (!alerted.Contains(id)) justSurfaced.Add(id);
                if (now.SetEquals(alerted)) return;
                alerted = now;
                RequestRefresh("");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[" + Mod.LogTag + "] alerts not checked: " + e);
            }
        }

        /// <summary>Red (threat, bad) and yellow (warning, concern), exactly as the game colors the status dot.</summary>
        private static bool AlertShows(ColonyDiagnostic.DiagnosticResult.Opinion opinion)
        {
            switch (opinion)
            {
                case ColonyDiagnostic.DiagnosticResult.Opinion.DuplicantThreatening:
                case ColonyDiagnostic.DiagnosticResult.Opinion.Bad:
                    return ModSettings.ShowOnRedAlert;
                case ColonyDiagnostic.DiagnosticResult.Opinion.Warning:
                case ColonyDiagnostic.DiagnosticResult.Opinion.Concern:
                    return ModSettings.ShowOnYellowAlert;
                default:
                    return false;
            }
        }

        /// <summary>Any colored dot, red or yellow.</summary>
        private static bool IsAnyAlert(ColonyDiagnostic.DiagnosticResult.Opinion opinion)
        {
            return opinion >= ColonyDiagnostic.DiagnosticResult.Opinion.DuplicantThreatening
                && opinion <= ColonyDiagnostic.DiagnosticResult.Opinion.Concern;
        }

        /// <summary>
        /// May the game play its alert sound and shake this row? Every row of the game's own
        /// list: yes, as always. A hidden asteroid: only once, when it surfaces under the
        /// link; the game's repeats (a status jumping between red and yellow) stay silent,
        /// and so does an alert the player chose not to see.
        /// </summary>
        public static bool AlertSoundAllowed(int worldId)
        {
            try
            {
                if (!AsteroidListState.IsHidden(ClusterManager.Instance.GetWorld(worldId))) return true;
                return justSurfaced.Remove(worldId);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[" + Mod.LogTag + "] alert sound check failed: " + e);
                return true;
            }
        }

        /// <summary>
        /// Keeps the game's order, then puts the link at the very bottom and every shown
        /// hidden asteroid right under it (opened list, or the one the camera is on), so a
        /// hidden asteroid always appears in the same place. Its shown parked rockets follow it.
        /// </summary>
        public static void Reorder(WorldSelector selector)
        {
            try
            {
                if (link == null || owner != selector || selector.worldRows == null) return;
                link.transform.SetAsLastSibling();

                // Shown hidden asteroids, in the game's order.
                List<KeyValuePair<int, MultiToggle>> hiddenRows = new List<KeyValuePair<int, MultiToggle>>();
                foreach (KeyValuePair<int, MultiToggle> pair in selector.worldRows)
                {
                    if (pair.Value == null || !pair.Value.gameObject.activeSelf) continue;
                    if (AsteroidListState.IsHidden(ClusterManager.Instance.GetWorld(pair.Key)))
                        hiddenRows.Add(pair);
                }
                hiddenRows.Sort((a, b) => a.Value.transform.GetSiblingIndex().CompareTo(b.Value.transform.GetSiblingIndex()));

                foreach (KeyValuePair<int, MultiToggle> asteroid in hiddenRows)
                {
                    asteroid.Value.transform.SetAsLastSibling();
                    // Its parked rockets (shown while the camera is there) move along, right under it.
                    foreach (KeyValuePair<int, MultiToggle> pair in selector.worldRows)
                    {
                        if (pair.Value == null || !pair.Value.gameObject.activeSelf) continue;
                        WorldContainer world = ClusterManager.Instance.GetWorld(pair.Key);
                        if (world != null && world.IsModuleInterior && world.ParentWorldId == asteroid.Key)
                            pair.Value.transform.SetAsLastSibling();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[" + Mod.LogTag + "] asteroid list not sorted: " + e);
            }
        }

        private static void UpdateLink(int hiddenCount)
        {
            if (link == null)
            {
                if (hiddenCount == 0 || owner.worldRowContainer == null) return;
                GameObject go = GameUi.CopyLink(owner.worldRowContainer, "HiddenLink", () =>
                {
                    expanded = !expanded;
                    RequestRefresh("");
                });
                if (go == null) return;
                link = go.GetComponent<MultiToggle>();
                linkLabel = go.GetComponentInChildren<LocText>();
            }
            bool show = hiddenCount > 0;
            if (link.gameObject.activeSelf != show) link.gameObject.SetActive(show);
            if (show) GameUi.SetText(linkLabel, string.Format(STRINGS.RENAMEHIDEASTEROIDS.HIDDEN_LINK, hiddenCount));
        }
    }
}
