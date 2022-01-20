using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Brad Hanel
 * IGME 202 01
 * Agent Manager - scene manager for all things HVZ
 */
public class AgentManager : MonoBehaviour
{
    //all the gameobjects and models
    public GameObject zCube;
    public GameObject humanCube;
    public GameObject treeCube;
    public GameObject shotCube;
    public GameObject pCube;
    public List<GameObject> zombies = new List<GameObject>();
    public List<GameObject> humans = new List<GameObject>();
    public List<GameObject> obs = new List<GameObject>();
    public List<GameObject> bullets = new List<GameObject>();
    GameObject player;

    //bools for game mode
    public bool inGame = false;
    bool canShoot = true;

    //other stuff for game mode
    float shotRotation;
    Vector3 shotVelocity;

    //zombie and human's attached scripts
    Zombie zombieScript;
    Human humanScript;

    //random spawn position
    Vector3 spawnPosition;

    //for debug mode
    public bool debugging;

    SmoothFollow sf;

    //plays sounds
    AudioSource[] aud;

    // Start is called before the first frame update
    void Start()
    {
        aud = GetComponents<AudioSource>();
        sf = GameObject.Find("Main Camera").GetComponent<SmoothFollow>();
        debugging = true;

        for(int i = 0; i < 1; i++)
        {
            //spawn position for zombie
            spawnPosition = new Vector3(Random.Range(2,48), 1, Random.Range(2, 73));
            //make zombie
            zombies.Add(Instantiate(zCube, spawnPosition, Quaternion.Euler(270, 0, 180)));
        }
        for (int i = 0; i < 10; i++)
        {
            //spawn position for human
            spawnPosition = new Vector3(Random.Range(2, 48), 1, Random.Range(2, 73));
            //make human
            humans.Add(Instantiate(humanCube, spawnPosition, Quaternion.Euler(270, 0, 180)));
        }
        for (int i = 0; i < 20; i++)
        {
            //spawn position for obstacle
            spawnPosition = new Vector3(Random.Range(2, 48), 1, Random.Range(2, 73));
            //make obstacle
            obs.Add(Instantiate(treeCube, spawnPosition, Quaternion.identity));
        }


    }

    // Update is called once per frame
    void Update()
    {
        //loops through zombies
        foreach(GameObject zombie in zombies)
        {
            zombieScript = zombie.GetComponent<Zombie>();
            //will find closest human, unless there are none, if so, stop
            if(humans.Count > 0)
            {
                GameObject closestHuman = zombieScript.FindClosestEnemy(humans);
                Vector3 evasionPoint = closestHuman.GetComponent<Human>().velocity;
                evasionPoint.Normalize();
                zombieScript.Pursuit(closestHuman.transform.position + (evasionPoint * 3));
            }
            else
            {
                zombieScript.StopSeeking();
            }

            //keeps zombie in bounds
            zombieScript.StayInBounds();

            zombieScript.ObstacleAvoidance(obs);

            zombieScript.Separate(zombies);
        }

        //loops through humans
        foreach (GameObject human in humans)
        {
            humanScript = human.GetComponent<Human>();
            //runs away if within 10 of a zombie
            if (zombies.Count > 0)
            {
                GameObject closest = humanScript.FindClosestEnemy(zombies);
                //they actually seek when the zombie is too close, because otherwise they end up running back to the zombie before dying, as if accepting their fate
                if (Vector3.Distance(closest.transform.position, human.transform.position) < 4)
                {
                    humanScript.Evasion(closest.transform.position);
                }
                else if (Vector3.Distance(closest.transform.position, human.transform.position) < 10)
                {
                    Vector3 evasionPoint = closest.GetComponent<Zombie>().velocity;
                    evasionPoint.Normalize();
                    humanScript.Evasion(closest.transform.position + (evasionPoint * 3));
                }
                else
                {
                    humanScript.StopFleeing();
                }
            }
            else
            {
                humanScript.StopFleeing();
            }
            //keeps human in bounds
            humanScript.StayInBounds();

            humanScript.ObstacleAvoidance(obs);

            humanScript.Separate(humans);
        }

        //if zombie and human collide, destroy human, create zombie in its place
        for(int i = 0; i < zombies.Count; i++)
        {
            for(int j = 0; j < humans.Count; j++)
            {
                if(CircleCollision(zombies[i], humans[j]))
                {
                    zombies.Add(Instantiate(zCube, humans[j].transform.position, Quaternion.Euler(270, 0, 180)));
                    Destroy(humans[j]);
                    //return to main cam if player dies
                    if(inGame && humans[j] == player)
                    {
                        GameObject cam = GameObject.Find("Main Camera");
                        cam.transform.position = new Vector3(26.7f, 62.2f, 6.1f);
                        cam.transform.rotation = Quaternion.Euler(67.065f, 2.162f, 1.927f);
                        inGame = false;
                    }
                    //"oh no"
                    aud[3].Play(0);
                    humans.RemoveAt(j);
                }
            }
            //if bullet hits zombie, kill zombie and bullet 
            for (int j = 0; j < bullets.Count; j++)
            {
                if (CircleCollision(zombies[i], bullets[j]))
                {
                    //"goal!"
                    aud[2].Play(0);

                    //kill zombie
                    Destroy(zombies[i]);
                    zombies.RemoveAt(i);

                    //kill puck
                    Destroy(bullets[j]);
                    bullets.RemoveAt(j);
                }
            }
        }
        //for debug mode
        if (Input.GetKeyDown(KeyCode.D))
        {
            debugging = !debugging;
        }

        //create bullet if user hits space
        SpawnBullets();

        //if in game, player can move around using arrow keys
        if (inGame)
        {
            Human playerScript = player.GetComponent<Human>();
            Vector3 playerpos = playerScript.vehiclePosition;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                playerScript.vehiclePosition = playerpos + playerScript.direction / 10.0f;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                playerScript.vehiclePosition = playerpos - playerScript.direction / 10.0f;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                playerScript.direction = Quaternion.Euler(0, -1, 0) * playerScript.direction;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                playerScript.direction = Quaternion.Euler(0, 1, 0) * playerScript.direction;
            }
        }

    }

    //detects if objects are colliding
    public bool CircleCollision(GameObject zombie, GameObject human)
    {

        //radius of zombie
        float zRadius = (zombie.GetComponent<BoxCollider>().bounds.size.x) / 2;

        //radius of human
        float hRadius = (human.GetComponent<BoxCollider>().bounds.size.x) / 2;

        //distance between the two
        float dist = Vector3.Distance(zombie.transform.position, human.transform.position);

        //if the distance is less than the addition of the radii of the two objects, they must be touching
        if (dist < (zRadius + hRadius))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "Spawn Zombie"))
        {
            //max 30, I don't wanna crash it
            if (!(zombies.Count + humans.Count > 30))
            {
                //spawn position for zombie
                spawnPosition = new Vector3(Random.Range(2, 48), 1, Random.Range(2, 73));
                //make zombie
                zombies.Add(Instantiate(zCube, spawnPosition, Quaternion.Euler(270, 0, 180)));
                aud[1].Play(0);
            }

            
        }
        //make a new human
        if (GUI.Button(new Rect(10, 60, 200, 50), "Spawn Human"))
        {
            //max 30, I don't wanna crash it
            if (!(zombies.Count + humans.Count > 30))
            {
                //spawn position for zombie
                spawnPosition = new Vector3(Random.Range(2, 48), 1, Random.Range(2, 73));
                //make zombie
                humans.Add(Instantiate(humanCube, spawnPosition, Quaternion.Euler(270, 0, 180)));
                aud[4].Play(0);
            }
        }
        //turn a zombie back into a human
        if (GUI.Button(new Rect(10, 110, 200, 50), "Heal Human"))
        {
            if(zombies.Count > 0)
            {
                Vector3 zpos = zombies[0].transform.position;
                Destroy(zombies[0]);
                zombies.RemoveAt(0);

                humans.Add(Instantiate(humanCube, zpos, Quaternion.Euler(270, 0, 180)));
                aud[6].Play(0);
            }
        }
        //starts the simulation over
        if(GUI.Button (new Rect(10, 160, 200, 50), "Restart")){

            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); // loads current scene
        }
        //starts game mode
        if (GUI.Button(new Rect(10, 210, 200, 50), "Start Game Mode"))
        {
            if (!inGame)
            {
                aud[7].Play(0);
                inGame = true;
                //spawn position for zombie
                spawnPosition = new Vector3(Random.Range(2, 48), 1, Random.Range(2, 73));
                player = Instantiate(pCube, spawnPosition, Quaternion.Euler(270, 0, 180));
                //make zombie
                humans.Add(player);
                player.GetComponent<Human>().isPlayer = true;
                sf.target = player.transform;
                /*
                 * Default cam position:
                 * 26.7 , 62.2 , 6.1
                 * 67.065 , 2.162 , 1.927
                 */
            }
        }
    }

    //spawns bullets when player shoots
    void SpawnBullets()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (inGame && canShoot)
            {
                //pew
                aud[0].Play(0);

                //make bullet
                shotRotation = player.transform.eulerAngles.z;
                GameObject bullet = Instantiate(shotCube, new Vector3(player.transform.position.x, player.transform.position.y + 1, player.transform.position.z), Quaternion.Euler(0, 0, shotRotation));


                //MATH
                shotVelocity = player.GetComponent<Human>().direction * 20;
                bullet.GetComponent<Puck>().velocity = shotVelocity;
                Debug.Log(bullet.GetComponent<Puck>().velocity);

                bullets.Add(bullet);
                //timing mechanism
                canShoot = false;
                StartCoroutine(KillBullets(bullet));
                
            }

        }


    }

    //timing mechanism
    public IEnumerator KillBullets(GameObject bullet)
    {
        yield return new WaitForSeconds(2.0f);
        Destroy(bullet);
        bullets.Remove(bullet);
        canShoot = true;
    }
}
