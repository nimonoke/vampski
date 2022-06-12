using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] private float maxMoveSpeed = 3f;
    [SerializeField] private float speedUpdatePercentage = 70f;
    [SerializeField] private float ghostMinDuration = 2f;
    [SerializeField] private float ghostMaxDuration = 6f;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float confusionSpriteSpeed = 2f;  // How fast the confusion sprite rotates
    
    private float ghostDuration;  // How long the player's controls are inverted
    private float moveHorizontal;

    private bool invertedControls = false;
    private bool isEnabled = true;  // Bool for the entire script, used in if statements so everything can be properly disabled if the player is fucked

    [SerializeField] private GameObject confusion;  // Placeholder sprite for when the controls are inverted
    [SerializeField] private GameObject gameOverText;

    private PlayerDrainScript playerDrain;

    private Rigidbody2D rb2D;
    private Animator anim;



    private void Awake()
    {
        rb2D = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();
        playerDrain = gameObject.GetComponent<PlayerDrainScript>();
    }

    private void Update()
    {
        confusion.transform.Rotate(confusionSpriteSpeed, 0f, 0f);  // Rotate the confusion sprite

        PlayerChangeSpeed();  // Constantly update the player's speed based on the missing health
        PlayerMovement();  // Check for movement inputs
        PlayerReduceHealth();  // Constantly remove player's health

        playerDrain.PlayerLosingHPValue += playerDrain.PlayerLosingHPSpeed * Time.deltaTime; // Increases the speed by which the player loses health

    }

    private void FixedUpdate()
    {
        if (isEnabled && !invertedControls)
            rb2D.AddForce(new Vector2(moveHorizontal * moveSpeed, 0f), ForceMode2D.Impulse); // Move the player according to the input which is included in the moveHorizontal variable
        if (isEnabled && invertedControls)
            rb2D.AddForce(new Vector2(-moveHorizontal * moveSpeed, 0f), ForceMode2D.Impulse); // Invert the player's controls
    }



    // Methods

        // Movement Methods
        
    private void PlayerChangeSpeed()
    {
        moveSpeed = maxMoveSpeed - (playerDrain.PlayerCurrentHealth / speedUpdatePercentage);
    }

    private void PlayerMovement()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal"); // Store -1 for left and +1 for right into the variable

        if (moveHorizontal > 0) // Moving right
        {
            PlayerMoveRight();
        }
        else if (moveHorizontal < 0) // Moving left
        {
            PlayerMoveLeft();
        }
        else // Standing still
        {
            anim.SetBool("isWalking", false);
        }
    }

    private void PlayerMoveRight()
    {
        anim.SetBool("isWalking", true);
        
        if (!invertedControls)
            gameObject.transform.localScale = new Vector3(4f, 4f, 4f);  // Flip the sprite towards right (***change this when you change the player's sprite/size***)

        if (invertedControls)
            gameObject.transform.localScale = new Vector3(-4f, 4f, 4f);  // Flip the sprite in the opposite direction (because of inverted controls)
    }

    private void PlayerMoveLeft()
    {
        anim.SetBool("isWalking", true);
        if (!invertedControls)
            gameObject.transform.localScale = new Vector3(-4f, 4f, 4f);  // Flip the sprite towards left (***change this when you change the player's sprite/size***)
    
        if (invertedControls)
            gameObject.transform.localScale = new Vector3(4f, 4f, 4f);  // Flip the sprite in the opposite direction (because of inverted controls)
    }

    private void InvertControls()
    {
        invertedControls = true;
        confusion.SetActive(true);  // Show confusion sprite

        ghostDuration = Random.Range(ghostMinDuration, ghostMaxDuration);

        Invoke("NormalizeControls", ghostDuration);
    }

    private void NormalizeControls()
    {
        invertedControls = false;
        confusion.SetActive(false);  // Hide confusion sprite
    }



        // Misc methods
    private void PlayerReduceHealth()
    {
        playerDrain.PlayerCurrentHealth -= playerDrain.PlayerLosingHPValue * Time.deltaTime;
        /*
        Slowly reduce player's current health 
        by PlayerLosingHealthValue (a variable that is constantly 
        increasing in the Update method)
        */

        if (playerDrain.PlayerCurrentHealth <= playerDrain.PlayerMinimumHP) // Cap health at 0 (it can't go lower and I don't really need this since it will be gameover at 0 but eh)
        {
            playerDrain.PlayerCurrentHealth = playerDrain.PlayerMinimumHP;
        }
    }

    private void PlayerDeath()
    {
        anim.SetBool("isDead", true);
        gameObject.GetComponent<PlayerDrainScript>().PlayerIsBusted = true;
    }
 

    private void GameOver()
    {
        gameOverText.SetActive(true);

        anim.SetBool("isDraining", false);

        // Disable Player
        gameObject.GetComponent<PlayerDrainScript>().enabled = false;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        Destroy(gameObject.GetComponent<Rigidbody2D>());
        isEnabled = false;
        this.enabled = false;
    }



    // Getters & Setters

        // Methods
    public void GetPlayerDeath() { PlayerDeath(); }
    public void GetGameOver() { GameOver(); }
    public void GetInvertControls() { InvertControls(); }

        // Variables
    public Animator Anim
    {
        get { return anim; }
        set { anim = value; }
    }

    public float MoveSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = value; }
    }

    public bool IsEnabled
    {
        get { return isEnabled; }
    }

}