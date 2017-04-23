using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class PopQueue : MonoBehaviour
{
    Game _game;
    List<Tile> _queue = new List<Tile>();

    public void AddTile(Tile tile)
    {
        _queue.Add(tile);
        tile.transform.SetParent(transform);
        tile.coords = new Coords(0, _queue.Count - 1);
        tile.transform.DOLocalMove(tile.coords, 0.3f);
    }

    public Tile Pop()
    {
        if (_queue.Count > 0)
        {
            var toReturn = _queue[0];
            _queue.RemoveAt(0);
            for (int i = 0; i < _queue.Count; i ++)
            {
                _queue[i].coords = new Coords(0, i);
                _queue[i].transform.DOLocalMove(_queue[i].coords, 0.2f);
            }
            return toReturn;
        }
        return null;
    }

}
