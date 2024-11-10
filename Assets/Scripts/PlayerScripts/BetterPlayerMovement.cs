using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterPlayerMovement : MonoBehaviour
{
    private enum State
    {
        Idle = 0,
        Running,
        Falling,
        Jumping,
        Digging,
        Stuck,
        Dash
    }


    private State MovementState = State.Idle;

    private Rigidbody2D m_RigidBody = null;
    private Vector2 m_Vel = new Vector2(0, 0);
    void Start()
    {

    }

    private void Idle()
    { }
    private void Run() { }
    private void Fall() { }
    private void Jump() { }
    private void Dig() { }
    private void Stuck() { }

    private void Dash() { }

    // Update is called once per frame
    void Update()
    {
        switch (MovementState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Running:
                Run();
                break;
            case State.Falling:
                Fall();
                break;
            case State.Digging:
                Dig();
                break;
            case State.Jumping:
                Jump();
                break;
        }
    }

    void ApplyVelocity()
    {
        Vector3 pos = m_RigidBody.transform.position;
        pos.x += m_Vel.x;
        pos.y += m_Vel.y;
        m_RigidBody.transform.position = pos;
    }
}
