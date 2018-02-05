using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour {

    public LevelController level;
    public GameController hub;
    public bool inhub;
    public Transform player;
    public Transform enemyParent;
    public Text winText;
    public Text dieText;
    public Text levelNotDone;
    public Text enemyCountText;
    public string enemyPreText;
    public Text healthText;
    public RectTransform healthBar;

    // Use this for initialization
    void Start ()
    {
        winText.enabled = levelNotDone.enabled = false;
        dieText.enabled = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        healthBar.sizeDelta = new Vector2((pc.health / pc.maxHealth) * 100f, 10f);
        if (inhub)
        {
            enemyCountText.enabled = false;
        }
        else
        {
            if (level.levelOver)
            {
                dieText.enabled = false;
                levelNotDone.enabled = false;
                StartCoroutine(timedText(winText, 3f));
            }
            else
            {
                enemyCountText.text = enemyPreText + enemyParent.childCount;
            }
        }
    }

    public IEnumerator timedText(Text text, float time)
    {
        text.enabled = true;
        yield return new WaitForSecondsRealtime(time);
        text.enabled = false;
    }
}
