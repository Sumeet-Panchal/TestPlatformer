using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {
    
    public Transform player;
    public Transform[] pickupWeapons;
    public float playerWeaponSpeed;
    public Transform platform;
    public Transform platformsParent;
    public Transform goal;
    public Transform[] enemies;
    public float enemyWeaponSpeed;
    public Transform enemiesParent;
    public Transform checkpoint;
    public Transform checkpointParent;
    public Transform[] pickups;
    public Transform pickupParent;
    public Transform projectilesParent;
    public Transform groundWeaponsParent;
    public Transform thrownParent;
    public HudController hud;
    public Vector2 respawnPoint;
    public int platformCount;
    public int platformsBetweenCheckpoint;
    public float lowestPos;
    public float ringOutDamage;
    public float groundItemsRotateSpeed;
    public bool levelOver;

    private Transform playerCamInternal;
    private Vector2 goalPosition;
    private float deathTextTime;

    // Use this for initialization
    void Start ()
    {
        respawnPoint = new Vector2(0, 0);
        spawnPlatforms();
        levelOver = false;
    }

    public void setPlayer(GameObject p)
    {
        player = p.transform.Find("Player");
        player.gameObject.GetComponent<PlayerController>().level = this;
        player.gameObject.GetComponent<PlayerController>().inHub = false;
        if (player.gameObject.GetComponent<PlayerController>().weapon != null)
            player.gameObject.GetComponent<PlayerController>().weapon.gameObject.GetComponent<WeaponInfo>().level = this;
        playerCamInternal = p.transform;
        hud = playerCamInternal.Find("Canvas").gameObject.GetComponent<HudController>();
        hud.inhub = false;
        hud.level = this;
        hud.enemyParent = enemiesParent;
    }
	
	// Update is called once per frame
	void Update () {
        if (player.transform.position.y < lowestPos ||
            player.transform.position.x < -10 ||
            player.transform.position.x > goalPosition.x + 20f)
        {
            Vector2 pos;
            if (player.gameObject.GetComponent<PlayerController>().platform != null)
            {
                Transform platform = player.gameObject.GetComponent<PlayerController>().platform;
                pos = new Vector2(platform.transform.position.x, platform.transform.position.y + 1);
            }
            else
            {
                pos = respawnPoint;
            }
            player.transform.SetPositionAndRotation(pos, Quaternion.identity);
            player.gameObject.GetComponent<PlayerController>().damage(ringOutDamage);
        }
    }

    public void win()
    {
        levelOver = true;
        StartCoroutine(endLevel());
    }

    public void respawnPlayer()
    {
        StartCoroutine(hud.timedText(hud.dieText, 5f));
        player.position = respawnPoint;
        player.gameObject.GetComponent<PlayerController>().health = player.gameObject.GetComponent<PlayerController>().maxHealth;
    }

    public IEnumerator endLevel()
    {
        yield return new WaitForSeconds(3f);
        PlayerController pc = player.gameObject.GetComponent<PlayerController>();
        pc.transform.position = new Vector2(0, 1.5f);
        SceneManager.LoadScene("hub", LoadSceneMode.Additive);
        SceneManager.sceneLoaded += backToHub;
    }

    private void backToHub(Scene hub, LoadSceneMode hubMode)
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        GameObject levelContainer = hub.GetRootGameObjects()[0];
        GameObject p = playerCamInternal.gameObject;
        p.transform.parent = levelContainer.transform;
        p.name = p.name.Replace("(Clone)", "");
        levelContainer.GetComponent<GameController>().setPlayer(p);
        SceneManager.sceneLoaded -= backToHub;
    }

    private void spawnPlatforms()
    {
        float x = 0f;
        float y = -1f;
        float scaleXMin = 4f;
        float scaleXMax = 8f;
        float scaleYMin = 0.25f;
        float scaleYMax = 1f;
        float scaleX = Random.Range(scaleXMin, scaleXMax);
        float scaleY = Random.Range(scaleYMin, scaleYMax);
        Transform cur = Instantiate(platform, new Vector2(x, y), Quaternion.identity, platformsParent);
        cur.localScale = new Vector2(scaleX, scaleY);
        bool spawnItem = false;
        for (int i = 1; i < platformCount; i++)
        {
            x = x + (scaleX + scaleXMin) + 2;
            y = y + Random.Range(-3f, 5.5f);
            scaleX = Random.Range(scaleXMin, scaleXMax);
            scaleY = Random.Range(scaleYMin, scaleYMax);
            cur = Instantiate(platform, new Vector2(x, y), Quaternion.identity, platformsParent);
            cur.localScale = new Vector2(scaleX, scaleY);

            bool spawnCheckpoint = (i % platformsBetweenCheckpoint == 0) && (i + (platformsBetweenCheckpoint/3) < platformCount);

            if (!spawnItem)
                spawnItem = (Random.Range(0f, 1f) >= .7f) && (i != platformCount - 1);
            else
                spawnItem = false;

            bool spawnWeapon = Random.Range(0f, 1f) >= .5f;

            if (spawnCheckpoint)
                Instantiate(checkpoint, new Vector2(x, y + scaleY / 3), Quaternion.identity, checkpointParent);
            
            else if (spawnItem) 
                Instantiate(pickups[Random.Range(0, pickups.Length)], new Vector2(x, y + scaleY / 3), Quaternion.identity, pickupParent);
                
            

            if (spawnWeapon)
            {
                Transform weapon = Instantiate(pickupWeapons[Random.Range(0,pickupWeapons.Length)], new Vector2(x + Random.Range(-1f * scaleX/2 + 0.1f, scaleX/ 2 - 0.1f), y + scaleY / 2), Quaternion.identity, groundWeaponsParent);
                WeaponInfo wi = weapon.gameObject.GetComponent<WeaponInfo>();
                weapon.gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;
                wi.held = false;
                wi.projectilesParent = projectilesParent;
                wi.projectileSpeed = playerWeaponSpeed;
                wi.owner = player.gameObject.GetComponent<PlayerController>();
                wi.level = this;
            }

            spawnEnemy(cur, spawnCheckpoint || spawnItem || i == platformCount-1);

            if (y - 10f < lowestPos) lowestPos = y - 10f;
        }
        goalPosition = new Vector2(x, y + scaleY / 3);
        Instantiate(goal, goalPosition, Quaternion.identity);
    }

    private void spawnEnemy(Transform platform, bool spawnItem)
    {
        if (Random.Range(0f, 1f) >= .5f)
        {
            float x = platform.transform.position.x;
            float y = platform.transform.position.y;

            if (spawnItem) x -= 1f;

            Transform e = Instantiate(enemies[Random.Range(0, enemies.Length)], new Vector2(x, y + 1f), Quaternion.identity, enemiesParent);
            EnemyController ec = e.gameObject.GetComponent<EnemyController>();
            ec.level = this;
            ec.platform = platform;

            if (spawnItem) x += 1f;

            ec.maxX = x + ((platform.transform.localScale.x / 2) * .9f);
            ec.minX = x - ((platform.transform.localScale.x / 2) * .9f);
            ec.player = player;
            ec.weapon.gameObject.GetComponent<WeaponInfo>().projectilesParent = projectilesParent;
            ec.weapon.gameObject.GetComponent<WeaponInfo>().level = this;
        }
    }
}
