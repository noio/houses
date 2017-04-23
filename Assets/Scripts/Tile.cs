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

public enum TileMode
{
    OnField,
    OnStack,
    InCursor
}

public class Tile : MonoBehaviour {

	public TileType type;
    public Coords coords;
    public float stackPosition;

    SpriteRenderer _renderer;
    public SpriteRenderer sprite
    {
        get
        {
            return _renderer;
        }
    }
    TileMode _mode;
    public TileMode mode
    {
        get
        {
            return _mode;
        }
        set
        {
            if (value == TileMode.OnField)
            {
                sprite.color = Color.white;
                GetComponent<Collider2D>().enabled = true;
            }
            _mode = value;
        }
    }


    void Awake()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }
	// Update is called once per frame
	void Update ()
    {
        if (_mode == TileMode.OnField)
        {
            float t = (transform.position.x + Time.time) / 3;
            _renderer.transform.localPosition = new Vector3(0, 0.1f * Mathf.PerlinNoise(t, transform.position.y));
        }
        else if (_mode == TileMode.OnStack)
        {
            float t = Time.time * stackPosition * 3;
            float x = 0; //0.2f * stackPosition * (Mathf.PerlinNoise(transform.position.x, t) - .5f);
            float y = 0.2f * stackPosition * (Mathf.PerlinNoise(t, transform.position.y) - .5f);
            _renderer.transform.localPosition = new Vector3(x, y);
        }
	}


    public void Bump(Vector2 force)
    {
        GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
    }

}
