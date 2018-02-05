using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public Transform playerCamPrefab;
    public PlayerController player;
    public Transform playerCam;
    public Transform weaponsParent;
    public Transform thrownParent;
    public float groundItemRotateSpeed;

    // Use this for initialization
    void Start()
    {
        if (SceneManager.sceneCount == 1 && player == null)
        {
            playerCam = Instantiate(playerCamPrefab, transform);
            player = playerCam.transform.Find("Player").GetComponent<PlayerController>();
            player.inHub = true;
            player.hub = this;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setPlayer(GameObject p)
    {
        playerCam = p.transform;
        player = p.transform.Find("Player").GetComponent<PlayerController>();
        player.gameObject.GetComponent<PlayerController>().level = null;
        player.gameObject.GetComponent<PlayerController>().inHub = true;
        player.gameObject.GetComponent<PlayerController>().resetPlayer();
        player.gameObject.GetComponent<PlayerController>().hub = this;
        if (player.gameObject.GetComponent<PlayerController>().weapon != null)
        {
            player.gameObject.GetComponent<PlayerController>().weapon.gameObject.GetComponent<WeaponInfo>().inHub = false;
            player.gameObject.GetComponent<PlayerController>().weapon.gameObject.GetComponent<WeaponInfo>().hub = this;
        }
        HudController hud = playerCam.transform.Find("Canvas").GetComponent<HudController>();
        hud.inhub = true;
        hud.hub = this;
    }
}
