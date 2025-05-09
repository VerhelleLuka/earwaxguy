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
    public float brakeSpeed = 0.5f;
 
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
    }


    // Update is called once per frame
    void Update()
    {
        m_currentState?.Update();

        if (!canDash)
        {
            dashCooldownTimer += Time.deltaTime;
            if (dashCooldownTimer >= dashCooldown)
            {
                dashCooldownTimer = 0f;
                // canDash = true;
            }
        }

        if (GetCurrentStateName() != "SwingingState" && CanSwing)
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
    }



    public string GetCurrentStateName()
    {
        return m_currentState.GetType().Name;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() > 0f && (GetCurrentStateName() == "WallRunState" || m_groundObjects.Count > 0) && GetCurrentStateName() != "JumpingState")
        {
            ChangeState(new JumpingState(this));
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
            ChangeState(new GroundedState(this));
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
            previousState = m_currentState;
        else
            previousState =newState;
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

            if (GetCurrentStateName() != "SwingingState")
                lineRenderer.enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision);
        groundCount = m_groundObjects.Count;

        if (collision.transform.CompareTag("RunnableWall"))
        {
            ChangeState(new WallRunState(this));
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        m_groundObjects.Remove(collision.gameObject);
        groundCount = m_groundObjects.Count;

        bool wallRunning = false;
        foreach (GameObject Object in m_groundObjects)
        {

            if (Object.CompareTag("RunnableWall"))
            {
                wallRunning = true;
            }
        }
        if (!wallRunning)
        {
            //ChangeState(new FallingState(this));
        }
        //Debug.Log(wallRunning);
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
            //Push back out
            //Vector2 impulse = contact.normal * (contact.normalImpulse / Time.fixedDeltaTime);
            //pos.x += impulse.x;
            //pos.y += impulse.y;
            //Debug.Log(pos);

            if (Mathf.Abs(contact.normal.y) > Mathf.Abs(contact.normal.x))
            {
                //Hit ground
                if (contact.normal.y > 0)
                {
                    if (m_groundObjects.Contains(contact.collider.gameObject) == false)
                    {
                        m_groundObjects.Add(contact.collider.gameObject);
                    }
                    if (GetCurrentStateName() == "FallingState")
                    {
                        //If we've been pushed up, we've hit the ground.  Go to a ground-based state.
                        if (m_wantsRight || m_wantsLeft)
                        {
                            ChangeState(new GroundedState(this));
                        }
                    }
                }
                //Hit Roof
                else

                    ChangeState(new FallingState(this));

            }

        }
        body.transform.position = pos;
    }
}
