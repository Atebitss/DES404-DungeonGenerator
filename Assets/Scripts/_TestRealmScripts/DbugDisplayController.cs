using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DbugDisplayController : MonoBehaviour
{
    [SerializeField] private TMP_Text DbugText;
    public Vector3 playerPosition = Vector3.zero;
    public Vector3 playerVelocity = Vector3.zero;

    public void SwitchVisible()
    {
        if (this.enabled) { this.gameObject.SetActive(false); }
        else if (!this.enabled) { this.gameObject.SetActive(true); }
    }

    private void FixedUpdate() { UpdateDisplayText(); }
    private void UpdateDisplayText()
    {
        DbugText.text =
            "Player Position: " + playerPosition +
            "\nPlayer Velocity: " + playerVelocity;
    }
}
