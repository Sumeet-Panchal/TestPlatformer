using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "PlayerWeapon":
                if (!collision.gameObject.GetComponent<WeaponInfo>().held)
                {
                    collision.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0f;
                    collision.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
                }
                break;
        }
    }
}
