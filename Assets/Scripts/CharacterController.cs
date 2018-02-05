using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

    public float speed;
    public float health;
    public float maxHealth;
    public LevelController level;
    public GameController hub;
    public Transform weapon;
    public Rigidbody2D rb;
    public Quaternion weaponRotation;
    public Transform platform;

    // Use this for initialization
    void Start () {
	}
	
    public void baseUpdate()
    {
        if (health <= 0) die();
    }

    public void baseLateUpdate()
    {
        weapon.rotation = weaponRotation;
    }

    public void damage(float damage)
    {
        health -= damage;
    }

    public void die()
    {
        if (this is PlayerController)
        {
            level.respawnPlayer();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
