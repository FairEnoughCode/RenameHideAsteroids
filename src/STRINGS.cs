namespace RenameHideAsteroids
{
    /// <summary>
    /// UI text. English is the default; translations live in translations/&lt;language&gt;.po
    /// with keys like "RenameHideAsteroids.STRINGS.RENAMEHIDEASTEROIDS.HIDDEN_LINK".
    /// </summary>
    public static class STRINGS
    {
        public static class RENAMEHIDEASTEROIDS
        {
            /// <summary>Link under the asteroid list; {0} is the number of hidden asteroids.</summary>
            public static LocString HIDDEN_LINK = "+ Hidden asteroids ({0})";
            public static LocString HIDE_BUTTON = "Hide";
            public static LocString SHOW_BUTTON = "Show";

            /// <summary>Settings window (PLib, "Options" button in the mod menu).</summary>
            public static class OPTIONS
            {
                public static LocString CATEGORY = "Hidden asteroids";
                public static LocString HIDE_ASTEROIDS = "Hide asteroids";
                public static LocString HIDE_ASTEROIDS_TOOLTIP = "When enabled, asteroids can be hidden in the right panel.";
                public static LocString CATEGORY_SHOW = "Show a hidden asteroid";
                public static LocString SHOW_ON_RED = "On red alert";
                public static LocString SHOW_ON_RED_TOOLTIP = "Works only if \"Hide asteroids\" is enabled.\nWhen enabled, a hidden asteroid with a red alert temporarily reappears in the list until the alert is over.\nThe alert sound plays only once.";
                public static LocString SHOW_ON_YELLOW = "On yellow alert";
                public static LocString SHOW_ON_YELLOW_TOOLTIP = "Works only if \"Hide asteroids\" is enabled.\nWhen enabled, a hidden asteroid with a yellow alert temporarily reappears in the list until the alert is over.\nThe alert sound plays only once.";
            }
        }
    }
}
