using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Brad Hanel
 * IGME 202 01
 * Zombie class - child class for zombie, they attack humans and are dumb
 */

public class Zombie : Vehicle
{

    // Use this for initialization



    public override void CalcSteeringForces()
    {
        //ultimate force vector, affected by every possible force
        Vector3 ult = Vector3.zero;
        //seeking is a little stronger for zombies because they are ruthless and stop at nothing
        ult += seekingForce * 2;
        ult += boundsForce;
        for(int j = 0; j < avoidanceForces.Length; j++)
        {
            ult += avoidanceForces[j] * 2;
        }
        for (int j = 0; j < separationForces.Length; j++)
        {
            ult += separationForces[j] * 2;
        }
        if (ult == Vector3.zero)
        {
            Wander();
            ult += seekingForce * 2;
        }

        //keeps forces from getting too high
        Mathf.Clamp(ult.magnitude, 0, 2);

        //don't go up or down
        ult.y = 0;
        ApplyForce(ult);
    }

    
 

    //when no more humans, stop
    public void StopSeeking()
    {
        seekingForce = Vector3.zero;
 
    }
}
