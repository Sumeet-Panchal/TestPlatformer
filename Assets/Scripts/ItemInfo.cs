using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemInfo : MonoBehaviour {
    public bool held;
    public LevelController level;
    public CharacterController owner;
    public Color originalColor;
    public bool inHub;
    public GameController hub;

    public void rotate()
    {
        float rotateSpeed = 1f;
        if (inHub)
        {
            rotateSpeed = hub.groundItemRotateSpeed;
        }
        else
        {
            rotateSpeed = level.groundItemsRotateSpeed;
        }
        transform.Rotate(new Vector2(0f, 1f) * rotateSpeed * Time.deltaTime);
    }
}
