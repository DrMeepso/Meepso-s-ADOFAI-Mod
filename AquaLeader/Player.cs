using ADOFAI;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AquaLeader
{   

    struct LoadedReplayFile
    {

        public string MapHash;
        public string AccPercentage;

        public ReplayInfo Replay;

        public string PathToReplay;
        public string PathToMap;

    }

    internal static class Player
    {

        public static bool IsPlayingReplay { get; private set; }
        public static List<LoadedReplayFile> LoadedReplays = new List<LoadedReplayFile>();

        public static ReplayInfo CurrentReplay;
        public static List<InputAction> allInputs;
        public static List<InputAction> finishedInputs;

        public static void Setup()
        {

            IsPlayingReplay = false;
            LoadedReplays = new List<LoadedReplayFile>();

            allInputs = new List<InputAction>();
            finishedInputs = new List<InputAction>();

        }

        private static InputAction[] actions;
        public static void Update()
        {

            scrController controller = scrController.instance;
            scrPlanet planet = controller.chosenplanet;
            scrConductor conductor = planet.conductor;

            SteamAPI.RunCallbacks();


            if (IsPlayingReplay && scrController.instance.gameworld)
            {

                for (int i = 0; i < actions.Length; i++)
                {
                    InputAction input = actions[i];

                    if (!finishedInputs.Contains(input))
                    {

                        //AquaMain.Log("i");

                        if (conductor.songposition_minusi > input.InputTime && controller.currentState == States.PlayerControl)
                        {

                            finishedInputs.Add(input);

                            input.hasRun = true;
                            controller.Hit();


                            AquaMain.Log("Run: " + i);

                        }

                    }

                }

            }

        }

        private static Dictionary<string, string> HashToPath = new Dictionary<string, string>();
        
        public static void getLevelsInDir(string dir)
        {

            if (!Directory.Exists(dir))
            {
                return;
            }

            string[] files = Directory.GetFiles(dir);
            foreach (string file in files)
            {
                
                if (File.Exists(file))
                {
                    if (Path.GetExtension(file) == ".adofai")
                    {

                        string hash = Utils.GetHashOfPath(file);

                        if (!HashToPath.ContainsKey(hash))
                        {
                            HashToPath.Add(hash, file);
                        }                
                    }
                }
            }

        }
        
        public static void FetchReplayFiles()
        {

            AquaMain.Log("Get Workshop");

            LoadedReplays.Clear();

            //SteamWorkshop.resultItems = new List<SteamWorkshop.ResultItem>();
            if (scnCLS.instance != null)
            {

                Dictionary<string, string> loadedLevels = Traverse.Create(scnCLS.instance).Field("loadedLevelDirs").GetValue() as Dictionary<string, string>;

                string[] values = loadedLevels.Values.ToArray();
                for (int i = 0;  i < values.Length; i++)
                {

                    getLevelsInDir(values[i]);

                }



            } else
            {
                AquaMain.Log("Unable to get files...");
            }

            AquaMain.Log("Fetching Saved Replays...");

            string[] files = Directory.GetFiles(Application.dataPath + "\\Replays");
            for (int i  = 0; i < files.Length; i++)
            {
                string file = files[i];
                string extention = Path.GetExtension(file);

                if (File.Exists(file) && extention == ".alrpl")
                {

                    string replayFileContence = File.ReadAllText(file);
                    ReplayInfo remadeReplay = Newtonsoft.Json.JsonConvert.DeserializeObject<ReplayInfo>(replayFileContence);

                    LoadedReplayFile thisReplay = new LoadedReplayFile
                    {
                        Replay = remadeReplay,
                        MapHash = remadeReplay.MD5LevelHash,
                        PathToReplay = file,
                        AccPercentage = (remadeReplay.Results.XPercentage * 100).ToString(),
                        PathToMap = HashToPath[remadeReplay.MD5LevelHash]
                    };

                    LoadedReplays.Add(thisReplay);

                }

            }

        }

        static private Rect SelectRec = new Rect(0, 0, 180, 500);
        static private Vector2 ReplayListScrollVector;
        public static void OnGUI()
        {

            scrController controller = scrController.instance;
            scrPlanet planet = controller.chosenplanet;
            scrConductor conductor = planet.conductor;

            bool correctPage = false;
            if (scnCLS.instance != null)
            {
                Dictionary<string, string> loadedLevels = Traverse.Create(scnCLS.instance).Field("loadedLevelDirs").GetValue() as Dictionary<string, string>;


                string[] values = loadedLevels.Values.ToArray();
                correctPage = values[0] != "";
            }

            if (controller == null || planet == null || conductor == null)
            {
                return;
            }

            if (IsPlayingReplay)
            {

            } else if(!IsPlayingReplay && !controller.gameworld && correctPage) // replay select
            {

                GUILayout.Window(8, SelectRec, (int winID) => {

                    GUILayout.Label("Replays...");
                    if(GUILayout.Button("Reload"))
                    {
                        FetchReplayFiles();
                    }

                    GUILayout.BeginScrollView(ReplayListScrollVector);

                    LoadedReplayFile[] files = LoadedReplays.ToArray();

                    for (int i = 0; i < files.Length; i++)
                    {

                        LoadedReplayFile replayFile = files[i];

                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                        GUILayout.Label(replayFile.PathToMap);
                        if (GUILayout.Button("Watch"))
                        {
                            AquaMain.Log("Watch Replay!");

                            StartReplay(replayFile);

                        }

                        GUILayout.EndHorizontal();

                    }
                    GUILayout.EndScrollView();

                    GUI.DragWindow(SelectRec);
                }, "AquaLeader");

            }

        }

        public static void StartReplay(LoadedReplayFile file)
        {

            allInputs.Clear();

            file.Replay.Chunks.ForEach(chunk => {
                AquaMain.Log("Chunk");
                allInputs.AddRange(chunk.ChunkInputs);
                AquaMain.Log(allInputs.Count + "");
            });

            actions = allInputs.ToArray();

            SceneManager.LoadScene("scnGame", LoadSceneMode.Single);
            GCS.customLevelPaths = new string[1];
            GCS.customLevelPaths[0] = file.PathToMap;
            GCS.checkpointNum = 0;
            GCS.customLevelId = "Replay";

            CurrentReplay = file.Replay;

            IsPlayingReplay = true;
            scrController.instance.gameworld = true;

        }

    }
}
