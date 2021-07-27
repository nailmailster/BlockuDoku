using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length, startPos;
    public GameObject cam;
    public float parallaxEffect;

    void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        // print("length = " + length);
    }

    void Update()
    {
        //  это из урока Dani, где параллакс привязан к камере
        // float temp = cam.transform.position.x * (1 - parallaxEffect);
        // float dist = cam.transform.position.x * parallaxEffect;

        // transform.position = new Vector3(startPos + dist, transform.position.y, transform.position.z);

        // if (temp > startPos + length)
        //     startPos += length;
        // else if (temp < startPos - length)
        //     startPos -= length;




        float temp = transform.position.x * (1 - parallaxEffect);
        float dist = parallaxEffect * Time.deltaTime;

        startPos = transform.position.x;
        transform.position = new Vector3(startPos + dist, transform.position.y, transform.position.z);

        // if (temp > transform.position.x + length)
        if (transform.position.x > 7)//length || transform.position.x < -length)
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
    }
}
