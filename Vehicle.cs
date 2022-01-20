using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Brad Hanel
 * IGME 202 01
 * Vehicle class - base class for humans and zombies that do many things like pursue or evade each other and their surroundings
 */ 
public abstract class Vehicle : MonoBehaviour
{
    //debug line materials
    public Material debug1;
    public Material debug2;
    public Material debug3;
    public Material debug4;
    public Material debug5;

    // Vectors necessary for force-based movement
    public Vector3 vehiclePosition;
    public Vector3 acceleration;
    public Vector3 direction;
    public Vector3 velocity;

    //forces acted upon vehicles
    protected Vector3 boundsForce;
    protected Vector3 seekingForce;
    protected Vector3 fleeingForce;
    protected Vector3[] avoidanceForces;
    protected List<GameObject> avoid = new List<GameObject>();
    protected Vector3[] separationForces;
    public Vector3 inputForce;

    //for force calculations
    protected Vector3 desiredPosition;

    //if the human is user-controlled
    public bool isPlayer;

    //closest enemy stored here
    protected GameObject closestEnemy;

    // Floats
    public float mass;
    public float maxSpeed;

    protected AgentManager am;

    

    public abstract void CalcSteeringForces();

    void Start()
    {
        am = GameObject.Find("Scene Manager").GetComponent<AgentManager>();
        vehiclePosition = transform.position;

        //set everything to zero
        fleeingForce = Vector3.zero;
        seekingForce = Vector3.zero;
        inputForce = Vector3.zero;
        velocity = new Vector3(0, 0, 0);
        acceleration = new Vector3(0, 0, 0);

        //gives every vehicle a random direction and a default desired position if wandering
        direction = new Vector3(Random.Range(-2.0f, 2.0f), 0, Random.Range(-2.0f, 2.0f));
        direction.Normalize();
        desiredPosition = transform.position + (direction * 3);
    }

    // Update is called once per frame
    void Update()
    { 
        //add acceleration to velocity and velocity to position as always
        velocity += acceleration * Time.deltaTime;
        vehiclePosition += velocity * Time.deltaTime;

        //the player's direction is whatever he wants
        if (!isPlayer)
        {
            direction = velocity.normalized;
        }
        
        //keep them from accelerating off the map
        acceleration = Vector3.zero;
        transform.position = vehiclePosition;

        //spin the 3d model
        transform.LookAt(transform.position + direction, Vector3.up);
        
        //different for human and zombie
        CalcSteeringForces();

    }

    // ApplyForce
    // Receive an incoming force, divide by mass, and apply to the cumulative accel vector
    public void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    //if vehicle is going out of bounds, push it back in
    public void StayInBounds()
    {
        if(this.transform.position.x >= 48)
        {
            boundsForce = (new Vector3(-10, 0, 0));
        }
        else if (this.transform.position.x <= 2)
        {
            boundsForce = (new Vector3(10, 0, 0));
        }
        else if (this.transform.position.z >= 73)
        {
            boundsForce = (new Vector3(0, 0, -10));
        }
        else if (this.transform.position.z <= 2)
        {
            boundsForce = (new Vector3(0, 0, 10));
        }
        else
        {
            boundsForce = Vector3.zero;
            if (isPlayer)
            {
                velocity = Vector3.zero;
            }
        }
        //this shouldn't happen anymore, but just in case they try to fly...
        if(transform.position.y < 1 || transform.position.y > 1)
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            Debug.Log("get back on the ground");
        }
 
        
    }

    //applies pursuit force to local seeking force variable
    public void Pursuit(Vector3 targetPosition)
    {
        // Step 1: Find DV (desired velocity)
        // TargetPos - CurrentPos
        Vector3 desiredVelocity = targetPosition - vehiclePosition;

        // Step 2: Scale vel to max speed
        // desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        // Step 3:  Calculate seeking steering force
        seekingForce = desiredVelocity - velocity;

    }

    //applies evasion force to vehicle
    public void Evasion(Vector3 targetPosition)
    {
        // Step 1: Find DV (desired velocity)
        // TargetPos - CurrentPos
        Vector3 desiredVelocity = -(targetPosition - vehiclePosition);

        // Step 2: Scale vel to max speed
        
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        // Step 3:  Calculate seeking steering force
        fleeingForce = desiredVelocity - velocity;

    }

    //returns closest enemy and stores it in local closest enemy object
    public GameObject FindClosestEnemy(List<GameObject> enemies)
    {
        closestEnemy = enemies[0];
        for (int i = 0; i < enemies.Count; i++)
        {
            if (Vector3.Distance(closestEnemy.transform.position, transform.position) > Vector3.Distance(enemies[i].transform.position, transform.position))
            {
                closestEnemy = enemies[i];
            }
        }
        return closestEnemy;
    }

    //for avoiding obstacles, imperfect but they tend to steer around them
    public void ObstacleAvoidance(List<GameObject> obstacles)
    {

        avoidanceForces = new Vector3[20];

        
        float dotProd = 0;

        //kick off avoid list if no longer relevant
        avoid.Clear();

        for (int i = 0; i < obstacles.Count; i++)
        {
            //vector between object and obstacle
            Vector3 obsDirection = (obstacles[i].transform.position - transform.position);

            //dot product between that and right vector of object
            dotProd = Vector3.Dot(obsDirection, Quaternion.Euler(0, 90, 0) * direction);

            //negative if obstacle behind object
            float dotProdBehind = Vector3.Dot(obsDirection, direction);

            //distance between object and obstacle
            float distanceApart = Vector3.Distance(obstacles[i].transform.position, transform.position);

            //sum of radii of two objects
            float sumOfRad = obstacles[i].GetComponent<Obstacle>().GetRadius() + GetComponent<BoxCollider>().bounds.size.x / 2;

            

            //if object is in front, it is not too far to the right or left, and it's within ten units, avoid it
            if (dotProdBehind > 0 && Mathf.Abs(dotProd) < sumOfRad && distanceApart < 5)
            {
                Vector3 desiredVelocity;
                //object is to the right
                if (dotProd > 0)
                {
                    //move left
                    desiredVelocity = -(Quaternion.Euler(0, 90, 0) * direction);
                }
                else
                {
                    //move right
                    desiredVelocity = (Quaternion.Euler(0, 90, 0) * direction);
                }

                //Scale vel to max speed
                desiredVelocity.Normalize();
                desiredVelocity = desiredVelocity * maxSpeed;


                //Calculate seeking avoidance force
                avoidanceForces[i] = (desiredVelocity - velocity);
                //avoidanceForces.RemoveAt(avoidanceForces.Count - 1);

                avoid.Add(obstacles[i]);
            }
            //if it's not, don't avoid it
            else
            {
                avoidanceForces[i] = Vector3.zero;
                if (avoid.Contains(obstacles[i]))
                {
                    avoid.Remove(obstacles[i]);
                }
                
            }
        }
        
    }
    //keeps allies away from each other
    public void Separate(List<GameObject> allies)
    {

        separationForces = new Vector3[allies.Count];
        for (int i = 0; i < allies.Count; i++)
        {
            if (Vector3.Distance(allies[i].transform.position, transform.position) < 3 && allies[i].gameObject != gameObject)
            {
                // Step 1: Find DV (desired velocity)
                // TargetPos - CurrentPos
                Vector3 desiredVelocity = -(allies[i].transform.position - vehiclePosition);

                // Step 2: Scale vel to max speed

                desiredVelocity.Normalize();
                desiredVelocity = desiredVelocity * maxSpeed;

                // Step 3:  Calculate seeking steering force
                separationForces[i] = desiredVelocity - velocity;
            }
            else
            {
                separationForces[i] = Vector3.zero;
            }
        }
    }

    //for aimlessly wandering when there's not force on them
    public void Wander()
    {
        //just in case their desired position is impossible to reach
        if (desiredPosition.x >= 48 || desiredPosition.x <= 2 || desiredPosition.z >= 73 || desiredPosition.z <= 2)
        {
            direction = new Vector3(Random.Range(-2.0f, 0.5f), 0, Random.Range(-2.0f, 0.5f));
            //transform.Rotate(direction);
            desiredPosition = transform.position + (direction * 5);
        }
        if (desiredPosition.x - transform.position.x <= 1 && desiredPosition.z - transform.position.z <= 1)
        {
            //get them a new desired position
            Vector3 randomUnit = new Vector3(Random.Range(-2.0f, 0.5f), 0, Random.Range(-2.0f, 0.5f));
            randomUnit.Normalize();
            desiredPosition = transform.position + (direction * 5) + randomUnit;
        }

        // Step 1: Find DV (desired velocity)
        // TargetPos - CurrentPos
        Vector3 desiredVelocity = desiredPosition - vehiclePosition;

        // Step 2: Scale vel to max speed
        // desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        // Step 3:  Calculate seeking steering force
        seekingForce = desiredVelocity - velocity;

    }

    void OnRenderObject()

    {
        if (am.debugging)
        {
            // Set the material to be used for the first line

            debug1.SetPass(0);
            // Draws forward line

            GL.Begin(GL.LINES);

            GL.Vertex(transform.position);

            GL.Vertex(transform.position + direction * 3);

            GL.End();

            // Draws enemy detect line, if there's a human to detect


            if (closestEnemy != null && this.GetComponent<Zombie>())
            {
                debug2.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Vertex(transform.position);
                GL.Vertex(closestEnemy.transform.position);
                GL.End();
            }

            debug3.SetPass(0);


            // Draws right line

            GL.Begin(GL.LINES);

            GL.Vertex(transform.position);

            GL.Vertex(transform.position + Quaternion.Euler(0, 90, 0) * direction * 3);


            GL.End();

            //if human
            if (this.GetComponent<Human>())
            {
                debug4.SetPass(0);
            }
            //if zombie
            else
            {
                debug5.SetPass(0);
            }

            GL.Begin(GL.LINES);

            GL.Vertex(transform.position);
            Vector3 tempVel = velocity;
            tempVel.Normalize();
            GL.Vertex(transform.position + tempVel * 3);

            GL.End();





        }
    }

}
