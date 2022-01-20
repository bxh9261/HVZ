using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Brad Hanel
 * IGME 202 01
 * Puck class - doesn't do much but it gives the obstacle a radius
 */
public class Obstacle : MonoBehaviour
{
    float radius;
    // Start is called before the first frame update
    void Start()
    {
        radius = GetComponent<CapsuleCollider>().radius;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetRadius()
    {
        return radius;
    }
}
