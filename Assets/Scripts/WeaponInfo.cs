using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInfo : ItemInfo {
    
    public float damage;
    public float swingDamage;
    public float explosionDamage;
    public float moveSpeed;
    public float range;
    public float projectileSpeed;
    public float throwSpeed;
    public Transform projectile;
    public Transform projectilesParent;
    public Transform explosion;
    public Transform thrownParent;
    public Vector3 shootingPosition;
    public enum TYPES { MELEE, THROWING, SHOOTER}
    public TYPES type;
    public float explodeTime;
    public bool isSwinging;
    public bool passiveDamageCooldown;
    public bool passiveDamageOnCooldown;
    public Vector2 aimPos;
    public float maxThrowHeight;
    public float maxThrowVelocity;
    public bool thrown;

    private float startRotationSwing;
    private float orientationModifier;
    
    // Use this for initialization
    void Start()
    {
        isSwinging = false;

        if (owner is EnemyController) passiveDamageCooldown = true;
        else passiveDamageCooldown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!held && !thrown)
        {
            rotate();
        }
        else if (thrown)
        {

        }
        else
        {
            float movement = moveSpeed;
            bool megaSwing = false;
            if (owner is PlayerController && owner.GetComponent<PlayerController>().megaFire)
            {
                movement *= 3;
                megaSwing = true;
            }

            transform.position = owner.transform.position;

            if (type == TYPES.MELEE && (isSwinging || megaSwing))
            {
                transform.Rotate(new Vector3(0f,0f,-20f) * orientationModifier * movement * Time.deltaTime);

                bool stopSwing = Mathf.DeltaAngle(startRotationSwing, transform.rotation.eulerAngles.z) >= 90f ||
                    Mathf.DeltaAngle(startRotationSwing, transform.rotation.eulerAngles.z) <= -90f;

                if (megaSwing)
                {
                    stopSwing = !owner.GetComponent<PlayerController>().megaFire;
                }

                if (stopSwing)
                {
                    isSwinging = false;
                }
            }
        }
    }

    public float getDamage()
    {
        if (passiveDamageCooldown)
        {
            if (!passiveDamageOnCooldown)
            {
                StartCoroutine(cooldownJab());
                if (isSwinging) return swingDamage;
                return damage;
            }
            return 0f;
        }
        if (isSwinging) return swingDamage;
        return damage;
    }

    private IEnumerator cooldownJab()
    {
        passiveDamageOnCooldown = true;
        yield return new WaitForSecondsRealtime(0.5f);
        passiveDamageOnCooldown = false;
    }

    public void attack()
    {
        switch (type)
        {
            case TYPES.MELEE:
                if (!isSwinging) swing();
                break;
            case TYPES.SHOOTER:
                shoot();
                break;
            case TYPES.THROWING:
                hurl();
                break;
        }
    }

    private void shoot()
    {
        shootingPosition = transform.TransformPoint(new Vector2(0, 1.35f));
        GameObject p = Instantiate(projectile, shootingPosition, Quaternion.identity).gameObject;
        p.GetComponent<ProjectileController>().damage = damage;
        p.transform.rotation = transform.rotation;
        if (!inHub)
        {
            p.transform.parent = level.projectilesParent;
            p.GetComponent<ProjectileController>().megaFire = level.player.GetComponent<PlayerController>().megaFire;
        }
        p.GetComponent<ProjectileController>().speed = projectileSpeed;
        p.GetComponent<ProjectileController>().origin = owner;
        p.GetComponent<ProjectileController>().range = range;
    }

    private void swing()
    {
        if (transform.rotation.eulerAngles.z < 180f)
            orientationModifier = -1f;
        else
            orientationModifier = 1f;

        isSwinging = true;
        transform.Rotate(new Vector3(0f,0f,60f) * orientationModifier);
        startRotationSwing = transform.rotation.eulerAngles.z;
    }

    private void hurl()
    {
        Vector3 fireVel;
        Vector3 fireVel2;
        bool canfire = false;
        if (owner is EnemyController)
        {
            Vector3 playerVelocity = level.player.GetComponent<Rigidbody2D>().velocity;
            if (playerVelocity.Equals(new Vector2(0, 0)))
                canfire = fts.solve_ballistic_arc(transform.position, throwSpeed, level.player.position, 9.81f, out fireVel, out fireVel2) > 0;
            else
                canfire = fts.solve_ballistic_arc(transform.position, throwSpeed, level.player.position, playerVelocity, 9.81f, out fireVel, out fireVel2) > 0;
        }
        else
        {
            canfire = fts.solve_ballistic_arc(transform.position, throwSpeed, aimPos, 9.81f, out fireVel, out fireVel2) > 0;
        }
        if (canfire)
        {

            if (!inHub)
                transform.parent = level.thrownParent;
            else
                transform.parent = hub.thrownParent;

            held = false;
            thrown = true;
            owner.weapon = null;
            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();

            rb.gravityScale = 1f;
            rb.velocity = fireVel;
            StartCoroutine(explode());
        }
    }

    private IEnumerator explode()
    {
        yield return new WaitForSeconds(explodeTime);
        Transform explos = Instantiate(explosion, transform.position, Quaternion.identity, transform.parent);
        explos.gameObject.GetComponent<ExplosionController>().owner = owner;
        explos.gameObject.GetComponent<ExplosionController>().damage = explosionDamage;
        Destroy(gameObject);
    }
}
