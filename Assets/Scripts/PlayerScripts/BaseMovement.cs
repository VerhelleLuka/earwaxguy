using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseMovement : MonoBehaviour
{
    protected float HorizontalMovement;
    protected float VerticalMovement;
    public Rigidbody2D body;

    protected void Move(InputAction.CallbackContext context)
    {
        HorizontalMovement = context.ReadValue<Vector2>().x;
        VerticalMovement = context.ReadValue<Vector2>().y;

    }

    private void Start()
    {
        body = GetComponentInParent<Rigidbody2D>();
    }
}

