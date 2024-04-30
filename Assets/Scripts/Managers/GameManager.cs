using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool godMode = false;
    public float timer = 0f;
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
    }

    public void ResetLevel(Transform playerTransform)
    {
        timer = 0f;
        playerTransform.position = Vector3.zero;
        playerTransform.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        playerTransform.GetComponent<Rigidbody2D>().angularVelocity = 0f;
        playerTransform.rotation = Quaternion.identity;
    }
}
