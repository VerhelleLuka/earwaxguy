using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using static Cinemachine.DocumentationSortingAttribute;

public class BetterPlayerMovement : MonoBehaviour
{




    public Rigidbody2D body = null;
    private bool m_wantsRight = false;
    private bool m_wantsLeft = false;

    public float m_bounceTime = 0.1f;
    private List<GameObject> m_groundObjects = new List<GameObject>();
    public int groundCount = 0;

    /////MY VAIRABLES
    private float m_horizontalMovement;
    private float m_verticalMovement;

    //brakes
    public float brakeSpeed = 5f;

    public float slowDownRate = 5f;

    //dash
    public bool canDash = false;
    public float dashCooldown = 1.5f;
    private float dashCooldownTimer = 0f;

    public GameObject qtip;

    //swing
    public Vector2 swingPosition = new Vector2();
    public float swingRotationOffset = -90f;//In degrees
    public float angularVelocityBeforeSwing = 0f;
    private bool CanSwing = false;
    private bool Swinging = false;
    public bool swinging { get { return Swinging; } }
    private bool TryingToSwing = false;
    //arms
    public LineRenderer lineRenderer;
    public DistanceJoint2D springJoint;

    //States

    private PlayerState m_currentState;

    public PlayerState previousState;

    //Jump
    public float jumpGraceTime = 0.15f;
    public float jumpBufferTime = 0.2f;
    public bool jumpBufferRequest = false;

    //wallrun
    private bool m_WallRunPending = false;



    public float horizontalMovement
    {
        get { return m_horizontalMovement; }
    }
    public float verticalMovement
    {
        get { return m_verticalMovement; }
    }

    public List<GameObject> groundObjects
    {
        get { return m_groundObjects; }   // get method

    }

    void Start()
    {
        ChangeState(new GroundedState(this));
        body = GetComponent<Rigidbody2D>();
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void FixedUpdate()
    {
        m_currentState?.Update();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_WallRunPending)
        {
            ChangeState(new WallRunState(this));
            m_WallRunPending = false;
        }

        if (!canDash)
        {
            dashCooldownTimer += Time.deltaTime;
            if (dashCooldownTimer >= dashCooldown)
            {
                dashCooldownTimer = 0f;
                // canDash = true;
            }
        }

        if (m_currentState is not SwingingState && CanSwing)
        {
            Vector3 playerPos = transform.position;
            playerPos.z = 0f;
            Vector3 swingPos = swingPosition;
            //Draw line halfway between player and swing
            Vector3 lineRendererPos = playerPos + 0.5f * (swingPos - playerPos);
            lineRendererPos.z = -1f;
            lineRenderer.SetPosition(0, lineRendererPos);

            lineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y, -1f));
        }

        //brake
        if (horizontalMovement == 0f && verticalMovement == 0f && (m_currentState is GroundedState || m_currentState is WallRunState))
        {
            body.velocity = Vector3.Lerp(body.velocity, Vector3.zero, brakeSpeed * Time.deltaTime);
        }
    }



    public string GetCurrentStateName()
    {
        return m_currentState.GetType().Name;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() <= 0f) return; // only respond to press, not release

        bool onGround = groundObjects.Count > 0;
        bool onWallRun = m_currentState is WallRunState;
        bool falling = m_currentState is FallingState;
        bool dashState = m_currentState is DashState;
        bool jumping = m_currentState is JumpingState;

        // 1) Wallrun jump: always allow if wallrunning
        if (onWallRun && !jumping && !dashState)
        {
            ChangeState(new JumpingState(this));
            jumpGraceTime = -1f;
            return;
        }

        // 2) Normal grounded jump
        if (onGround && !jumping && !dashState)
        {
            ChangeState(new JumpingState(this));
            jumpGraceTime = -1f;
            return;
        }

        // 3) Coyote time jump: allow shortly after leaving ground
        if (falling && jumpGraceTime > 0f && previousState is not JumpingState && !jumping && !dashState)
        {
            ChangeState(new JumpingState(this));
            jumpGraceTime = -1f;
            return;
        }

        // 4) Buffered jump: pressed while falling but allowed to execute when landing
        if (falling && !jumping && !dashState)
        {
            jumpBufferRequest = true;
            jumpBufferTime = 0.2f;
        }

    }

    public void Swing(InputAction.CallbackContext context)
    {
        TryingToSwing = context.ReadValue<float>() > 0f ? true : false;

        if (!TryingToSwing)
        {
            Swinging = false;
            Destroy(springJoint);
            body.angularVelocity = angularVelocityBeforeSwing;
            lineRenderer.enabled = false;
            if (m_groundObjects.Count > 0)
            {
                ChangeState(new GroundedState(this));

            }
            else if(m_currentState is not WallRunState)
                ChangeState(new FallingState(this));
        }
        else if (CanSwing)
        {
            lineRenderer.enabled = true;
            ChangeState(new SwingingState(this));
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        m_horizontalMovement = context.ReadValue<Vector2>().x;
        m_verticalMovement = context.ReadValue<Vector2>().y;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (canDash && context.ReadValue<float>() > 0f)
        {
            canDash = false;
            ChangeState(new DashState(this, context.control.name));
        }
    }

    public void ChangeState(PlayerState newState)
    {
        if (m_currentState != null)
        {
            if (GetCurrentStateName() == newState.GetType().Name)
            {
                return;
            }
            previousState = m_currentState;
        }
        else
            previousState = newState;
        m_currentState?.Exit();
        m_currentState = newState;
        m_currentState.Enter();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Swing")
        {
            lineRenderer.enabled = true;
            CanSwing = true;
            swingPosition = collision.transform.position;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == ("Swing"))
        {
            CanSwing = false;

            if (m_currentState is SwingingState)
                lineRenderer.enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision);
        groundCount = m_groundObjects.Count;
        if (collision.transform.CompareTag("RunnableWall"))
        {
            // ChangeState(new WallRunState(this));
            m_WallRunPending = true;
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        m_groundObjects.Remove(collision.gameObject);
        groundCount = m_groundObjects.Count;

        foreach (GameObject Object in m_groundObjects)
        {

            if (Object.CompareTag("RunnableWall"))
            {
                m_WallRunPending = true;
                //ChangeState(new WallRunState(this));
            }
        }

    }

    public void ClearCollisions()
    {
        m_groundObjects.Clear();
        groundCount = m_groundObjects.Count;
    }

    private void ProcessCollision(Collision2D collision)
    {

        m_groundObjects.Remove(collision.gameObject);
        Vector3 pos = body.transform.position;

        foreach (ContactPoint2D contact in collision.contacts)
        {

            if (Mathf.Abs(contact.normal.y) > Mathf.Abs(contact.normal.x))
            {
                //Hit ground
                if (contact.normal.y > 0)
                {
                    if (m_groundObjects.Contains(contact.collider.gameObject) == false)
                    {
                        m_groundObjects.Add(contact.collider.gameObject);
                    }
                    if (m_currentState is FallingState)
                    {
                        //If we've been pushed up, we've hit the ground.  Go to a ground-based state.
                        if ((m_wantsRight || m_wantsLeft) && !collision.transform.CompareTag("RunnableWall"))
                        {
                            ChangeState(new GroundedState(this));
                        }
                        else if (collision.transform.CompareTag("RunnableWall"))
                        {
                            ChangeState(new WallRunState(this));
                        }
                    }
                }
                //Hit Roof
                else if (m_currentState is WallRunState && !collision.transform.CompareTag("RunnableWall"))
                {
                    ChangeState(new FallingState(this));

                }

            }

        }
        body.transform.position = pos;
    }
}
