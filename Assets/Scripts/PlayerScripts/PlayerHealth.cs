using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 10;

    private int Rings = 0;
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag =="Enemy")
        {
            if (Rings > 0)
            {
                LoseRings();
            }
            else
                Kill();
        }
    }

    private void Kill()
    {
        GameManager.instance.ResetLevel(transform);
    }

    void LoseRings()
    {
        Rings = 0;
    }

}
