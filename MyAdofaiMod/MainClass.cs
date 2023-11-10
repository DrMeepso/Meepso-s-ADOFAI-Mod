using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

// TODO: Rename this namespace to your mod's name.
namespace MyAdofaiMod
{
    /// <summary>
    /// The main class for the mod. Call other parts of your code from this
    /// class.
    /// </summary>
    public static class MainClass
    {
        /// <summary>
        /// Whether the mod is enabled. This is useful to have as a global
        /// property in case other parts of your mod's code needs to see if the
        /// mod is enabled.
        /// </summary>
        public static bool IsEnabled { get; private set; }

        // UMM 
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static UnityModManager.ModEntry ModEntry { get; private set; }

        private static Harmony harmony;

        internal static void Setup(UnityModManager.ModEntry modEntry) {
            Logger = modEntry.Logger;
            ModEntry = modEntry;

            // Add hooks to UMM event methods
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            IsEnabled = value;
            if (value) {
                StartMod(modEntry);
            } else {
                StopMod(modEntry);
            }
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry) 
        {

            Logger.Log("GUI");

        }

        private static void StartMod(UnityModManager.ModEntry modEntry) {
            // Patch everything in this assembly
            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void StopMod(UnityModManager.ModEntry modEntry) {
            // Unpatch everything
            harmony.UnpatchAll(modEntry.Info.Id);
        }
    }
}
