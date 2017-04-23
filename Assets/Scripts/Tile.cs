using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Empty = 1,
    Forest = 1 << 1,
    Slum = 1 << 2,
    Apartment = 1 << 3,
    Mansion = 1 << 4,
}

public enum TileColor
{
    Blue,
    Red,
    Green
}

public class Tile : MonoBehaviour {

	public TileType type;
    public Coords coords;

    SpriteRenderer _renderer;
    public Sprite sprite
    {
        get
        {
            return renderer;
        }
    }

    void Awake()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }
	// Update is called once per frame
	void Update () {
        _renderer.transform.position = new Vector3(0, 0.01f * Mathf.PerlinNoise(transform.position.x + Time.time, transform.position.y));
	}
}
