using HarmonyLib;
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Windows;
using UnityModManagerNet;

// TODO: Rename this namespace to your mod's name.
namespace MyAdofaiMod
{

    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPatch(typeof(scrController), "CountValidKeysPressed")]
        [HarmonyPostfix]
        public static void ValidKey(int __result)
        {
            //MainClass.Logger.Log($"User pressed {__result} key(s).");
        }

        static private HitMargin ThisHitMargin = HitMargin.Perfect;

        // called when a value is added to the current score manager thingy
        [HarmonyPatch(typeof(scrMistakesManager), "AddHit")]
        [HarmonyPrefix]
        public static void AddHitFix(HitMargin hit)
        {

            ThisHitMargin = hit;

        }

        // called when a valid movement is triggered (Moving in menu, Tapping in game or missing in game)
        [HarmonyPatch(typeof(scrController), "Hit")]
        [HarmonyPostfix]
        public static void OnHit(bool __result)
        {
            
            scrController controller = scrController.instance;
            scrPlanet planet = controller.chosenplanet;
            scrConductor conductor = planet.conductor;

            planet.angle = 0;

            if (!controller.gameworld) return;

            //UnityModManager.Logger.Clear();

            //double marginScale = (planet.currfloor.nextfloor == null) ? 1.0 : planet.currfloor.nextfloor.marginScale;
            //HitMargin InputHit = scrMisc.GetHitMargin((float)planet.cachedAngle, (float)planet.targetExitAngle, controller.isCW, (float)(conductor.bpm * controller.speed), conductor.song.pitch, 1.0);
            HitMargin InputHit = ThisHitMargin;

            //MainClass.Logger.Log(InputHit.ToString() + ": " + controller.currentSeqID);
            //MainClass.Logger.Log(conductor.songposition_minusi +" : "+ planet.currfloor.entryTime);
            //MainClass.Logger.Log(Time.unscaledDeltaTime.ToString());

        }

        // on offical level loaded
        [HarmonyPatch(typeof(scrController), "EnterLevel")]
        [HarmonyPrefix]
        public static void OnEnterLevel(string worldAndLevel, bool speedTrial)
        {

            MainClass.Logger.Log(worldAndLevel);

        }

        // only runs when custom level is loaded
        [HarmonyPatch(typeof(scnGame), "LoadLevel")]
        [HarmonyPrefix]
        public static void OnEnterCustomLevel(string levelPath)
        {

            bool isCustom = false;

            if (levelPath.IndexOfAny(Path.GetInvalidPathChars()) == -1)
            {
                isCustom = true;
            }

            MainClass.Logger.Log(levelPath + ", " + isCustom);

        }

        // called when player fails
        [HarmonyPatch(typeof(scrController), "FailAction")]
        [HarmonyPrefix]
        public static void FailActionPatch()
        {

            MainClass.Logger.Log("User fail action, Discard current chunk!");

        }

        // idfk
        [HarmonyPatch(typeof(scnEditor), "ResetScene")]
        [HarmonyPrefix]
        public static void ResetScenePatch()
        {

            MainClass.Logger.Log("Reset Scene action!");

        }

        // triggerd when landing on generic portal
        [HarmonyPatch(typeof(scrController), "OnLandOnPortal")]
        [HarmonyPrefix]
        public static void OnLandOnPortalPatch()
        {

            if (scrController.instance.gameworld)
            {
                MainClass.Logger.Log("Has Finished Level!");
            }

        }

        // triggerd when landing on generic portal
        [HarmonyPatch(typeof(scrController), "Checkpoint_Exit")]
        [HarmonyPostfix]
        public static void OnCheckpointExit()
        {

            MainClass.Logger.Log("Exited Checkpoint");

        }

        // triggerd when controll is handed to the player!
        [HarmonyPatch(typeof(scrController), "PlayerControl_Enter")]
        [HarmonyPostfix]
        public static void PlayerControl_Enter()
        {

            scrController controller = scrController.instance;
            scrPlanet planet = controller.chosenplanet;
            scrConductor conductor = planet.conductor;

            MainClass.Logger.Log("PlayerControl_Enter");

            if (controller.currentSeqID == 0 && controller.gameworld) 
            {
                MainClass.Logger.Log("Level start, from begining!");
            }

        }


    }
}
