using System;
using System.Collections.Generic;
using System.Reflection;
using ADOFAI.Editor.Actions;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

// TODO: Rename this namespace to your mod's name.
namespace AquaLeader
{
  
    // used for on screen logging
    struct LoggedValue
    {
        public string Message;
        public long UnixTime;
    }

    public static class AquaMain
    {

        public static bool IsEnabled { get; private set; }

        // UMM 
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static UnityModManager.ModEntry ModEntry { get; private set; }

        private static Harmony harmony;
        private static List<LoggedValue> loggedValues = new List<LoggedValue>();

        public static bool isRunningPlayback = false;

        internal static void Setup(UnityModManager.ModEntry modEntry) {
            Logger = modEntry.Logger;
            ModEntry = modEntry;

            // Add hooks to UMM event methods
            modEntry.OnToggle = OnToggle;
            modEntry.OnFixedGUI = OnGUI;

            GCS.banished = false;

        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            if (isRunningPlayback) 
            {
                Log("Cannot stop mod while watching replay");
                return false;
            }
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

            LoggedValue[] Logs = loggedValues.ToArray();
            Array.Reverse(Logs);

            for (int i = 0; i < Logs.Length; i++) 
            {

                GUI.Box(new Rect(10, (35 * i) + 5, 500, 30), Logs[i].Message);

                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Logs[i].UnixTime > 10)
                {
                    loggedValues.Remove(Logs[i]);
                }

            }

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

        public static void Log(string message)
        {
            LoggedValue thisLog = new LoggedValue { Message = message, UnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
            loggedValues.Add(thisLog);
            Logger.Log(message);
        }
    }
}
