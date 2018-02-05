using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour {
    
    public float radiusScale;
    public CharacterController owner;
    public float damage;
    public float speed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.localScale =  new Vector2(transform.localScale.x + speed * Time.deltaTime, transform.localScale.y + speed * Time.deltaTime);
        if (transform.localScale.x >= radiusScale)
        {
            Destroy(this.gameObject);
        }
    }

    public float getDamage()
    {
        return damage;
    }
}
