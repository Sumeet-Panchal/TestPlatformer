using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubBuildingController : MonoBehaviour {

    public enum TYPES { LEVELSTART}
    public TYPES type;
    public GameController hub;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void enterBuilding()
    {
        switch (type)
        {
            case TYPES.LEVELSTART:
                startLevel();
                break;
        }
    }

    private void startLevel()
    {
        PlayerController pc = hub.player;
        pc.transform.position = new Vector2(0, 0f);
        pc.inHub = false;
        SceneManager.LoadScene("level", LoadSceneMode.Additive);
        SceneManager.sceneLoaded += loadLevel;
    }

    private void loadLevel(Scene level, LoadSceneMode levelLoadMode)
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        GameObject levelContainer = level.GetRootGameObjects()[0];
        GameObject p = hub.playerCam.gameObject;
        p.transform.parent = levelContainer.transform;
        p.name = p.name.Replace("(Clone)", "");
        levelContainer.GetComponent<LevelController>().setPlayer(p);
        SceneManager.sceneLoaded -= loadLevel;
    }
}
