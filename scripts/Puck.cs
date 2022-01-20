using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Brad Hanel
 * IGME 202 01
 * Puck class - doesn't do much but it lets the puck move
 */
public class Puck : MonoBehaviour
{

    public Vector3 velocity;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }

}
