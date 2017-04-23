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
    public SpriteRenderer sprite
    {
        get
        {
            return _renderer;
        }
    }


    void Awake()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }
	// Update is called once per frame
	void Update () {
        _renderer.transform.localPosition = new Vector3(0, 0.1f * Mathf.PerlinNoise((transform.position.x + Time.time) / 3, transform.position.y));
	}
    void SetOnField (bool onField)
    {

    }


    public void Bump(Vector2 force)
    {
        GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
    }

}
