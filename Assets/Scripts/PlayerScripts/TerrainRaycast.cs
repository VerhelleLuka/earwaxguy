using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainRaycast : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f);

        if(hit)
        {
            //Debug.Log(hit.normal);
            //Debug.Log(hit.)
        }
    }
}
