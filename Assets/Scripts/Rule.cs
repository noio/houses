﻿using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    TopLeft = 1,
    Top = 1 << 1,
    TopRight = 1 << 2,
    Left = 1 << 3,
    // Self = 1 << 4,
    Right = 1 << 5,
    BottomLeft = 1 << 6,
    Bottom = 1 << 7,
    BottomRight = 1 << 8,
}

// [System.Serializable]
// public struct Condition
// {
//     [DirectionButtons]
//     public Direction position;
//     public int count;
//     [EnumFlagButtons]
//     public TileType type;
// }

// [CreateAssetMenu]
// public class Rule : ScriptableObject
// {
//     // public bool checkFlipped = true;
//     // public List<Condition> conditions;

//     public TileType currentType;
//     public TileType neighborType;
//     public int minCount;
//     public TileType effect;

// }

public class Rule2
{
    public TileType thisType;
    public TileType neighborType;
    public int neighborCount;
    public TileType effect;
    public bool onStack;

    static string _regex;

    public static Rule2 Parse(string ruleString)
    {
        if (_regex == null)
        {
            var types = String.Join("|",System.Enum.GetNames(typeof(TileType)));
            _regex = String.Format("(?<type>(\\/?({0})s?)+) (between|near|next to) (?<count>\\d+) (?<neighbors>(\\/?({0})s?)+)s? becomes (?<newType>(\\/?({0})s?)+)s?(?<stack> .+ stack)?", types);
            Debug.LogFormat("Using Regex to parse rules: {0}",_regex);
        }
        var match = Regex.Match(ruleString, _regex, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var r = new Rule2();
            r.thisType = Game.ParseTileType(match.Groups["type"].Value);
            r.neighborType = Game.ParseTileType(match.Groups["neighbors"].Value);
            r.neighborCount = int.Parse(match.Groups["count"].Value);
            r.effect = Game.ParseTileType(match.Groups["newType"].Value);
            r.onStack = String.IsNullOrEmpty(match.Groups["stack"].Value) == false;
            Debug.LogFormat("Rule parsed: {0} near {2} {1} becomes {3}. Stacked: {4}", r.thisType, r.neighborType, r.neighborCount, r.effect, r.onStack);
            return r;
        }
        Debug.LogFormat("failed to read regex");

        return null;
    }


}
