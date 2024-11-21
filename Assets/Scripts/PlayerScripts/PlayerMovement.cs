using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PlayerMovement : BaseMovement
{
    public float moveSpeed = 5f;
    public float brakeSpeed = 0.5f;
    public float jumpPower = 50f;
    public float maxVelocity = 15f;

    private bool Grounded = true;
    public bool grounded { get { return Grounded; } }


    //Swinging
    private DistanceJoint2D SpringJoint;
    private bool CanSwing = false;
    private bool Swinging = false;
    public bool swinging { get { return Swinging; } }
    private bool TryingToSwing = false;
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

    //Jump Grace Time
    private bool CanJump = true;
    private bool JumpRequest = false;
    private const float JumpRequestGraceTime = 0.3f;
    private float JumpRequestGraceTimer = 0f;

    private void Start()
    {
        body = GetComponentInParent<Rigidbody2D>();
    }

    private bool CheckSwing()
    {

        if (CanSwing && TryingToSwing)
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

        if (CanSwing && TryingToSwing)
        {

            lineRenderer.SetPosition(0, new Vector3(SwingPosition.x, SwingPosition.y, -1f));
            Swinging = true;
            AngularVelocityBeforeSwing = body.angularVelocity;
            TryingToSwing = false;
            return true;
        }
        return Swinging;
    }
    private void Swing()
    {
        Vector3 relativePos = new Vector3(SwingPosition.x, SwingPosition.y, 0) - transform.position;
        float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;
        angle += SwingRotationOffset;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        lineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y, -1f));

    }


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
        if (CheckSwing())
            Swing();

        if (JumpRequest)
        {
            JumpRequestGraceTimer += Time.deltaTime;
            if (JumpRequestGraceTimer < JumpRequestGraceTime && CanJump)
            {
                Jump();

            }
            else if(JumpRequestGraceTimer > JumpRequestGraceTime)
            {
                JumpRequestGraceTimer = 0f;
                JumpRequest = false;
            }
        }

    }
    public void Swing(InputAction.CallbackContext context)
    {
        TryingToSwing = context.ReadValue<float>() > 0f ? true : false;

        if (!TryingToSwing)
        {
            Swinging = false;
            Destroy(SpringJoint);
            body.angularVelocity = AngularVelocityBeforeSwing;
            lineRenderer.enabled = false;

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
        else if (StuckRemovable)
        {
            if (body.velocity.magnitude < maxWaxVelocity)
                body.AddForce(new Vector2(HorizontalMovement, VerticalMovement) * moveSpeed * Time.fixedDeltaTime);
            //Player has free movement in the wax
            body.velocity = new Vector2(HorizontalMovement, VerticalMovement) * body.velocity.magnitude;

            return;
        }
        else if (!Swinging)
        {
            if ((HorizontalMovement < 0 && body.velocity.x > 0) || (HorizontalMovement > 0 && body.velocity.x < 0))
                body.AddForce(new Vector2(HorizontalMovement * moveSpeed * brakeSpeed * Time.fixedDeltaTime, 0));

            else if (body.velocity.magnitude < maxVelocity)
                body.AddForce(new Vector2(HorizontalMovement * moveSpeed * Time.fixedDeltaTime, 0));
        }
        else //If player is swinging
        {
            Debug.Log("Here");
            if (body.velocity.magnitude < maxVelocity)
                body.AddForce(new Vector2(HorizontalMovement * moveSpeed * Time.fixedDeltaTime * 2, 2*  VerticalMovement * moveSpeed * Time.fixedDeltaTime));
        }

    }
    public void Reset()
    {
        GameManager.instance.ResetLevel(transform);
    }
    private void Jump()
    {
        if (CanJump)
        {
            body.velocity = new Vector2(body.velocity.x, 0);
            body.AddForce(new Vector2(0, jumpPower));
            CanJump = false;
            JumpRequest = false;
        }
        else
        {
            JumpRequest = true;
        }
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (CanDash && context.ReadValue<float>() > 0f)
            Jump();
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if ((CanDash && context.ReadValue<float>() > 0f) || GameManager.instance.godMode)
        {
            body.angularVelocity = 0f;
            body.velocity = new Vector2(0, 0);

            Vector2 newVelocity = new Vector2(HorizontalMovement, VerticalMovement).normalized;

            if (context.control.name == "rightButton")
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                newVelocity = new Vector2((mousePos.x - transform.position.x), (mousePos.y - transform.position.y)).normalized;
            }
            if (body.velocity.magnitude < dashPower)
                body.velocity = newVelocity * dashPower;
            else
                body.velocity = newVelocity * body.velocity.magnitude;

           DashExecuted?.Invoke();
            CanDash = false;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == ("Ground"))
        {
            Grounded = true;
            CanJump = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == ("Ground"))
        {
            Grounded = false;
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
            Grounded = false;
            CanJump = false;
            StuckRemovable = true;
            ExitWaxVelocityMultiplier = collision.GetComponent<MarchingSquares>().expulsionStrength;

        }
        if (collision.gameObject.tag == "WaxObject")
        {
            body.gravityScale = 0f;
            body.velocity = body.velocity * 0.1f;
            body.angularVelocity = 0f;
            Stuck = true;
            Grounded = false;

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
