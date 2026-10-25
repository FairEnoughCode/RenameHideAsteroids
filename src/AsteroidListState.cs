using KSerialization;

namespace RenameHideAsteroids
{
    /// <summary>
    /// Per-asteroid mark "hidden from the asteroid list", stored in the save.
    /// Without the mod the game simply skips this data when loading.
    /// The class name and the field name are part of the save format: do not rename them.
    /// </summary>
    [SerializationConfig(MemberSerialization.OptIn)]
    public class AsteroidListState : KMonoBehaviour
    {
        [Serialize]
        public bool hiddenInList;

        /// <summary>
        /// Is this world an asteroid the player has hidden? The start asteroid and rockets never
        /// are. With hiding switched off in the settings none is (the marks stay in the save).
        /// </summary>
        public static bool IsHidden(WorldContainer world)
        {
            if (!ModSettings.HideAsteroids || world == null || world.IsModuleInterior || world.IsStartWorld) return false;
            AsteroidListState state = world.GetComponent<AsteroidListState>();
            return state != null && state.hiddenInList;
        }
    }
}
