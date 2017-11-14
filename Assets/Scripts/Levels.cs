using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using Newtonsoft.Json;
using UnityEngine;

internal class Level
{
    public Dictionary<int, List<SeriesInfo>> Options;
    public int MinSize;
    public int MaxSize;
    public int LevelUpScore;

    public Level(Dictionary<string, int>[] options, int levelUpScore)
    {
        Options = new Dictionary<int, List<SeriesInfo>>();
        foreach (var option in options)
        {
            int series;
            option.TryGetValue("series", out series);
            int score;
            option.TryGetValue("score", out score);
            var digits = Levels.NumberOfDigits(series);
            if (!Options.ContainsKey(digits))
                Options[digits] = new List<SeriesInfo>();
            Options[digits].Add(new SeriesInfo(series, score));
        }

        LevelUpScore = levelUpScore;
        MinSize = Options.Keys.Min();
        MaxSize = Options.Keys.Max();
    }
}

public class Levels
{
    private Level[] _levels;
    public int CurrentLevel;

    public int NextLevelScore
    {
        get { return _levels[CurrentLevel].LevelUpScore; }
        set { _levels[CurrentLevel].LevelUpScore = value; }
    }

    public int minSeriesSize
    {
        get { return _levels[CurrentLevel].MinSize; }
    }

    public int maxSeriesSize
    {
        get { return _levels[CurrentLevel].MaxSize; }
    }

    public Levels()
    {
        TextAsset asset = Resources.Load("Levels/Levels") as TextAsset;
        _levels = JsonConvert.DeserializeObject<Level[]>(asset.text);
    }

    public SeriesInfo get_series_info(int series)
    {
        var digits = NumberOfDigits(series);
        SeriesInfo ret = null;
        if (_levels[CurrentLevel].Options.ContainsKey(digits))
            ret = _levels[CurrentLevel].Options[digits].Find(x => x.Equals(new SeriesInfo {Series = series, Score = 0}));
        return ret;
    }

    public static int NumberOfDigits(int num)
    {
        return (int) Math.Floor(Math.Log10(num) + 1);
    }
}