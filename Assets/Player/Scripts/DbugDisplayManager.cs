using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DbugDisplayManager : MonoBehaviour
{
    //text
    [SerializeField] private TMP_Text DbugText;

    //player world space
    public bool playerJumping = false, playerGrounded = false;
    public Vector3 playerPosition = Vector3.zero, playerVelocity = Vector3.zero, playerMovement = Vector3.zero;

    //player stats
    public int playerHealthCurrent = 0, playerHealthMax = 0;
    public float playerHealthPerSecond = 0f;
    public int playerStaminaCurrent = 0, playerStaminaMax = 0;
    public float playerStaminaPerSecond = 0f;
    public int playerMagicCurrent = 0, playerMagicMax = 0;
    public float playerMagicPerSecond = 0f;
    public int playerAttackDamage = 0;
    public float playerAttackSpeed = 0;

    //player dodge
    public bool playerDodging = false;
    public float playerDodgeCooldownTimer = 0f, playerDodgeStartTime = 0f;
    public Vector3 playerDodgeVelocity = Vector3.zero;

    //player attack
    public bool playerAttacking = false;
    public float playerAttackCooldownTimer = 0f, playerAttackStartTime = 0f;

    //player attack combo
    public bool playerComboing = false;
    public int lightAttackComboCounter = 0;
    public float playerLightAttackComboTimer = 0f, playerLightAttackComboStartTime = 0f;

    //player invincibility
    public bool playerInvincible = false;
    public float playerInvincibilityTimer = 0f, playerInvincibilityStartTime = 0f;



    public void SwitchVisible()
    {
        if (this.enabled) { this.gameObject.SetActive(false); }
        else if (!this.enabled) { this.gameObject.SetActive(true); }
    }

    private void FixedUpdate() { UpdateDisplayText(); }
    private void UpdateDisplayText()
    {
        DbugText.text =
            "Jumping: " + playerJumping +
            "   Grounded: " + playerGrounded +
            "\nPosition: " + playerPosition +
            "   Velocity: " + playerVelocity +
            "   Movement: " + playerMovement +
            "\nHP: " + playerHealthCurrent + "/" + playerHealthMax + " : " + playerHealthPerSecond +
            "\nStamina: " + playerStaminaCurrent + "/" + playerStaminaMax + " : " + playerStaminaPerSecond +
            "\nMagic: " + playerMagicCurrent + "/" + playerMagicMax + " : " + playerMagicPerSecond +
            "\nAttack: " + playerAttackDamage + "   Speed: " + playerAttackSpeed +
            "\nDodging: " + playerDodging +
            "   Dodge CD: " + playerDodgeCooldownTimer +
            "/Dodge Start: " + playerDodgeStartTime +
            "   Dodge Velocity: " + playerDodgeVelocity +
            "\nAttacking: " + playerAttacking +
            "   Attack CD: " + playerAttackCooldownTimer +
            "/Attack Start: " + playerAttackStartTime +
            "\nComboing: " + playerComboing +
            "   Combo Count: " + lightAttackComboCounter +
            "   Combo Timer: " + playerLightAttackComboTimer +
            "/Combo Start: " + playerLightAttackComboStartTime +
            "\nInvincible: " + playerInvincible +
            "   Invincible Timer: " + playerInvincibilityTimer +
            "/Invincible Start: " + playerInvincibilityStartTime;
    }
}
