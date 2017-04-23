using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour {

    public float time = 4;
	// Use this for initialization
	void Start () {
		Destroy(gameObject, time);
	}
}
