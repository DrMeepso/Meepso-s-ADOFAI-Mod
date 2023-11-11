using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Newtonsoft.Json;

namespace AquaLeader
{
    static class Saving
    {

        public static bool IsRecordingReplay = false;
        public static ReplayChunk CurrentChunk;
        public static List<ReplayChunk> AllChunks;

        static public void StartNewReplay()
        {

            AllChunks = new List<ReplayChunk>();

            CurrentChunk = new ReplayChunk
            {
                ChunkAttempts = 0,
                ChunkIndex = 0,
                ChunkInputs = new List<InputAction>()
            };
            AllChunks.Clear();

            IsRecordingReplay = true;

            AquaMain.Log("Replay Recording Started!");            

        }

        static public void OnReplayInput(HitMargin hitInfo, double InputTime, Vector2 PlanetPos, int SeqID)
        {

            InputAction action = new InputAction
            {
                hitMargin = hitInfo,
                InputTime = InputTime,
                PlanetPosition = Utils.Vector2ToVec2(PlanetPos),
                FloorSeqID = SeqID
            };

            CurrentChunk.ChunkInputs.Add(action);

        }

        static public void ResetCurrentChunk()
        {
            CurrentChunk.ChunkInputs.Clear();

            AquaMain.Log("Current chunk: " + CurrentChunk.ChunkIndex + ", Input cleared");
        }

        static public void AddNewChunk(int ChunkID)
        {

            AquaMain.Log("New Chunk Pushed! " + ChunkID);

            AllChunks.Add(CurrentChunk);
            CurrentChunk = new ReplayChunk
            {
                ChunkAttempts = 0,
                ChunkIndex = ChunkID,
                ChunkInputs = new List<InputAction>()
            };
        }

        static public void FinishRecording()
        {

            scrController controller = scrController.instance;
            scrPlanet planet = controller.chosenplanet;
            scrConductor conductor = planet.conductor;

            IsRecordingReplay = false;

            AllChunks.Add(CurrentChunk);
            AquaMain.Log("Saving Replay, ChunkCount: " + AllChunks.Count);

            if (GCS.customLevelId != null)
            {
                scnGame ThisGame = conductor.customLevelComponent;
                string LevelPath = ThisGame.levelPath;
                AquaMain.Log(LevelPath);
                if (File.Exists(LevelPath))
                {
                    string LevelContence = File.ReadAllText(LevelPath);
                    string LevelHash = Utils.CreateMD5(LevelContence);
                    AquaMain.Log(Utils.CreateMD5(LevelContence));

                    ReplayInfo ThisReplay = new ReplayInfo
                    {
                        Chunks = AllChunks,
                        MD5LevelHash = LevelHash,
                        Version = 1
                    };

                    string SerializedReplay = JsonConvert.SerializeObject(ThisReplay);
                    string FileName = ThisGame.levelData.song + "-" + DateTimeOffset.UtcNow.ToString() + ".ALRpl";
                    Utils.SaveReplayFile(SerializedReplay, FileName);

                }
            }
            else
            {
                AquaMain.Log("Unable to save replays of offical levels!");
            }

        }

    }
}
