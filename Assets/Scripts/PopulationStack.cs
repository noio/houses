using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class PopulationStack : MonoBehaviour
{
    Game _game;
    List<Tile> _stack = new List<Tile>();
    public SpriteRenderer background;
    int _maxSize;
    public int maxSize
    {
        get
        {
            return _maxSize;
        }
        set
        {
            _maxSize = value;
            if (_maxSize > 0)
            {
                background.enabled = true;
                background.transform.localScale = new Vector3(1,_maxSize,1);
                background.transform.localPosition = new Vector3(0, (_maxSize / 2.0f) -0.9f, 1 );
            }
            else
            {
                background.enabled = false;
            }
        }
    }

    public int size
    {
        get
        {
            return _stack.Count;
        }
    }

    public void Clear()
    {
        foreach (var tile in _stack)
        {
            Destroy(tile.gameObject);
        }
        _stack.Clear();
    }

    public void AddTile(Tile tile)
    {
        _stack.Add(tile);
        tile.mode = TileMode.OnStack;
        tile.transform.SetParent(transform);
        PositionTiles();
    }

    public Tile Pop()
    {
        if (_stack.Count > 0)
        {
            var toReturn = _stack[0];
            _stack.RemoveAt(0);
            PositionTiles();
            return toReturn;
        }
        return null;
    }

    void PositionTiles()
    {
        for (int i = 0; i < _stack.Count; i ++)
        {
            var tile = _stack[i];
            tile.coords = new Coords(0, i);
            tile.transform.DOLocalMove(tile.coords, 0.2f);
            if (i < maxSize)
            {
                tile.stackPosition = (i+1) / (float)maxSize;
            }
            else
            {
                tile.sprite.color = new Color(0.2f, 0.2f, 0.2f);
                // tile.GetComponent<TargetJoint2D>().enabled = false;
                // tile.GetComponent<Rigidbody2D>().gravityScale = 1;
            }
        }
    }

}
