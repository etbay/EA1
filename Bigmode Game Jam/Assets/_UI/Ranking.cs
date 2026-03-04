using System;
using UnityEngine;
public class Ranking : MonoBehaviour
{
    [System.Serializable]
    public struct RankRequirements
    {
        // each double represents the time in seconds that is required for the rank
        public double dReq;
        public double cReq;
        public double bReq;
        public double aReq;
        public double sReq;
    }
    public enum Rank
    {
        D = 0,
        C = 1,
        B = 2,
        A = 3,
        S = 4
    }
    public static Rank GenerateRank(RankRequirements level, double playerTime)
    {
        if (playerTime < level.sReq)
        {
            return Rank.S;
        }
        else if (playerTime < level.aReq)
        {
            return Rank.A;
        }
        else if (playerTime < level.bReq)
        {
            return Rank.B;
        }
        else if (playerTime < level.cReq)
        {
            return Rank.C;
        }
        else
        {
            return Rank.D;
        }
    }
}
