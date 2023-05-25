using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class minionAI : MonoBehaviour,IDamage
    
{   [Header("~~~~~~minion components~~~~~~~~")]
    [SerializeField] GameObject projectile;
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] GameObject particleExplosion;
    [Header("\n~~~~~Minion Stats~~~~~")]
    [Range(1,100)] [SerializeField] int hp;
    [SerializeField] int turnSpeed;
    [Range(.1f, 3f)] [SerializeField] float fireRate;
    [SerializeField] float animTranSpeed;
    bool isShooting;
    bool playerInRange;
    Vector3 playerDir;
    Color colorOrig;
    float speed;
    // Start is called before the first frame update
    void Start()
    {
        colorOrig = model.material.color;
        gameManager.instance.UpdateMinionsCounter(+1);
        
    }
    
    // Update is called once per frame
    void Update()
    {
        playerDir = gameManager.instance.player.transform.position - transform.position;
        agent.SetDestination(gameManager.instance.player.transform.position);
        speed = Mathf.Lerp(speed, agent.velocity.normalized.magnitude, Time.deltaTime * animTranSpeed);
        anim.SetFloat("Speed", speed);
        FacePlayer();
        if (!isShooting)
        {
            
            StartCoroutine(Shoot());
        }    
    }
    public void TakeDamage(int dmg)
    {
        hp -= dmg;
       StartCoroutine( DamageColor());
        if (hp <= 0)
        {
            gameManager.instance.UpdateMinionsCounter(-1);
            Instantiate(particleExplosion,transform.position,transform.rotation);
            Destroy(gameObject);
        }
        else
        {
            anim.SetTrigger("Damage");
            agent.SetDestination(gameManager.instance.player.transform.position);
            
            StartCoroutine(DamageColor());
        }

    }
    IEnumerator Shoot()
    {

        isShooting = true;
        anim.SetTrigger("Shoot");

        yield return new WaitForSeconds(fireRate);
        isShooting = false;
    }
    public void CreateBullet()
    {
        Instantiate(projectile, transform.position, transform.rotation);
    }
    void FacePlayer()
    {

        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
    }
    IEnumerator DamageColor()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(.1f);
        model.material.color = colorOrig;
    }
   

}
