using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D body;
    public float moveSpeed = 5f;
    public float brakeSpeed = 0.5f;
    public float jumpPower = 50f;
    public float maxVelocity = 15f;

    public bool grounded = true;
    private float HorizontalMovement;
    private float VerticalMovement;

    //Swinging
    private DistanceJoint2D SpringJoint;
    private bool CanSwing = false;
    private bool Swinging = false;
    private Vector2 SwingPosition = new Vector2();
    private const float SwingRotationOffset = -90f;//In degrees
    private float AngularVelocityBeforeSwing = 0f;

    //arms
    public LineRenderer lineRenderer;

    //Stuck in (removable) wax
    private bool Stuck = false;
    private bool StuckRemovable = false;
    public float moveSpeedStuck = 60f;
    public float forceToAddOnExitWax = 10f;
    private float ExitWaxVelocityMultiplier = 0f;
    public float maxWaxVelocity = 150f;

    //dash
    private bool CanDash = true;
    public float dashPower = 10f;
    public float dashCooldown = 2f;
    private float dashCooldownTimer = 0f;
    public event Action DashExecuted;

    



    private void Update()
    {
        if (!CanDash)
        {
            dashCooldownTimer += Time.deltaTime;
            if (dashCooldownTimer >= dashCooldown)
            {
                dashCooldownTimer = 0f;
                CanDash = true;
            }
        }
        if (!Swinging && CanSwing)
        {
            Vector3 playerPos = transform.position;
            playerPos.z = 0f;
            Vector3 swingPos = SwingPosition;
            //Draw line halfway between player and swing
            Vector3 lineRendererPos = playerPos + 0.5f * (swingPos - playerPos);
            lineRendererPos.z = -1f;
            lineRenderer.SetPosition(0, lineRendererPos);

            lineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y, -1f));
        }
        else if (Swinging)
        {
            Vector3 relativePos = new Vector3(SwingPosition.x, SwingPosition.y, 0) - transform.position;
            float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;
            angle += SwingRotationOffset;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            lineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y, -1f));
        }
    }
    private void FixedUpdate()
    {

        //To check if player input is inverse of player velocity
        if (Stuck)
        {
            body.velocity = new Vector2(HorizontalMovement * moveSpeedStuck * Time.fixedDeltaTime, VerticalMovement * moveSpeedStuck * Time.fixedDeltaTime);
            return;
        }
        if (StuckRemovable)
        {
            if(body.velocity.magnitude < maxWaxVelocity)
                body.AddForce(new Vector2(HorizontalMovement, VerticalMovement) * moveSpeed * Time.fixedDeltaTime);
            //Player has free movement in the wax
            body.velocity = new Vector2(HorizontalMovement, VerticalMovement) * body.velocity.magnitude;

            return;
        }
        if (!Swinging)
        {
            if ((HorizontalMovement < 0 && body.velocity.x > 0) || (HorizontalMovement > 0 && body.velocity.x < 0))
                body.AddForce(new Vector2(HorizontalMovement * moveSpeed * brakeSpeed * Time.fixedDeltaTime, 0));

            else
                body.AddForce(new Vector2(HorizontalMovement * moveSpeed * Time.fixedDeltaTime, 0));
        }
        else
        {
            body.AddForce(new Vector2(HorizontalMovement * moveSpeed * Time.fixedDeltaTime, VerticalMovement * moveSpeed * Time.fixedDeltaTime));
        }
           
    }
    public void Move(InputAction.CallbackContext context)
    {
        HorizontalMovement = context.ReadValue<Vector2>().x;
        VerticalMovement = context.ReadValue<Vector2>().y;

    }
    public void Reset()
    {
        GameManager.instance.ResetLevel(transform);

    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (grounded)
        {
            body.velocity = new Vector2(body.velocity.x, 0);
            body.AddForce(new Vector2(0, jumpPower));
            grounded = false;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (CanDash || GameManager.instance.godMode)
        {
            body.velocity = new Vector2(0, 0);
            Vector2 newVelocity = new Vector2(HorizontalMovement, VerticalMovement).normalized;

            if (context.control.name == "rightButton")
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                newVelocity = new Vector2((mousePos.x - transform.position.x), (mousePos.y - transform.position.y)).normalized;
            }

            body.velocity = newVelocity * dashPower;

            DashExecuted?.Invoke();
            CanDash = false;
        }

    }
    public void Swing(InputAction.CallbackContext context)
    {
        if (CanSwing)
        {
            lineRenderer.enabled = true;

            if (SpringJoint == null)
            {
                SpringJoint = gameObject.AddComponent<DistanceJoint2D>();
                SpringJoint.connectedAnchor = SwingPosition;
                SpringJoint.distance = Vector2.Distance(SwingPosition, gameObject.transform.position);
                SpringJoint.enableCollision = false;
            }
        }
        if (context.ReadValue<float>() > 0f && CanSwing)
        {
            lineRenderer.SetPosition(0, new Vector3(SwingPosition.x, SwingPosition.y, -1f));
            Swinging = true;
            AngularVelocityBeforeSwing = body.angularVelocity;
        }

        else if (context.ReadValue<float>() == 0f)
        {
            Swinging = false;
            Destroy(SpringJoint);
            body.angularVelocity = AngularVelocityBeforeSwing;
            lineRenderer.enabled = false;

        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == ("Ground"))
        {
            grounded = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.tag == "Swing")
        {
            lineRenderer.enabled = true;
            CanSwing = true;
            SwingPosition = collision.transform.position;
        }
        if (collision.gameObject.tag == "RemovableWaxObject")
        {
            body.gravityScale = 0f;
            body.angularVelocity = 0f;
            StuckRemovable = true;
            ExitWaxVelocityMultiplier = collision.GetComponent<MarchingSquares>().expulsionStrength;

        }
        if (collision.gameObject.tag == "WaxObject")
        {
            body.gravityScale = 0f;
            body.velocity = body.velocity * 0.1f;
            body.angularVelocity = 0f;
            Stuck = true;

        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == ("Swing"))
        {
            CanSwing = false;
            if (!Swinging)
                lineRenderer.enabled = false;

        }
        if (collision.gameObject.tag == "RemovableWaxObject")
        {
            body.AddForce(body.velocity * forceToAddOnExitWax);
            body.gravityScale = 1f;
            StuckRemovable = false;
            body.AddForce(body.velocity.normalized * ExitWaxVelocityMultiplier);
        }
        if (collision.gameObject.tag == "WaxObject")
        {
            body.velocity = body.velocity.normalized;
            body.gravityScale = 1f;
            Stuck = false;
        }
    }
}
