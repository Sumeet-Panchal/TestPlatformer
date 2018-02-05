using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;

public class PlayerController : CharacterController {

    public float speedModifier;
    public float jumpStrength;
    public float jumpForce;
    public LayerMask groundLayers;
    public bool megaFire;
    public bool inHub;

    private bool onGround;
    private bool doubleJumping;
    private bool overItem;
    private Transform possPickupItem;

    // Use this for initialization
    void Start ()
    {
        onGround = false;
        doubleJumping = false;
        megaFire = false;
    }

    // Update is called once per frame
    void Update () {
        baseUpdate();
        if (Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }

        float horizontal = 0;
        float vertical = rb.velocity.y;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            horizontal = -speed;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            horizontal = speed;
        }
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            if (onGround) vertical = jumpForce;
            else
            {
                if (!doubleJumping)
                {
                    doubleJumping = true;
                    vertical = jumpForce;
                }
            }
        }
        if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && !onGround)
        {
            vertical = -jumpForce/3;
        }
        rb.velocity = new Vector2(horizontal,vertical);

        if ((Input.GetKeyDown(KeyCode.Q) && overItem) || (overItem && weapon == null))
        {
            pickUpItem(possPickupItem);
        } else if (Input.GetKeyDown(KeyCode.Q) && !overItem)
        {
            dropHeldItem();
        }

        if (weapon != null)
        {
            weapon.GetComponent<WeaponInfo>().inHub = inHub;
            bool fire = Input.GetMouseButtonDown(0);
            if (megaFire)
            {
                fire = Input.GetMouseButton(0);
                weapon.gameObject.GetComponent<SpriteRenderer>().color = Color.magenta;
            }
            else
            {
                weapon.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            }

            if (fire)
            {
                if (weapon.gameObject.GetComponent<WeaponInfo>().type == WeaponInfo.TYPES.THROWING)
                {
                    weapon.gameObject.GetComponent<WeaponInfo>().aimPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                weapon.gameObject.GetComponent<WeaponInfo>().attack();
            }
        }
    }

    public void dropHeldItem()
    {
        if (weapon != null)
        {
            if (!inHub)
            {
                weapon.parent = level.groundWeaponsParent;
            }
            else
            {
                weapon.parent = hub.weaponsParent;
            }
            weapon.SetPositionAndRotation(transform.position, new Quaternion(0, 0, 0, 0));
            weapon.gameObject.GetComponent<WeaponInfo>().held = false;
            weapon.gameObject.GetComponent<Rigidbody2D>().gravityScale = 1f;
            weapon.gameObject.GetComponent<SpriteRenderer>().color = weapon.gameObject.GetComponent<WeaponInfo>().originalColor;
            weapon = null;
            overItem = false;
        }
    }

    public void resetPlayer()
    {
        health = maxHealth;

    }

    public void pickUpItem(Transform item)
    {
        dropHeldItem();
        weapon = item;
        weapon.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0f;
        weapon.gameObject.GetComponent<WeaponInfo>().held = true;
        weapon.gameObject.GetComponent<WeaponInfo>().owner = this;
        weapon.parent = this.transform;
        weapon.SetPositionAndRotation(transform.position, Quaternion.identity);
        overItem = false;
    }

    private void LateUpdate()
    {
        if (weapon != null && !weapon.GetComponent<WeaponInfo>().isSwinging)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 dir = Input.mousePosition - pos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            weaponRotation = Quaternion.AngleAxis(angle - 90f, weapon.forward);
            baseLateUpdate();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject collider = collision.gameObject;

        if (inHub)
        {
            switch (collider.tag)
            {
                case "HubBuilding":
                    collider.GetComponent<HubBuildingController>().enterBuilding();
                    break;
            }
        }
        else
        {
            switch (collider.tag)
            {
                case "Goal":
                    if (level.enemiesParent.childCount == 0)
                    {
                        level.win();
                        collider.SetActive(false);
                    }
                    else
                    {
                        StartCoroutine(level.hud.timedText(level.hud.levelNotDone, 3f));
                    }
                    break;
                case "Checkpoint":
                    level.respawnPoint = new Vector2(collider.transform.position.x, collider.transform.position.y + 1);
                    break;
                case "EnemyWeapon":
                    if (collision.gameObject.GetComponent<WeaponInfo>().type == WeaponInfo.TYPES.MELEE)
                        damage(collider.GetComponent<WeaponInfo>().getDamage());
                    break;
                case "Pickup":
                    if ((collider.gameObject.GetComponent<PowerupController>().weaponPowerup && weapon != null) ||
                                (!collider.gameObject.GetComponent<PowerupController>().weaponPowerup))
                        collider.gameObject.GetComponent<PowerupController>().applyPowerup(this);
                    break;
                case "Explosion":
                    if (!collider.GetComponent<ExplosionController>().owner.Equals(this))
                        damage(collider.GetComponent<ExplosionController>().getDamage());
                    break;
            }
        }
        switch (collider.tag)
        {
            case "PlayerWeapon":
                if (collider.transform.parent != transform)
                {
                    overItem = true;
                    possPickupItem = collider.transform;
                }
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (inHub)
        {

        }
        else
        {

        }
        //Same regardless of hub or level
        if (possPickupItem == collision.transform) possPickupItem = null;

        switch (collision.gameObject.tag)
        {
            case "PlayerWeapon":
                overItem = false;
                possPickupItem = null;
                if (collision.gameObject.GetComponent<WeaponInfo>().thrown) collision.gameObject.GetComponent<PolygonCollider2D>().isTrigger = false;
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collider = collision.gameObject;

        if (inHub)
        {

        }
        else
        {

        }
        switch (collider.tag)
        {
            case "Enemy":
                onGround = true;
                doubleJumping = false;
                break;
            case "HubGround":
            case "Platform":
                onGround = true;
                doubleJumping = false;
                platform = collision.transform;
                break;

        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        GameObject collider = collision.gameObject;

        if (inHub)
        {

        }
        else
        {

        }
        switch (collider.tag)
        {
            case "Enemy":
            //Enemy Stuff, no break so you can jump off of enemies
            case "HubGround":
            case "Platform":
                onGround = false;
                break;

        }
    }
}
