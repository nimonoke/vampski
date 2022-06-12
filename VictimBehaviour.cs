using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictimBehaviour : MonoBehaviour
{
    [SerializeField] private float victimMoveSpeed = 1.5f;
    [SerializeField] private float victimHealth = 100f;
    [SerializeField] private float timeForVictimDespawning = 3f;
    [SerializeField] private float victimMinDrainSpeed = 15f;
    [SerializeField] private float victimMaxDrainSpeed = 45f;
    [SerializeField] private float victimCurrentMoveSpeed;
    private float victimDrainSpeed;
    private float victimHealthBarPercentage = 1f;
    
    private bool movingLeft;
    private bool wasDrained;
    private bool isGrounded;
    private bool isKilled;

    [SerializeField] private GameObject victimHealthObject;  // Parent object that will be disabled/enabled when the draining is active/inactive
    [SerializeField] private GameObject victimHealthBarObject;  // Child object that is the health BAR, which will be controlled (increased/decreased)
    [SerializeField] private GameObject victimSprite;
    public GameObject playerObject;

    private PlayerBehaviour playerBehaviour;
    private PlayerDrainScript playerDrainScript;

    private Rigidbody2D rb2D;
    private Animator anim;
    private Transform victimHealthBar;  // Transform component of the victimHealthBarObject that will be resized



    private void Awake()
    {
        victimDrainSpeed = Random.Range(victimMinDrainSpeed, victimMaxDrainSpeed);  // Illusion of each victim having different amount of HP (they just drain faster/slower)
        victimCurrentMoveSpeed = victimMoveSpeed;
        wasDrained = false;
        
        playerObject = GameObject.Find("Player");
        playerBehaviour = playerObject.GetComponent<PlayerBehaviour>();
        playerDrainScript = playerObject.GetComponent<PlayerDrainScript>();
        
        rb2D = gameObject.GetComponent<Rigidbody2D>();
        anim = victimSprite.GetComponent<Animator>();
        victimHealthBar = victimHealthBarObject.GetComponent<Transform>();

        victimHealthObject.SetActive(false);
        victimHealthBar.localScale = new Vector3(victimHealthBarPercentage, 1f, 1f);  // Set the HP bar to max

        VictimRandomMove();
        anim.SetBool("isRunning", true);
    }

    private void Update()
    {
        VictimGameOverCheck();

        Physics2D.IgnoreLayerCollision(7, 6); // Prevent the victims from colliding with the player
        Physics2D.IgnoreLayerCollision(7, 7);  // Prevent the victims from colliding with the other victims
        Physics2D.IgnoreLayerCollision(7, 8);  // Prevent the victims from colliding with the cops
    }

    private void FixedUpdate()
    {
        rb2D.AddForce(new Vector2(victimMoveSpeed, 0f), ForceMode2D.Impulse);  // Moves victim right away in a random direction (based on victimMoveSpeed)
    }

    private void OnCollisionEnter2D(Collision2D other)  // Removes the victims gameObjects if they each the end to reduce clutter (***Change this to similar method as the cops - relative to the victim/player distance***)
    {
        if (other.gameObject.tag == "Destroyer")
            Destroy(this.gameObject);
    }




    // Methods

        // Misc Methods
    private void VictimDrain()
    {
        Vector3 temp = new Vector3(playerObject.transform.position.x , gameObject.transform.position.y, 0f);  
        gameObject.transform.position = temp;  // When the player is draining, set the victim to the same spot as the player (fixing the collider bug)

        VictimHealthBarBehaviour();

        anim.SetBool("isBeingDrained", true);
        anim.SetBool("isRunning", false);

        victimMoveSpeed = 0f;
        wasDrained = true;

        victimHealth -= (victimDrainSpeed * Time.deltaTime) * 1.5f;
            if (victimHealth <= 0)
                victimHealth = 0;
    }

    private void VictimHealthBarBehaviour()
    {
        victimHealthObject.SetActive(true);  // Show victim's health bar
        victimHealthBarPercentage = victimHealth/100;  // Calculate % of the victim's health 
        victimHealthBar.localScale = new Vector3(victimHealthBarPercentage, 1f, 1f);  // Reduce the health bar UI
    }

    private void VictimGameOverCheck()
    {
        if (playerDrainScript.PlayerIsBusted)
        {
            victimMoveSpeed = 0f;

            anim.SetBool("isBeingDrained", false);
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);

            anim.SetBool("gameOver", true);

            victimHealthObject.SetActive(false);
        }
    }

    private void VictimStopAnim()
    {
        victimHealthObject.SetActive(false);  // Disable the victim's health bar whenever the victim isn't being drained
        anim.SetBool("isBeingDrained", false);
    }

        // Victim Movement Methods
    private void VictimRandomMove()
    {
        int randomNumber = Random.Range(1, 3);

        if (randomNumber == 1) VictimMoveLeft();
        if (randomNumber == 2) VictimMoveRight();
    }

    private void VictimMoveLeft()
    {
        gameObject.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);

        victimMoveSpeed = -victimCurrentMoveSpeed;
        movingLeft = true;
    }

    private void VictimMoveRight()
    {
        gameObject.transform.localScale = new Vector3(-1.4f, 1.4f, 1.4f);
        victimHealthObject.transform.localScale = new Vector3(-0.4f, 0.4f, 0.4f);  // Flip the sprite

        victimMoveSpeed = victimCurrentMoveSpeed;
        movingLeft = false;
    }

    private void VictimRunAway()
    {
        VictimRandomMove();
        if (movingLeft) victimMoveSpeed = -3f;
        if (!movingLeft) victimMoveSpeed = 3f;

        anim.SetBool("isBeingDrained", false);
        anim.SetBool("isRunning", true);

        this.gameObject.GetComponent<BoxCollider2D>().enabled = false;

        victimHealthObject.SetActive(false); // Disable the victim's health bar whenever the victim dies

        Invoke("DeleteVictim", timeForVictimDespawning);
    }

    // Victim Death Methods
    private void VictimDeathCheck()
    {
        if (victimHealth == 0)
        {
            VictimDeath();
            isKilled = true;
            // Add ghost behaviour here?? Or just instantiate it and have a seperate script for the ghost prefab
        }
    }


    private void VictimDeath()
    {
        playerDrainScript.PlayerDrainStop();

        anim.SetBool("isDead", true);  // Play the victim death animation

        // Disable colliders and rigidbody for the victim (so it's not interactive anymore)
        this.GetComponent<BoxCollider2D>().enabled = false;
        victimSprite.GetComponent<BoxCollider2D>().enabled = false;
        Destroy(this.GetComponent<Rigidbody2D>());
        Destroy(victimSprite.GetComponent<Rigidbody2D>());

        victimHealthObject.SetActive(false); // Disable the victim's health bar whenever the victim dies

        Invoke("DeleteVictim", timeForVictimDespawning);  // Delete the victim object completely after 3 secs (*** Maybe also set it to delete once it's out of screen***)

        this.enabled = false;  // Disable this script
    }

    private void DeleteVictim()
    {
        Destroy(gameObject);
    }



    // Getters & Setters

        // Methods
    public void GetVictimDrain() { VictimDrain(); }
    public void GetVictimDeathCheck() { VictimDeathCheck(); }
    public void GetVictimStopAnim() { VictimStopAnim(); }
    public void GetVictimRunAway() { VictimRunAway(); }

        // Variables
    public float VictimMoveSpeed
    {
        get { return victimMoveSpeed; }
        set { victimMoveSpeed = value; }
    }

    public bool WasDrained
    {
        get { return wasDrained; }
    }

    public bool MovingLeft
    {
        get { return movingLeft; }
    }
    
    public bool IsKilled
    {
        get { return isKilled; }
    }
}
