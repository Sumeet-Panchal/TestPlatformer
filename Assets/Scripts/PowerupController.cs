using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupController : MonoBehaviour {

    public enum TYPES { MEGA_WEAPON, HEALTH_UP}
    public TYPES type;
    public float powerTime;
    public float healthBack;
    public bool weaponPowerup;

    private Coroutine running;

    public void applyPowerup(PlayerController player)
    {
        switch (type)
        {
            case TYPES.MEGA_WEAPON:
                player.megaFire = true;
                break;
            case TYPES.HEALTH_UP:
                if (player.health < player.maxHealth)
                {
                    player.health += healthBack;
                    Destroy(this.gameObject);
                }
                return;

        }

        if (running != null)
            StopCoroutine(running);

        running = StartCoroutine(removePowerup(powerTime, player));
        gameObject.GetComponent<Renderer>().enabled = false;
        gameObject.GetComponent<Collider2D>().enabled = false;
    }

    private IEnumerator removePowerup(float time, PlayerController player)
    {
        yield return new WaitForSeconds(time);
        switch (type)
        {
            case TYPES.MEGA_WEAPON:
                player.megaFire = false;
                break;
        }
        running = null;
        Destroy(this.gameObject);
    }
}
