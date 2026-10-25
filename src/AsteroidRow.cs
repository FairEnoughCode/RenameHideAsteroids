using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RenameHideAsteroids
{
    /// <summary>
    /// Added to each row of the asteroid list. While the mouse is over the row it shows
    /// "Hide" (or "Show" for a hidden asteroid) in place of the game's status text
    /// ("Harvest" etc.), so the two never overlap. Without the
    /// mouse the row looks exactly like the game's. The component lives and dies with its row.
    /// </summary>
    internal class AsteroidRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly Vector3[] corners = new Vector3[4];

        private int worldId = -1;
        private RectTransform rowRect;
        private LocText status;          // the game's status text left of the dot
        private TextButton action;
        private bool allowHide, allowShow, hovered;

        public void Init(int id)
        {
            if (worldId == id) return;
            worldId = id;
            rowRect = GetComponent<RectTransform>();
            HierarchyReferences refs = GetComponent<HierarchyReferences>();
            status = refs != null ? refs.TryGetReference<LocText>("StatusLabel") : null;
        }

        /// <summary>Which action this row may offer on hover.</summary>
        public void SetButtons(bool hide, bool show)
        {
            allowHide = hide;
            allowShow = show;
            Apply();
        }

        public void OnPointerEnter(PointerEventData eventData) { hovered = true; Apply(); }

        public void OnPointerExit(PointerEventData eventData) { hovered = false; Apply(); }

        // A row the mod hides while the mouse is on it gets no "exit": reset here instead.
        private void OnDisable() { hovered = false; Apply(); }

        private void Apply()
        {
            bool show = hovered && (allowHide || allowShow) && EnsureAction();
            if (action != null)
            {
                if (action.gameObject.activeSelf != show) action.gameObject.SetActive(show);
                if (show)
                {
                    // Placed after it is switched on: the letters can only be measured then.
                    action.SetText(allowHide ? STRINGS.RENAMEHIDEASTEROIDS.HIDE_BUTTON : STRINGS.RENAMEHIDEASTEROIDS.SHOW_BUTTON);
                    Place();
                }
            }
            // The game only rewrites the status text, never switches it on or off, so this stays put.
            if (status != null && status.enabled == show) status.enabled = !show;
        }

        // Look of the "Hide" / "Show" text (chosen 17.10.2026 on mock-ups): the game's
        // yellow (Constants.WARNING_COLOR, the color of statuses like "Harvest"), white under
        // the mouse, the same font size as the "+ Hidden asteroids" link.
        // Place (25.10.2026): exactly where the game puts its own status text.
        private static readonly Color TextColor = new Color32(0xFF, 0xEE, 0x6E, 0xFF);
        private static readonly Color TextHoverColor = Color.white;
        private const float FallbackScale = 0.85f; // of the status text, if the game's link isn't found

        /// <summary>
        /// The text is a copy of the game's status text of this very row (same font), sized
        /// like the game's "+ See all" link. Made on first hover.
        /// </summary>
        private bool EnsureAction()
        {
            if (action != null) return true;
            if (status == null) return false;

            GameObject go = Instantiate(status.gameObject, transform, false);
            go.name = "RenameHideAsteroids_Action";
            // Size and place are set by the mod, not by the game's layout.
            ContentSizeFitter fitter = go.GetComponent<ContentSizeFitter>();
            if (fitter != null) DestroyImmediate(fitter);
            go.AddOrGet<LayoutElement>().ignoreLayout = true;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0.5f);

            LocText label = go.GetComponent<LocText>();
            label.enabled = true;
            label.raycastTarget = true;
            label.enableAutoSizing = false;
            float linkSize = GameUi.LinkFontSize();
            label.fontSize = linkSize > 0f ? linkSize : Mathf.Round(status.fontSize * FallbackScale);
            label.textWrappingMode = TMPro.TextWrappingModes.NoWrap;

            action = go.AddComponent<TextButton>();
            action.Init(label, TextColor, TextHoverColor);
            action.onClick = () => AsteroidList.SetHidden(worldId, allowHide);
            go.SetActive(false);
            return true;
        }

        /// <summary>
        /// The right edge and the height of the game's status text of this row, nothing of our own:
        /// if Klei move the status, "Hide" moves with it. The width is our own text's, because
        /// the game's frame shrinks to its text and is empty when the asteroid has no status.
        /// </summary>
        private void Place()
        {
            if (rowRect == null || status == null) return;
            LocText label = action.Label;
            RectTransform rt = label.rectTransform;
            status.rectTransform.GetWorldCorners(corners);
            Vector3 bottomLeft = rowRect.InverseTransformPoint(corners[0]);
            Vector3 topRight = rowRect.InverseTransformPoint(corners[2]);
            rt.sizeDelta = new Vector2(Mathf.Ceil(label.preferredWidth) + 2f, topRight.y - bottomLeft.y);
            float middle = (bottomLeft.y + topRight.y) * 0.5f;
            rt.anchoredPosition = new Vector2(topRight.x - rowRect.rect.xMax, middle - rowRect.rect.center.y);
        }
    }

    /// <summary>
    /// Clickable text: one color normally, another under the mouse.
    /// </summary>
    internal class TextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public System.Action onClick;
        public LocText Label { get; private set; }
        private Color normal, hover;

        public void Init(LocText label, Color normalColor, Color hoverColor)
        {
            Label = label;
            normal = normalColor;
            hover = hoverColor;
            label.color = normal;
        }

        public void SetText(string text)
        {
            Label.key = "";
            Label.SetText(text);
        }

        public void OnPointerEnter(PointerEventData eventData) { Label.color = hover; }

        public void OnPointerExit(PointerEventData eventData) { Label.color = normal; }

        private void OnDisable() { if (Label != null) Label.color = normal; }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (onClick != null) onClick();
        }
    }
}
