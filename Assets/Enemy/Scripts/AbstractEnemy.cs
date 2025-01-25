using UnityEngine;
using System.Collections.Generic;
public abstract class AbstractEnemy : MonoBehaviour
{
    //scripts
    public AbstractSceneManager SM;
    public PlayerController PC;
    
    //health
    public int health;
    public int GetHealth() { return health; }
    public void SetHealth(int newHealth) { health = newHealth; HealthCheck(); }
    public void AlterHealth(int change)
    {
        health += change;
        HealthCheck();
    }
    private void HealthCheck()
    {
        if (health <= 0)
        {
            //Debug.Log(this.gameObject.name + " has died!");
            Destroy(this.gameObject);
        }
        else
        {
            //Debug.Log(this.gameObject.name + " HP: " + health);
        }
    }


    //damage
    public int damage;
    public int GetDamage() { return damage; }
    public void SetDamage(int newDamage) { damage = newDamage; }


    //speed
    public int speed;
    public int GetSpeed() { return speed; }
    public void SetSpeed(int newSpeed) { speed = newSpeed; }

    //movement
    public bool moving = false;
    public void AllowMovement(bool allowed) { moving = allowed; }
    public abstract void StartMovement();


    //render
    public Renderer enemyRenderer;
    public Material baseMaterial;
    public void ResetMaterial() { this.gameObject.GetComponent<Renderer>().material = baseMaterial; }
    public void SetMaterial(Material newMaterial) { this.gameObject.GetComponent<Renderer>().material = newMaterial; }
}
