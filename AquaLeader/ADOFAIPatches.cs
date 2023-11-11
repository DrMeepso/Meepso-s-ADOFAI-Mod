using HarmonyLib;
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Windows;
using UnityModManagerNet;

// TODO: Rename this namespace to your mod's name.
namespace AquaLeader
{

    [HarmonyPatch]
    public static class ADOFAIPatches
    {
        [HarmonyPatch(typeof(scrController), "CountValidKeysPressed")]
        [HarmonyPostfix]
        public static void ValidKey(int __result)
        {
            //MainClass.Logger.Log($"User pressed {__result} key(s).");
        }

        static private HitMargin ThisHitMargin = HitMargin.Perfect;
        static private int ChunkNumber = 0;

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

            if (!controller.gameworld) return;

            //UnityModManager.Logger.Clear();

            //double marginScale = (planet.currfloor.nextfloor == null) ? 1.0 : planet.currfloor.nextfloor.marginScale;
            //HitMargin InputHit = scrMisc.GetHitMargin((float)planet.cachedAngle, (float)planet.targetExitAngle, controller.isCW, (float)(conductor.bpm * controller.speed), conductor.song.pitch, 1.0);
            HitMargin InputHit = ThisHitMargin;

            //MainClass.Logger.Log(InputHit.ToString() + ": " + controller.currentSeqID);
            //MainClass.Logger.Log(conductor.songposition_minusi +" : "+ planet.currfloor.entryTime);
            //MainClass.Logger.Log(Time.unscaledDeltaTime.ToString());

            if (Saving.IsRecordingReplay)
            {
                Saving.OnReplayInput(InputHit, conductor.songposition_minusi, planet.other.transform.position, planet.currfloor.seqID);
            }

        }

        // on offical level loaded
        [HarmonyPatch(typeof(scrController), "EnterLevel")]
        [HarmonyPrefix]
        public static void OnEnterLevel(string worldAndLevel, bool speedTrial)
        {

            AquaMain.Log(worldAndLevel);

        }

        // only runs when custom level is loaded
        [HarmonyPatch(typeof(scnGame), "LoadLevel")]
        [HarmonyPrefix]
        public static void OnEnterCustomLevel(string levelPath)
        {

            

        }

        // called when player fails
        [HarmonyPatch(typeof(scrController), "FailAction")]
        [HarmonyPrefix]
        public static void FailActionPatch()
        {

            AquaMain.Log("User fail action, Discard current chunk!");
            Saving.ResetCurrentChunk();
            ChunkNumber--;

        }

        // idfk
        [HarmonyPatch(typeof(scnEditor), "ResetScene")]
        [HarmonyPrefix]
        public static void ResetScenePatch()
        {

            AquaMain.Log("Reset Scene action!");


        }

        // triggerd when landing on generic portal
        [HarmonyPatch(typeof(scrController), "OnLandOnPortal")]
        [HarmonyPrefix]
        public static void OnLandOnPortalPatch()
        {

            if (scrController.instance.gameworld)
            {
                //AquaMain.Log("Has Finished Level!");
                Saving.FinishRecording();

            }

        }

        // triggerd when respawning at a checkpoint after the count down
        [HarmonyPatch(typeof(scrController), "Checkpoint_Exit")]
        [HarmonyPostfix]
        public static void OnCheckpointExit()
        {

            //AquaMain.Log("Exited Checkpoint");

        }

        // triggered when respawning at a checkpoint before the count down 
        [HarmonyPatch(typeof(scrController), "Checkpoint_Enter")]
        [HarmonyPostfix]
        public static void OnCheckpointEnter()
        {

            AquaMain.Log("Entered Checkpoint");
            ChunkNumber++;

        }

        // triggered when new chunk is triggered
        [HarmonyPatch(typeof(scrMistakesManager), "MarkCheckpoint")]
        [HarmonyPostfix]
        public static void OnCheckpointUpdate()
        {

            ChunkNumber++;
            //AquaMain.Log("On Chunk: " + ChunkNumber);
            Saving.AddNewChunk(GCS.checkpointNum);

        }

        // triggerd when controll is handed to the player!
        [HarmonyPatch(typeof(scrController), "PlayerControl_Enter")]
        [HarmonyPostfix]
        public static void PlayerControl_Enter()
        {

            scrController controller = scrController.instance;
            scrConductor conductor = controller.chosenplanet.conductor;

            //MainClass.Log("PlayerControl_Enter");

            if (controller.currentSeqID == 0 && controller.gameworld) 
            {
                AquaMain.Log("Level start, from begining!");
                ChunkNumber = 0;
                Saving.StartNewReplay();
                AquaMain.Log(GCS.customLevelId == null ? "Offical" : "Custom");

                if (GCS.customLevelId != null)
                {
                    AquaMain.Log(conductor.customLevelComponent.levelPath);
                }
            }

        }

        // triggerd when controll is handed to the player!
        [HarmonyPatch(typeof(SteamManager), "Awake")]
        [HarmonyPostfix]
        public static void SteamManagerAwake()
        {

            AquaMain.Log("Steamworks Started!");

        }

    }
}
