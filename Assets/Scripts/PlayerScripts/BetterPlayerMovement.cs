using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Cinemachine.DocumentationSortingAttribute;

public class BetterPlayerMovement : MonoBehaviour
{
    public float moveAccel = (0.12f * 60.0f);
    public float groundFriction = 0.85f;
    public float gravity = (-0.05f * 60.0f);

    public float bounceVel = 1.25f;
    public float jumpMinTime = 0.06f;
    public float jumpMaxTime = 0.20f;
    public float airFallFriction = 0.975f;
    public float airMoveFriction = 0.85f;
    public Vector2 vel = new Vector2(0, 0);



    private Rigidbody2D m_rigidBody = null;
    private bool m_jumpPressed = false;
    private bool m_jumpHeld = false;
    private bool m_wantsRight = false;
    private bool m_wantsLeft = false;
    private bool m_shootPressed = false;
    private bool m_fireRight = true;
    private bool m_hasWeapon = false;
 
    public float m_bounceTime = 0.1f;
    private List<GameObject> m_groundObjects = new List<GameObject>();

    public List<GameObject> groundObjects
    {
        get { return m_groundObjects; }   // get method

    }

    public PlayerState m_currentState;
    void Start()
    {
        ChangeState(new IdleState(this));
        m_rigidBody = GetComponent<Rigidbody2D>();
    }


    // Update is called once per frame
    void Update()
    {
        m_currentState?.Update();
    }

    void ApplyAngularVelocity()
    {

    }

    public void ApplyVelocity()
    {
        Vector3 pos = m_rigidBody.transform.position;
        pos.x += vel.x;
        pos.y += vel.y;
        m_rigidBody.transform.position = pos;
    }

    void ApplyGravity()
    {
        
    }

    public string GetCurrentState()
    {
        return m_currentState.GetType().Name;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(context.ReadValue<float>() > 0f && m_groundObjects.Count > 0)
            ChangeState(new JumpingState(this));
       
    }

    public void ChangeState(PlayerState newState)
    {
        m_currentState?.Exit();
        m_currentState = newState;
        m_currentState.Enter();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        m_groundObjects.Remove(collision.gameObject);
    }

    public void ClearCollisions()
    {
        m_groundObjects.Clear();
    }

    private void ProcessCollision(Collision2D collision)
    {
        m_groundObjects.Remove(collision.gameObject);
        Vector3 pos = m_rigidBody.transform.position;

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
                    if (GetCurrentState() == "FallingState")
                    {
                        //If we've been pushed up, we've hit the ground.  Go to a ground-based state.
                        if (m_wantsRight || m_wantsLeft)
                        {       
                            ChangeState(new RunningState(this));
                        }
                        else
                        {
                            ChangeState(new IdleState(this));
                        }
                    }
                }
                //Hit Roof
                else
                {
                    vel.y = 0;
                    ChangeState(new FallingState(this));
                }
            }
            else
            {
                if ((contact.normal.x > 0 && vel.x < 0) || (contact.normal.x < 0 && vel.x > 0))
                {
                    vel.x = 0;
                }
            }
        }
        m_rigidBody.transform.position = pos;
    }
}
