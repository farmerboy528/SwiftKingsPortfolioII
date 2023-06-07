using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class necromancerAI : MonoBehaviour,IDamage,IPhysics
{
    [Header("-----Components------")]
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] Transform minionSpawnPoint;//where minions spawn from
    [SerializeField] Transform headPos;
    [SerializeField] Transform shootPos;
   
    [Header("\n----Enemy Stats------")]
    [Range(1, 100)] [SerializeField] int hp;
    [SerializeField] int turnSpeed;
    [SerializeField] int shootAngle;
    [SerializeField]  float viewCone;
    [SerializeField] float animTranSpeed;
    [SerializeField] int roamDistance;
    [SerializeField] int roamPauseTime;
    [SerializeField] float retreatTime;//how long enemies will retreat for
    [SerializeField] int runAwayDistance;//how far the enmemy will runaway
    [Header("\n-----Enemy Weapon------")]
    
    [SerializeField] GameObject minions;
    [SerializeField] float minionSpawnRate;
    [SerializeField] GameObject projectile;
    [Range(.1f, 3)] [SerializeField] float fireRate;
    
    
    bool isShooting;
    bool isSpawning;
    bool isRetreating;
    bool playerInRange;
 
    float angleToPlayer;
    Vector3 playerDir;
    Vector3 playerFutureDir;
    Vector3 startingPos;
    Color colorOrig;
    bool destinationChosen;
    float stoppingDistOrig;
    float speed;
    float retreatDistance;
    void Start()
    {
        colorOrig = model.material.color;
        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;
        retreatDistance = stoppingDistOrig - 3;
    }

    // Update is called once per frame
    IEnumerator spawnMinions()
    {if (gameManager.instance.numberOfMinions < gameManager.instance.maxNumberOfMinions)
        {
            isSpawning = true;
            Instantiate(minions, minionSpawnPoint.position, transform.rotation);
            yield return new WaitForSeconds(minionSpawnRate);
            isSpawning = false;
        }
        
    }
  
    void Update()
    {
        if (agent.isActiveAndEnabled)
        {
            speed = Mathf.Lerp(speed, agent.velocity.normalized.magnitude, Time.deltaTime * animTranSpeed);
            anim.SetFloat("Speed", speed);
            if (playerInRange && !CanSeePlayer())
            {

                StartCoroutine(Roam());

            }
            else if (agent.destination != gameManager.instance.pScript.futurePos.transform.position)
            {
                StartCoroutine(Roam());
            }
        }
    }
    bool CanSeePlayer()
    {

        playerDir = gameManager.instance.player.transform.position - headPos.position;
        playerFutureDir = gameManager.instance.pScript.futurePos.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(new Vector3(playerDir.x, 0, playerDir.z), transform.forward);
        Debug.DrawRay(headPos.position, playerDir);
        // Debug.Log(angleToPlayer);
        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewCone)
            {
                if (!isRetreating)//makes sure enemy isn't retreating
                {
                    if (agent.remainingDistance <= retreatDistance) //checks to see if agent needs to retreating
                    {
                        StartCoroutine(Retreat(transform.position - (playerDir.normalized * runAwayDistance), retreatTime));//starts retreating away from player = to retreat distance for however long it's scared
                    }
                    else
                    {
                        agent.stoppingDistance = stoppingDistOrig;
                        agent.SetDestination(gameManager.instance.pScript.futurePos.transform.position);
                    }
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        FacePlayer();
                    }
                    if (!isShooting)
                    {
                        StartCoroutine(shoot());
                    }
                    if (!isSpawning)
                    {
                        StartCoroutine(spawnMinions());
                    }
                }
                return true;
            }
        }
        agent.stoppingDistance = 0;

        return false;

    }
    //bool CanSeePlayer()
    //{
    //    playerDir = gameManager.instance.player.transform.position - headPos.position;
    //    angleToPlayer = Vector3.Angle(transform.forward, playerDir);
    //    Debug.DrawRay(headPos.position, playerDir);
    //    //Debug.Log(angleToPlayer);
    //    RaycastHit hit;
    //    if (Physics.Raycast(headPos.position, playerDir, out hit))
    //    {
    //        if (hit.collider.CompareTag("Player") && angleToPlayer <= viewCone)
    //        {
    //            agent.stoppingDistance = stoppingDistOrig;
    //            agent.SetDestination(gameManager.instance.player.transform.position);
    //            destinationChosen = true;
    //            if (agent.remainingDistance <= agent.stoppingDistance)
    //            {
    //                FacePlayer();
    //            }
    //            if(!isSpawning)
    //            {
    //                StartCoroutine(spawnMinions());
    //            }
    //            
    //            if (!isShooting && angleToPlayer <= shootAngle)
    //            {   
    //                StartCoroutine(shoot());
    //            }
    //            return true;
    //        }
    //        else
    //        {
    //            agent.stoppingDistance = 0;
    //        }
    //    }
    //
    //
    //
    //    return false;
    //}
    IEnumerator DamageColor()//enemy blinks red when they take damage
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(.1f);
        model.material.color = colorOrig;
    }
    IEnumerator shoot()
    {
        isShooting = true;
        anim.SetTrigger("Shoot");

        yield return new WaitForSeconds(fireRate);
        isShooting = false;
    }
 

    public void CreateBullet()
    {
        Instantiate(projectile, shootPos.position, transform.rotation);
    }
    void FacePlayer()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerFutureDir.x, 0, playerFutureDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
    }
    public void TakeDamage(int dmg)
    {
        hp -= dmg;

        if (hp <= 0)
        {

            gameManager.instance.UpdateGameGoal(-1);
            anim.SetBool("Dead", true);
            agent.enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
            StopAllCoroutines();
            Destroy(gameObject, 30);
            gameManager.instance.currentScore++;
        }
        else
        {
            anim.SetTrigger("Damage");
           agent.SetDestination(gameManager.instance.player.transform.position);
            playerInRange = true;
            StartCoroutine(DamageColor());
        }

    }
    public void TakePushBack(Vector3 dir)
    {
        agent.velocity += dir;//enemy gets pushed by our shots
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            agent.stoppingDistance = stoppingDistOrig;
            playerInRange = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            agent.stoppingDistance = 0;
        }
    }
    IEnumerator Roam()
    {
        if (!destinationChosen && agent.remainingDistance < 0.05f)
        {
            destinationChosen = true;
            agent.stoppingDistance = 0;
            yield return new WaitForSeconds(roamPauseTime);
            destinationChosen = false;

            Vector3 ranPos = Random.insideUnitSphere * roamDistance;
            ranPos += startingPos;

            NavMeshHit hit;
            NavMesh.SamplePosition(ranPos, out hit, roamDistance, 1);

            agent.SetDestination(hit.position);
        }
    }
    IEnumerator Retreat(Vector3 retreatPos, float retreatTime)//takes in the position to retreat to and for how long
    {
        isRetreating = true;//sets retreating to true
        agent.stoppingDistance = 0;
        agent.SetDestination(retreatPos);//Sets agent position to the desired retreat location

        yield return new WaitForSeconds(retreatTime);//how long the enemy will continue retreating for

        agent.stoppingDistance = stoppingDistOrig;
        agent.SetDestination(gameManager.instance.pScript.transform.position);
        isRetreating = false;//stops the retreat
    }

}
