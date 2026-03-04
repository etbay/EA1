using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public string sceneName;
    public string levelName;
    public Ranking.RankRequirements requirements;
    public string nextLevel;
}
