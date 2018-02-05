using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour {

    public float speed;
    public float range;
    public float damage;
    public CharacterController origin;
    public bool megaFire;

    private Rigidbody2D rb;
    private Vector2 originPoint;

	// Use this for initialization
	void Start () {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        originPoint = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        bool regularFire = true;
        if (origin is PlayerController)
        {
            if (megaFire)
            {
                rb.AddRelativeForce(new Vector2(0f, speed));
                regularFire = false;
            }
        }

        if (regularFire) rb.velocity = transform.TransformDirection(new Vector2(0, speed));
        if (Vector2.Distance(originPoint, transform.position) >= range)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>().damage(damage);
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyController>().damage(damage);
        }
        if (!collision.gameObject.CompareTag("Projectile"))
        {
            Destroy(this.gameObject);
        }
    }
}
