using ADOFAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AquaLeader
{
    [Serializable]
    struct ReplayInfo
    {
        public float Version;
        public string MD5LevelHash;

        public ReplayResultsInfo Results;

        public List<ReplayChunk> Chunks;

    }

    struct ReplayResultsInfo
    {
        public float XPercentage;
        public float Percentage;
    }

    [Serializable]
    struct ReplayChunk 
    { 
    
        public int ChunkIndex;
        public int ChunkAttempts;
        public List<InputAction> ChunkInputs;

    }

    [Serializable]
    struct InputAction
    {

        public HitMargin hitMargin;
        public double InputTime;
        public Vec2 PlanetPosition;
        public int FloorSeqID;

    }

    public struct Vec2
    {
        public float x;
        public float y;
    }

    public static class Utils
    {
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static Vec2 Vector2ToVec2(Vector2 vec2)
        {
            return new Vec2 { x = vec2.x, y = vec2.y };
        }

        public static bool SaveReplayFile(string replayFile, string FileName)
        {
            File.WriteAllText(Application.dataPath + "\\Replays\\" + FileName, replayFile);
            return true;
        }

        public static string RemoveInvalidChars(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string GetHashOfPath(string path)
        {
            string LevelContence = File.ReadAllText(path);
            return CreateMD5(LevelContence);
        }

    }
 
    
}
