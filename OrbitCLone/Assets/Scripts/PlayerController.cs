using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float radius;
    private float speed = (2 * Mathf.PI) / 2;
    private float angle = 0;
    private Vector2 position;
    private int decay = 10;
    private int count = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        angle += speed * Time.deltaTime;
    }

    void FixedUpdate()
    {
        position.x = Mathf.Cos(angle)*radius;
        position.y = Mathf.Sin(angle)*radius;
        transform.position = position;

        if(count < decay)
        {
            count++;
        }       
        else
        {
            count = 0;
            radius -= .3f;
        }
    }
}
