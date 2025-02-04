using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    public float forceToAdd = 20f;
    private void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.GetComponentInParent<Rigidbody2D>().velocity= new Vector2(other.gameObject.GetComponentInParent<Rigidbody2D>().velocity.x, 0f);
        other.gameObject.GetComponentInParent<Rigidbody2D>().AddForce(new Vector2(0, transform.up.y * forceToAdd), ForceMode2D.Impulse);
        other.gameObject.GetComponentInParent<BetterPlayerMovement>().canDash = true;
    }
}
