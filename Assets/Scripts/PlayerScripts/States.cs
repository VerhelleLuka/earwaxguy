using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;
using static Cinemachine.DocumentationSortingAttribute;
using static UnityEngine.RuleTile.TilingRuleOutput;


public enum State
{
    Grounded,
    Falling,
    Jumping,
    Digging,
    Stuck,
    Dash,
    Braking
}

public abstract class PlayerState
{
    protected BetterPlayerMovement player;

    public PlayerState(BetterPlayerMovement player)
    {
        this.player = player;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();

    public float stateTimer = 0.0f;

    // public abstract PlayerState GetState();
}

public class JumpingState : PlayerState
{

    public JumpingState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        Debug.Log("Entering Jumping State");
        player.body.velocity = new Vector2(player.body.velocity.x, 0);
        player.body.AddForce(new Vector2(0, jumpPower));
    }

    public override void Exit()
    {
        Debug.Log("Exiting Jumping State");
    }

    public override void Update()
    {

        //m_stateTimer += Time.fixedDeltaTime;
        //if (m_stateTimer < player.jumpMinTime)
        //    player.body.AddForce(new Vector2(0, jumpPower / 2));


        if (player.body.velocityY <= 0)
        {
            stateTimer = 0;
            player.ChangeState(new FallingState(player));
        }
        //player.vel.y += player.gravity * Time.fixedDeltaTime;

        player.ApplyVelocity();
    }
    public float jumpVel = 0.575f;
    public float jumpPower = 350f;


}



//Idle state
public class GroundedState : PlayerState
{
    public GroundedState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
       Debug.Log("Entering Idle State");
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Idle State");
    }

    public override void Update()
    {
        player.ApplyVelocity();
    }
    //public override string GetState() { return "Idle"; }
}


public class FallingState : PlayerState
{
    public FallingState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        Debug.Log("Entering Falling State");
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Idle State");
    }

    public override void Update()
    {


        if (player.groundObjects.Count > 0)
        {
            player.ChangeState(new GroundedState(player));
        }
        player.ApplyVelocity();
    }
    //public override string GetState() { return "Falling"; }
}

public class DiggingState : PlayerState
{
    public DiggingState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        //Debug.Log("Entering Idle State");
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Idle State");
    }

    public override void Update()
    {

    }
}

public class StuckState : PlayerState
{
    public StuckState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        //Debug.Log("Entering Idle State");
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Idle State");
    }

    public override void Update()
    {

    }
}

public class DashState : PlayerState
{
    public DashState(BetterPlayerMovement player, string buttonName) : base(player)
    {
        m_btnUsed = buttonName;
    }

    private string m_btnUsed = "";
    private const float m_dashPower = 10f;
    public override void Enter()
    {
        Debug.Log("Entering Dash State");
        player.body.angularVelocity = 0f;


        Vector2 newVelocity = new Vector2(player.horizontalMovement , player.verticalMovement).normalized;

        //check if RMB is used
        if (m_btnUsed == "rightButton")
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 normalizedVector = mousePos - player.transform.position;
            
            newVelocity = normalizedVector.normalized;
            //newVelocity = new Vector2((mousePos.x - player.transform.position.x), (mousePos.y - player.transform.position.y)).normalized;
            Debug.Log(Vector2.Dot(normalizedVector.normalized, player.body.velocity.normalized));

            if (Vector2.Dot(normalizedVector.normalized, player.body.velocity.normalized) > 0)
            {
                player.body.velocity = newVelocity * m_dashPower + player.body.velocity;
            }
            else
            {
                player.body.velocity = newVelocity * m_dashPower;

                player.body.velocity = newVelocity * player.body.velocity.magnitude;
            }

            return;
        }
        
  //      Debug.Log(Vector2.Dot(new Vector2(player.horizontalMovement, player.verticalMovement).normalized, player.body.velocity.normalized));
        if (Vector2.Dot(new Vector2(player.horizontalMovement, player.verticalMovement).normalized, player.body.velocity.normalized) > 0)
            player.body.velocity = newVelocity * m_dashPower + player.body.velocity;
        else
        {
            player.body.velocity = newVelocity * m_dashPower;

            player.body.velocity = newVelocity * player.body.velocity.magnitude;
        }


    }

    public override void Exit()
    {
        //Debug.Log("Exiting Idle State");
    }

    public override void Update()
    {
        m_dashTimer += Time.deltaTime;

        if(m_dashTimer >= m_dashTime)
        {
            m_dashTimer = 0;
            if (player.groundCount > 0)
            {
                player.ChangeState(new GroundedState(player));
            }
            else
                player.ChangeState(new FallingState(player));
        }
    }
    private const float m_dashTime = 0.8f;
    private float m_dashTimer = 0f;

}
