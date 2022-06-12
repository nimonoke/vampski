using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDrainScript : MonoBehaviour
{
    [SerializeField] private float playerMaxHealth = 100f;
    [SerializeField] private float playerCurrentHealth = 50f;
    [SerializeField] private float playerLosingHPValue = 1f;
    [SerializeField] private float playerLosingHPSpeed = 1f;
    [SerializeField] private float drainSpeed = 20f;
    private float playerMinimumHP = 0f;

    private bool isDraining = false;
    private bool playerIsBusted = false;

    private PlayerBehaviour playerBehaviour;
    private CopSpawn copSpawn;
    
    [SerializeField] private Slider slider;



    private void Awake()
    {
        playerBehaviour = gameObject.GetComponent<PlayerBehaviour>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))  // Stop the draining and enable the player move script when you let go of SPACE
        {
            PlayerDrainStop();
        }

        PlayerDrainBehaviour();
    }

    private void OnTriggerStay2D(Collider2D other)  // Trigger to check if the victim is colliding with the player
    {
        if (other.gameObject.tag == "Victim" && playerBehaviour.IsEnabled)
        {
            PlayerDrainCheck();  // Check if the player is holding SPACE to see if it should run the Drain method or not

            VictimBehaviour victimBehaviour = other.gameObject.GetComponent<VictimBehaviour>();  // Assign only the current victim's script (not all the victims)

            if (isDraining)
            {
                victimBehaviour.GetVictimDrain();
                victimBehaviour.GetVictimDeathCheck();
                
                if (victimBehaviour.IsKilled)
                {
                    playerBehaviour.GetInvertControls();

                    copSpawn = other.GetComponent<CopSpawn>();
                    copSpawn.Invoke("GetSpawnCop", 0.1f);
                }

            }

            else if (!isDraining && !victimBehaviour.WasDrained)  // Basically if nothing happened yet (no drain ever) (*not sure if necessary, test later*)
            {
                victimBehaviour.GetVictimStopAnim();
            }

            else if (victimBehaviour.WasDrained)
            {
                other.GetComponent<VictimBehaviour>().GetVictimRunAway();

                copSpawn = other.GetComponent<CopSpawn>();
                copSpawn.Invoke("GetSpawnCop", 0.1f);
            }
        }
    }



    // Methods

        // Attack Methods
    public void PlayerDrainCheck()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            PlayerDrainBegin();
        }
        else
        {
            PlayerDrainStop();
        }
    }

    public void PlayerDrainBegin()
    {
        isDraining = true;
        playerBehaviour.Anim.SetBool("isDraining", true);

        playerBehaviour.enabled = false;
    }

    public void PlayerDrainStop()
    {
        isDraining = false;
        playerBehaviour.Anim.SetBool("isDraining", false);

        playerBehaviour.enabled = true;
    }

    private void PlayerDrainBehaviour()
    {
        if (isDraining == true)
        {
            playerCurrentHealth += drainSpeed * Time.deltaTime;
            
            if (playerCurrentHealth >= playerMaxHealth)  // Cap the health at max health (100)
                playerCurrentHealth = playerMaxHealth;
        }

        slider.value = playerCurrentHealth;  // Adjust the HP Bar
    }



    // Getters & Setters
    public bool IsDraining
    {
        get { return isDraining; }
        set { isDraining = value; }
    }

    public bool PlayerIsBusted
    {
        get { return playerIsBusted; }
        set { playerIsBusted = value; }
    }

    public float DrainSpeed
    {
        get { return drainSpeed; }
    }

    public float PlayerCurrentHealth
    {
        get { return playerCurrentHealth; }
        set { playerCurrentHealth = value; }
    }

    public float PlayerLosingHPValue
    {
        get { return playerLosingHPValue; }
        set { playerLosingHPValue = value; }
    }

    public float PlayerLosingHPSpeed
    {
        get { return playerLosingHPSpeed; }
        set { playerLosingHPSpeed = value; }
    }

    public float PlayerMinimumHP
    {
        get { return playerMinimumHP; }
    }

}
