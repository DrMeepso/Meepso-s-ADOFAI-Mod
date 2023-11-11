using UnityModManagerNet;

// TODO: Rename this namespace to your mod's name.
namespace AquaLeader
{
    /// <summary>
    /// Entry class for the mod.
    /// </summary>
    internal static class Startup
    {
        // entry point for UMM
        internal static void Load(UnityModManager.ModEntry modEntry) {
            AquaMain.Setup(modEntry);
        }
    }
}
