using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Brad Hanel
 * IGME 202 01
 * Human class - child class for humans, they avoid zombies
 */
public class Human : Vehicle
{
  
    Vector3 closestZombie;
    
    
    //set fleeing force to zero
    public void StopFleeing()
    {
        fleeingForce = Vector3.zero;
        //velocity = Vector3.zero;
    }



    public override void CalcSteeringForces()
    {
       
        Vector3 ult = Vector3.zero;
        //avoid obstacles
        for (int j = 0; j < avoidanceForces.Length; j++)
        {
            ult += avoidanceForces[j];
        }
        //avoid other humans
        for (int j = 0; j < separationForces.Length; j++)
        {
            ult += separationForces[j];
        }
        //human player can kinda do whatever he dang well pleases
        if (!isPlayer)
        {
            //number one priority is avoiding zombies
            ult += fleeingForce * 2;
            if (ult == Vector3.zero)
            {
                Wander();
                ult += seekingForce;
            }

        }
        ult += boundsForce;
        //don't push too hard
        Mathf.Clamp(ult.magnitude, 0, 5);
        //don't go up or down
        ult.y = 0;
        ApplyForce(ult);

    }
        
}
