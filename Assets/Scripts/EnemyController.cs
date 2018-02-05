using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : CharacterController {

    public enum TYPES { PROJECTILE, SWORD, CHARGE };
    public float velocity;
    public float maxX;
    public float minX;
    public float startY;
    public Transform player;
    public float cooldownTime;
    public LayerMask shootingLayers;
    public bool useWeapon;
    
    private bool canAttack;
    private float range;

    // Use this for initialization
    void Start ()
    {
        maxHealth = health;
        velocity = speed * (Random.Range(0f,1f)>.5f ? 1f : -1f);
        startY = transform.position.y;
        canAttack = true;

        if (useWeapon) range = weapon.gameObject.GetComponent<WeaponInfo>().range;
    }
	
	// Update is called once per frame
	void Update () {
        baseUpdate();
        if (transform.position.y <= level.lowestPos)
        {
            Destroy(this.gameObject);
        }
        if ((transform.position.y < (startY + 1f) && transform.position.y > (startY - 1f)) &&
            (transform.position.x > minX - 1.5f && transform.position.x < maxX + 1.5f))
        {
            if (transform.position.x > maxX)
            {
                velocity = -1f * Mathf.Abs(velocity);
            }
            else if (transform.position.x < minX)
            {
                velocity = Mathf.Abs(velocity);
            }
            rb.velocity = new Vector2(velocity, 0);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, -10);
        }

        RaycastHit2D ray = Physics2D.Linecast(transform.position, player.transform.position, shootingLayers);
        bool inRange = Vector2.Distance(transform.position, player.transform.position) <= range;
        if (ray != null && inRange)
        {
            if (ray.collider.gameObject.CompareTag("Player") && canAttack)
            {
                attack();
                StartCoroutine(attackCooldown());
            }
        }
    }

    public void attack()
    {
        if (weapon != null)
            weapon.gameObject.GetComponent<WeaponInfo>().attack();
    }

    private IEnumerator attackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(cooldownTime);
        canAttack = true;
    }

    private void LateUpdate()
    {
        if (weapon != null && !weapon.GetComponent<WeaponInfo>().isSwinging)
        {
            Vector3 pos = transform.position;
            Vector3 dir = player.transform.position - pos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            weaponRotation = Quaternion.AngleAxis(angle - 90f, weapon.forward);
            baseLateUpdate();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform") && !collision.transform.Equals(platform))
        {
            startY = collision.transform.position.y + .5f;
            platform = collision.transform;
            maxX = platform.transform.position.x + ((platform.transform.localScale.x / 2) * .9f);
            minX = platform.transform.position.x - ((platform.transform.localScale.x / 2) * .9f);
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(gameObject.GetComponent<Collider2D>(), collision.collider);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerWeapon") && 
            collision.gameObject.GetComponent<WeaponInfo>().type == WeaponInfo.TYPES.MELEE && 
            collision.gameObject.GetComponent<WeaponInfo>().held)
        {
            damage(collision.gameObject.GetComponent<WeaponInfo>().getDamage());
        }

        if (collision.gameObject.CompareTag("Explosion") && !collision.gameObject.GetComponent<ExplosionController>().owner.Equals(this))
        {
            damage(collision.gameObject.GetComponent<ExplosionController>().getDamage());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("EnemyWeapon"))
            if (collision.gameObject.GetComponent<WeaponInfo>().thrown) collision.gameObject.GetComponent<PolygonCollider2D>().isTrigger = false;
    }
}
