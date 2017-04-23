using System.Collections.Generic;

[System.Serializable]
public class Levels
{
    public List<Level> levels;
}

[System.Serializable]
public class Level
{
    public List<string> layout;
    public string tiles;
    public bool shuffleTiles;
    public int scoreToWin;
    public int maxStackSize;
}

