using UnityEngine;

namespace RenameHideAsteroids
{
    /// <summary>
    /// Copies of the game's own UI pieces from the pinned resources panel, so what the mod
    /// adds looks exactly like Klei's: the small orange "+ See all" link.
    /// </summary>
    internal static class GameUi
    {
        private static bool loggedMissing;

        /// <summary>The orange "+ See all" link. Its text is set with SetText.</summary>
        public static GameObject CopyLink(GameObject parent, string name, System.Action onClick)
        {
            PinnedResourcesPanel panel = PinnedResourcesPanel.Instance;
            GameObject go = Copy(panel != null ? panel.seeAllButton : null, parent, name, onClick);
            if (go == null) return null;

            // The text says it all; drop the game's "See all" tooltip that came with the copy.
            ToolTip tt = go.GetComponent<ToolTip>();
            if (tt != null) Object.Destroy(tt);
            return go;
        }

        /// <summary>Our own text on a copied label (not the game's translation key).</summary>
        public static void SetText(LocText label, string text)
        {
            if (label == null) return;
            label.key = "";
            label.SetText(text);
        }

        /// <summary>Font size of the game's "+ See all" link, or 0 if it isn't available.</summary>
        public static float LinkFontSize()
        {
            PinnedResourcesPanel panel = PinnedResourcesPanel.Instance;
            LocText label = panel != null && panel.seeAllButton != null ? panel.seeAllButton.GetComponentInChildren<LocText>(true) : null;
            return label != null ? label.fontSize : 0f;
        }

        private static GameObject Copy(MultiToggle template, GameObject parent, string name, System.Action onClick)
        {
            if (template == null)
            {
                // The resource panel is not ready yet (or Klei changed it); nothing is added.
                if (!loggedMissing) Debug.Log("[" + Mod.LogTag + "] resource panel buttons not found yet");
                loggedMissing = true;
                return null;
            }
            GameObject go = Util.KInstantiateUI(template.gameObject, parent, force_active: true);
            go.name = "RenameHideAsteroids_" + name;
            go.GetComponent<MultiToggle>().onClick = onClick;
            return go;
        }
    }
}
