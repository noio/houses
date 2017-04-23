using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Serialization;

using DG.Tweening;


/*
- Levels
- Win condition
- Loss condition
- Juice it

 */

public struct Coords
{
    public int x;
    public int y;


    public Coords(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool Equals(Coords other)
    {
        return x.Equals(other.x) && y.Equals(other.y);
    }

    public override bool Equals(object obj)
    {
        if (obj is Coords)
        {
            return Equals((Coords)obj);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (391 + x.GetHashCode()) * 23 + y.GetHashCode();
    }

    public static Coords operator +(Coords a, Coords b)
    {
        return new Coords(a.x + b.x, a.y + b.y);
    }
    public static Coords operator -(Coords a, Coords b)
    {
        return new Coords(a.x - b.x, a.y - b.y);
    }

    public static Coords operator *(Coords a, Coords b)
    {
        return new Coords(a.x * b.x, a.y * b.y);
    }

    public override string ToString()
    {
        return "(" + x + "," + y + ")";
    }
    public static implicit operator Vector2(Coords coord)
    {
        return new Vector2(coord.x, coord.y);
    }
    public static implicit operator Vector3(Coords coord)
    {
        return new Vector3(coord.x, coord.y, coord.y * 0.0001f);
    }
}

public class RuleAction
{
    public RuleAction(Coords location, Rule2 rule)
    {
        this.location = location;
        this.rule = rule;
    }
    public Coords location;
    public Rule2 rule;
}

public class Game : MonoBehaviour
{
    public int score;

    static Coords[] Neighbors = new Coords[] {
        new Coords(0,1),
        new Coords(1,1),
        new Coords(1,0),
        new Coords(1,-1),
        new Coords(0,-1),
        new Coords(-1,-1),
        new Coords(-1,0),
        new Coords(-1,1),
    };

    public Vector2 tileCenter = new Vector2(4/8f, 7/16f);
    public Tile tilePrefab;
    public PlayingField fieldPrefab;
    public ParticleSystem demolishedEffect;
    public Sprite forestSprite;
    public Sprite emptySprite;
    public Sprite slumSprite;
    public Sprite midSprite;
    public Sprite mansionSprite;
    public TextAsset defaultRules;
    public TextAsset defaultLevels;

    Dictionary<Direction, Coords> offsets = new Dictionary<Direction, Coords>();
    Dictionary<Coords, Tile> _tiles = new Dictionary<Coords, Tile>();

    Queue<RuleAction> _actionQueue = new Queue<RuleAction>();
    List<TileType> _types = new List<TileType>();
    HashSet<Coords> _playingField = new HashSet<Coords>();

    PopulationStack _stack;
    Cursor _cursor;
    List<Rule2> _rules = new List<Rule2>();
    string _rulesPath;
    Transform _field;
    List<TileType> _tileMixture = new List<TileType>();
    List<TileType> _tileBuffer = new List<TileType>();
    Level _currentLevel;

    public Text scoreText;
    public InputField rulesText;
    public Levels levels;


    // Use this for initialization
    void Awake()
    {
        _rulesPath = Path.Combine(Application.persistentDataPath, "rules.txt");

        if (File.Exists(_rulesPath))
        {
            rulesText.text = File.ReadAllText(_rulesPath);
        }
        else
        {
            rulesText.text = defaultRules.text;
        }
        ParseRules(rulesText.text);

        JsonUtility.FromJsonOverwrite(defaultLevels.text, levels);



        // offsets[Direction.Top] = new Coords(0,1);
        // offsets[Direction.TopRight] = new Coords(1,1);
        // offsets[Direction.Right] = new Coords(1,0);
        // offsets[Direction.BottomRight] = new Coords(1,-1);
        // offsets[Direction.Bottom] = new Coords(0,-1);
        // offsets[Direction.BottomLeft] = new Coords(-1,-1);
        // offsets[Direction.Left] = new Coords(-1,0);
        // offsets[Direction.TopLeft] = new Coords(-1,1);
        // offsets[Direction.Self] = new Coords(0,0);


        foreach( var field in FindObjectsOfType<PlayingField>())
        {
            _playingField.Add(new Coords(Mathf.RoundToInt(field.transform.position.x), Mathf.RoundToInt(field.transform.position.y)));
        }

        var firstTile = MakeTile(TileType.Forest, new Coords(0,0));
        AddTile(firstTile);

        foreach (TileType type in System.Enum.GetValues(typeof(TileType)))
        {
            _types.Add(type);
        }

        _stack = FindObjectOfType<PopulationStack>();

        _cursor = FindObjectOfType<Cursor>();

        SetupLevel(levels.levels[1]);

        FillCursor();
        UpdateScore();
    }

    void ClearGame()
    {
        score = 0;
        foreach (var pair in _tiles)
        {
            Destroy(pair.Value.gameObject);
        }
        _tiles.Clear();
    }

    public void ParseRules(string newRules)
    {
        File.WriteAllText(_rulesPath, newRules);
        var lines = newRules.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        _rules.Clear();
        foreach(var line in lines)
        {
            var rule = Rule2.Parse(line);
            if (rule != null)
            {
                _rules.Add(rule);
            }
        }
    }

    Tile MakeTile(TileType type, Coords coords)
    {
        var tile = Instantiate(tilePrefab);
        tile.type = type;
        tile.coords = coords;
        tile.transform.localPosition = coords;
        var renderer = tile.GetComponent<SpriteRenderer>();
        switch (type)
        {
            case TileType.Empty: tile.sprite.sprite = emptySprite; break;
            case TileType.Forest: tile.sprite.sprite = forestSprite; break;
            case TileType.Slum: tile.sprite.sprite = slumSprite; break;
            case TileType.Apartment: tile.sprite.sprite = midSprite; break;
            case TileType.Mansion: tile.sprite.sprite = mansionSprite; break;
        }
        return tile;
    }

    void SetupLevel(Level level)
    {
        _currentLevel = level;
        ClearGame();
        if (_field != null)
        {
            Destroy(_field);
        }
        _field = new GameObject("Field").transform;
        _field.localPosition = new Vector3(0,0,1);

        _playingField.Clear();


        int height = level.layout.Count;
        int width = level.layout[0].Length;
        Coords mid = new Coords(width / 2, height / 2);
        for (int i = 0; i < width; i ++)
        {
            for (int j = 0; j < height; j ++)
            {
                var token = level.layout[j][i];
                if (token != ' ')
                {
                    var coord = new Coords(i, j) - mid;
                    _playingField.Add(coord);
                    var fieldTile = Instantiate(fieldPrefab).transform;
                    fieldTile.SetParent(_field);
                    fieldTile.localPosition = coord;

                    if (token != '-')
                    {
                        var tileType = ParseTileType(token.ToString());
                        var tile = MakeTile(tileType, coord);
                        AddTile(tile);
                    }
                }
            }
        }

        _tileBuffer.Clear();
        _tileMixture.Clear();
        foreach (char s in level.tiles)
        {
            _tileMixture.Add(ParseTileType(s.ToString()));
        }

        _stack.maxSize = _currentLevel.maxStackSize;
    }

    public bool CanPlace(List<Tile> tiles, Coords atPoint)
    {
        foreach (var tile in tiles)
        {
            var coords = tile.coords + atPoint;
            if (_tiles.ContainsKey(coords) || _playingField.Contains(coords) == false)
            {
                return false;
            }
        }
        return true;
    }

    public void PlaceTiles(List<Tile> tiles, Coords startingPoint)
    {
        foreach (var tile in tiles)
        {
            tile.coords += startingPoint;
            AddTile(tile);
        }

        DequeuePopulation();

        CheckRules();
        ProcessActions();

        FillCursor();
        UpdateScore();

    }

    void UpdateScore()
    {
        score = 0;
        foreach (var pair in _tiles)
        {
            if ((pair.Value.type & (TileType.Mansion | TileType.Apartment | TileType.Slum)) != 0)
            {
                score ++;
            }
        }
        scoreText.text = string.Format("{0}/{1}", score.ToString(), _currentLevel.scoreToWin);

    }

    void AddTile(Tile newTile)
    {
        Assert.IsFalse(_tiles.ContainsKey(newTile.coords));
        _tiles[newTile.coords] = newTile;
        var pos = (Vector3)newTile.coords;// + (Vector3) tileCenter;
        newTile.transform.SetParent(null);
        newTile.transform.DOLocalMove(pos, 0.3f);


        newTile.mode = TileMode.OnField;
        // newTile.GetComponent<SpriteRenderer>().sortingOrder = -newTile.coords.y;
        newTile.GetComponent<TargetJoint2D>().target = pos;
    }

    void DequeuePopulation()
    {
        var firstInQueue = _stack.Pop();
        if (firstInQueue != null)
        {
            var empties = new List<Tile>();
            foreach (var pair in _tiles)
            {
                if (pair.Value.type == TileType.Empty)
                {
                    empties.Add(pair.Value);
                }
            }
            empties.Shuffle();
            bool foundAPlace = false;
            foreach (var empty in empties)
            {
                bool anyRuleApplied = false;
                foreach (var rule in _rules)
                {
                    if (rule.effect != firstInQueue.type && RuleApplies(rule, empty.coords, firstInQueue.type))
                    {
                        anyRuleApplied = true;
                        break;
                    }
                }
                if (anyRuleApplied == false)
                {
                    firstInQueue.coords = empty.coords;
                    _tiles.Remove(empty.coords);
                    Destroy(empty.gameObject);
                    AddTile(firstInQueue);
                    foundAPlace = true;
                    break;
                }
            }
            if (foundAPlace == false)
            {
                _stack.AddTile(firstInQueue);
            }
        }


    }


    void FillCursor()
    {
        if (_tileBuffer.Count == 0)
        {
            _tileBuffer.AddRange(_tileMixture);
            if (_currentLevel.shuffleTiles)
            {
                _tileBuffer.Shuffle();
            }
        }

        var newTile = MakeTile(_tileBuffer[0], new Coords(0,0));
        _tileBuffer.RemoveAt(0);
        _cursor.AddTile(newTile);

        // for (int i = 0; i < 1; i ++){
        //     for (int j = 0; j < 1; j ++){
        //         TileType type;
        //         float v = Random.value;
        //         if (v <= 0.3)
        //         {
        //             type = TileType.Mansion;
        //         }
        //         else if (v <= 0.6)
        //         {
        //             type = TileType.Apartment;
        //         }
        //         else if (v <= 0.9)
        //         {
        //             type = TileType.Slum;
        //         }
        //         else
        //         {
        //             type = TileType.Forest;
        //         }
        //         var newTile = MakeTile(type, new Coords(i,j));
        //         _cursor.AddTile(newTile);
        //     }
        // }
    }

    void CheckRules()
    {
        var involvedNeighbors = new List<Tile>();
        foreach (var pair in _tiles)
        {
            var tile = pair.Value;

            involvedNeighbors.Clear();

            foreach (var rule in _rules)
            {
                if (RuleApplies(rule, tile.coords, tile.type, involvedNeighbors))
                {
                    _actionQueue.Enqueue(new RuleAction(tile.coords, rule));
                    ShowRule(tile.coords, involvedNeighbors);
                    BumpNeighbors(tile.coords);
                    break;
                }
            }
        }
    }

    bool RuleApplies(Rule2 rule, Coords coords, TileType currentType, List<Tile> involved = null)
    {
        if (rule == null)
        {
            return false;
        }

        if (currentType == rule.thisType && CountNeighbors(coords, rule.neighborType, involved) >= rule.neighborCount)
        {
            // var newTile = MakeTile(rule.effect, tile.coords);
            return true;
        }
        return false;
    }

    void ProcessActions()
    {
        while (_actionQueue.Count > 0)
        {
            var action = _actionQueue.Dequeue();
            Tile existing;
            if (_tiles.TryGetValue(action.location, out existing))
            {
                _tiles.Remove(existing.coords);
                if (action.rule.onStack)
                {
                    _stack.AddTile(existing);
                }
                else
                {
                    Destroy(existing.gameObject);
                }
            }
            AddTile(MakeTile(action.rule.effect, action.location));
        }
    }

    void BumpNeighbors(Coords coords)
    {
        foreach (Coords offset in Neighbors)
        {
            // var offset = offsets[dir];
            var tile = GetTile(coords + offset);
            if (tile != null)
            {
                tile.Bump((Vector2)offset * 6);
            }
        }
    }

    void ShowRule(Coords coords, List<Tile> involvedTiles)
    {
        var demolished = Instantiate(demolishedEffect);
        demolished.transform.position = coords;
        Debug.DrawLine(coords, coords + new Vector3(0,1,0), Color.red, 1.0f);
        foreach (var tile in involvedTiles)
        {
            tile.sprite.color = Color.red;
            tile.sprite.DOColor(Color.white, 3.0f).SetEase(Ease.InOutFlash, 9, 0);
        }
    }

    Tile GetTile(Coords coord)
    {
        Tile tile;
        if (_tiles.TryGetValue(coord, out tile))
        {
            return tile;
        }
        return null;
    }



    // bool CheckRuleOn(Rule rule, Tile tile, int flipX = 1, int flipY = 1)
    // {
    //     foreach (var condition in rule.conditions)
    //     {
    //         int count = 0;
    //         foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
    //         {
    //             if ((condition.position & dir) == dir) {
    //                 var offset = offsets[dir];
    //                 var newCoord = tile.coords + offset * new Coord(flipX, flipY);

    //                 Tile target = GetTile(newCoord);
    //                 if (target != null)
    //                 {
    //                     if ((target.type & condition.type) == target.type)
    //                     {
    //                         count ++;
    //                     }
    //                 }
    //             }
    //         }
    //         if (count < condition.count)
    //         {
    //             return false;
    //         }
    //     }

    //     var newTile = MakeTile(rule.effect);
    //     newTile.coords = tile.coords;
    //     _actionQueue.Enqueue(newTile);
    //     Debug.LogFormat("queued {0} at {1}", rule.effect, newTile.coords);

    //     return true;
    // }

    List<Tile> GetNeighbors(Coords coord)
    {
        var result = new List<Tile>();

        foreach (var offset in Neighbors)
        {
            var tile = GetTile(coord + offset);
            if (tile != null)
            {
                result.Add(tile);
            }
        }

        return result;
    }

    int CountNeighbors(Coords coord, TileType type, List<Tile> involved = null)
    {
        int count = 0;
        foreach (var neighbor in GetNeighbors(coord))
        {
            if ((neighbor.type & type) == neighbor.type)
            {
                count ++;
                if (involved != null)
                {
                    involved.Add(neighbor);
                }
            }
        }
        return count;
    }

    public static TileType ParseTileType(string input)
    {
        TileType type = (TileType) 0;
        foreach (var typeString in input.Split(new char[]{'/'}))
        {
            if (typeString.Length == 1)
            {
                switch (input)
                {
                    case "E": return TileType.Empty;
                    case "F": return TileType.Forest;
                    case "M": return TileType.Mansion;
                    case "A": return TileType.Apartment;
                    case "S": return TileType.Slum;
                }
            }
            type |= (TileType)System.Enum.Parse(typeof(TileType), typeString.TrimEnd(new char[]{'s'}), true);
        }
        return type;
    }


}
