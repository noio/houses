﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class Cursor : MonoBehaviour
{
    Game _game;
    public Coords coords;
    List<Tile> _tiles = new List<Tile>();

    public void Awake()
    {
        _game = FindObjectOfType<Game>();
    }

    public void Update()
    {
        var screenPos = Input.mousePosition;
        var camera = Camera.main;
        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        // screenPos.y = camera.pixelHeight - screenPos.y;
        screenPos.z = 10;

        var worldPos = camera.ScreenToWorldPoint(screenPos);// + (Vector3)_game.tileCenter;
        coords.x = Mathf.RoundToInt(worldPos.x);// - 1;
        coords.y = Mathf.RoundToInt(worldPos.y);// - 1;
        transform.position = new Vector3(coords.x, coords.y, -1);

        if (_game.CanPlace(_tiles, coords))
        {
            SetTileColors(Color.white);
            if (Input.GetMouseButtonDown(0))
            {
                var tiles = new List<Tile>(_tiles);
                _tiles.Clear();
                _game.PlaceTiles(tiles, coords);
            }
        }
        else
        {
            SetTileColors(Color.red);
        }
    }

    void SetTileColors(Color color)
    {
        foreach(var tile in _tiles)
        {
            tile.sprite.color = color;
        }
    }

    public void AddTile(Tile tile)
    {
        tile.transform.SetParent(transform);
        tile.transform.localPosition = (Vector3)tile.coords;// + (Vector3)_game.tileCenter;
        tile.GetComponent<SpriteRenderer>().DOFade(0, 0);
        tile.GetComponent<SpriteRenderer>().DOFade(1, 0.4f);
        _tiles.Add(tile);
    }

}
