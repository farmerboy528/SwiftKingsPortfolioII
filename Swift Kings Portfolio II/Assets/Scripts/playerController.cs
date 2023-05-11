using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class playerController : MonoBehaviour//, IDamage
{
    [Header("~~~~~~~Components~~~~~~~")]
    [SerializeField] CharacterController controller;
    [Header("\n~~~~~~~Stats~~~~~~~")]
    [Header("~~~Player~~~")]
    [SerializeField] int hp;
    [SerializeField] float speed;
    [SerializeField] float sprintMult;
    [SerializeField] float jumpHeight;
    [SerializeField] float gravity;
    [SerializeField] int jumps;
    [Header("\n~~~Weapon~~~")]
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;
    [SerializeField] int shootDamage;

    int jumped;
    Vector3 move;
    Vector3 velocity;
    bool isGrounded;
    bool isSprinting;
    bool isShooting;
    int hpOriginal;

    // Start is called before the first frame update
    void Start()
    {
        hpOriginal = hp; //Store original health for respawn system
    }

    // Update is called once per frame
    void Update()
    {
        Sprint(); //Find out if the player is sprinting
        Movement(); //Move player
        if (Input.GetButton("Shoot") && !isShooting) //If the player is pressing the shoot button and not already shooting
        {
//            StartCoroutine(Shoot());
        }
    }
    void Movement()
    {
        isGrounded = controller.isGrounded; //Check for grounded
        if (isGrounded && velocity.y < 0) //If grounded and experiencing gravity
        {
            velocity.y = 0f; //Reset vertical velocity
            jumped = 0; //Reset times jumped
        }
        move = (transform.right * Input.GetAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical"));
        controller.Move(move * Time.deltaTime * speed);

        //Jump functionality
        if (Input.GetButtonDown("Jump") && jumped < jumps) //If press jump and haven't jumped more than jumps
        {
            jumped++; //Jump
            velocity.y += jumpHeight; //Move up
        }

        //Gravity
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    void Sprint()
    {
        //If holding down sprint button
        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true; //Sprint
            speed *= sprintMult; //Apply speed multiplier
        }
        else if (Input.GetButtonUp("Sprint")) //Once sprint button is let go
        {
            isSprinting = false; //Stop sprinting
            speed /= sprintMult; //Unapply speed multiplier
        }
    }
/*    IEnumerator Shoot()
    {
        isShooting = true; //Shoot
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f,0.5f)),out hit, shootDist))
        {
            IDamage obj = hit.collider.GetComponent<IDamage>();
            if(obj != null)
            {
                obj.TakeDamage(shootDamage);
            }
        }
    }*/
    public void TakeDamage(int dmg)
    {
        hp -= dmg;//Subtract damage taken
        if(hp <= 0)//If hp is less than or = to 0
        {
//            gameManager.instance.YouLose(); //Lose the game
        }
    }
    public void SpawnPlayer()
    {
        controller.enabled = false; //Disable CharacterController to allow manual position setting
//      transform.position = gameManager.instance.playerSpawnPos.transform.position; //Set the position to where the player is supposed to spawn
        controller.enabled = true; //Reenable controller to allow for the movement functions to work
        hp = hpOriginal; //Reset the player's hp to the original amount
    }
    public void HealPlayer(int amount)
    {
        hp += amount; //Increase hp
    }
}