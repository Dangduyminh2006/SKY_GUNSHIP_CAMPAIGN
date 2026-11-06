using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dữ liệu một lượt chơi trong lịch sử
/// </summary>
[Serializable]
public class MatchEntry
{
    public string date;
    public int score;
    public int wave;
    public int bossKills;
    public bool isVictory;
}

/// <summary>
/// Cấu trúc lưu trữ toàn bộ lịch sử đấu và kỷ lục
/// </summary>
[Serializable]
public class MatchHistoryData
{
    public int highScore = 0;
    public int maxWave = 1;
    public List<MatchEntry> matches = new List<MatchEntry>();
}

/// <summary>
/// Quản lý lưu trữ và truy xuất lịch sử chơi game qua PlayerPrefs (JSON UTF-8)
/// </summary>
public static class MatchHistoryManager
{
    private const string PrefsKey = "FlappyBeatUp_MatchHistory";
    private static MatchHistoryData cachedData = null;

    public static MatchHistoryData LoadHistory()
    {
        if (cachedData != null) return cachedData;

        string json = PlayerPrefs.GetString(PrefsKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            cachedData = new MatchHistoryData();
        }
        else
        {
            try
            {
                cachedData = JsonUtility.FromJson<MatchHistoryData>(json);
                if (cachedData == null) cachedData = new MatchHistoryData();
            }
            catch
            {
                cachedData = new MatchHistoryData();
            }
        }
        return cachedData;
    }

    public static void SaveMatch(int score, int wave, int bossKills, bool isVictory)
    {
        MatchHistoryData data = LoadHistory();

        if (score > data.highScore)
        {
            data.highScore = score;
        }
        if (wave > data.maxWave)
        {
            data.maxWave = wave;
        }

        MatchEntry entry = new MatchEntry
        {
            date = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            score = score,
            wave = wave,
            bossKills = bossKills,
            isVictory = isVictory
        };

        data.matches.Insert(0, entry);
        if (data.matches.Count > 15)
        {
            data.matches.RemoveRange(15, data.matches.Count - 15);
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(PrefsKey, json);
        PlayerPrefs.Save();
        cachedData = data;
    }

    public static int GetHighScore()
    {
        return LoadHistory().highScore;
    }

    public static int GetMaxWave()
    {
        return LoadHistory().maxWave;
    }

    public static void ClearHistory()
    {
        PlayerPrefs.DeleteKey(PrefsKey);
        PlayerPrefs.Save();
        cachedData = new MatchHistoryData();
    }
}

