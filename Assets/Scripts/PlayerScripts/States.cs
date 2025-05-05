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
    Braking,
    Swinging
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
    public const float maxVelocity = 15f;
    public const float moveSpeed = 75f;

    public static Vector2 wallNormal;
    // public abstract PlayerState GetState();
}

public class JumpingState : PlayerState
{

    public JumpingState(BetterPlayerMovement player) : base(player)
    {

    }

    public override void Enter()
    {
        Debug.Log("Entering Jumping State");

        Vector2 newJumpDir = Vector2.zero;


        if (player.previousState.GetType().Name != "WallRunState")
        {
            player.body.velocity = new Vector2(player.body.velocity.x, 0);
            player.body.AddForce(new Vector2(0, jumpPower));

        }

        //WALL RUN JUMP
        else
        {
            newJumpDir = wallNormal * jumpPower;
            player.body.AddForce(new Vector2(newJumpDir.x, newJumpDir.y));
        }
        //else if (player.horizontalMovement < 0)
        //{
        //    newJumpDir = new Vector2(player.body.velocity.y, -1 * player.body.velocity.x).normalized * jumpPower;//90 degrees clockwise rotation
        //    player.body.AddForce(new Vector2(newJumpDir.x, newJumpDir.y));
        //}

        //else if (player.horizontalMovement > 0)
        //{
        //    newJumpDir = new Vector2(player.body.velocity.y * -1, player.body.velocity.x).normalized * jumpPower;//90 degrees counterclockwise rotation

        //    player.body.AddForce(new Vector2(newJumpDir.x, newJumpDir.y));
        //}
        player.canDash = true;


    }

    public override void Exit()
    {
        //Debug.Log("Exiting Jumping State");
    }

    public override void Update()
    {

        stateTimer += Time.fixedDeltaTime;

        //    player.body.AddForce(new Vector2(0, jumpPower / 2));


        if (player.body.velocityY <= 0 && player.previousState.GetType().Name != "WallRunState")
        {
            player.body.AddForce(new Vector2(player.horizontalMovement * moveSpeed * Time.fixedDeltaTime , 0));

            player.ChangeState(new FallingState(player));
        }
        else if (stateTimer > m_wallRunJumpDelay && player.previousState.GetType().Name == "WallRunState")
        {
            stateTimer = 0;

            player.ChangeState(new FallingState(player));

        }

    }
    private float m_wallRunJumpDelay = 1f;
    public float jumpVel = 0.575f;
    public float jumpPower = 400f;


}



//Idle state
public class GroundedState : PlayerState
{
    public GroundedState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        //Debug.Log("Entering Idle State");
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Idle State");
    }

    private float m_pushDownForce = 4f;

    public override void Update()
    {
        player.canDash = true;

        if (player.groundObjects.Count == 0)
        {
            player.ChangeState(new FallingState(player));
        }

        else if ((player.horizontalMovement < 0 && player.body.velocity.x > 0) || (player.horizontalMovement > 0 && player.body.velocity.x < 0))
        {
            player.body.angularVelocity = 0f;
            player.body.AddForce(new Vector2(player.horizontalMovement * moveSpeed * player.brakeSpeed * Time.fixedDeltaTime, 0));
        }

        else
        {
            player.body.AddForce(new Vector2(player.horizontalMovement * moveSpeed * Time.fixedDeltaTime, 0));
        }
       // player.body.velocity = new Vector2(Mathf.Clamp(player.body.velocity.x, maxVelocity * -1, maxVelocity), player.body.velocity.y);
    }
    //public override string GetState() { return "Idle"; }
}

public class WallRunState : PlayerState
{
    private int layerMask = LayerMask.GetMask("RunnableWall");
    public WallRunState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        // Debug.Log("Entering Wallrun State");
        player.body.gravityScale = 0;
        player.canDash = true;
    }

    public override void Exit()
    {
        // Debug.Log("Exiting Wallrun State");
        player.body.gravityScale = 1.5f;

    }


    public override void Update()
    {


        Vector3 p1 = player.transform.position;

        RaycastHit2D hit = Physics2D.Raycast(player.transform.position, wallNormal * -1, 0.5f, layerMask);

        if (wallNormal == Vector2.zero)
            hit = Physics2D.CircleCast(p1, 1, new Vector2(1, 0).normalized, 0.5f, layerMask);
        //hit = Physics2D.Raycast(player.transform.position, Vector2.down, 2f, layerMask);

        ApplyWallRunVelocity(wallNormal * -1);
        if (hit)
        {
            wallNormal = hit.normal;

            player.body.AddForce(wallNormal * -25);

        }
        else
            player.ChangeState(new FallingState(player));


        Debug.DrawLine(player.transform.position,
    new Vector3(player.transform.position.x + wallNormal.x,
   player.transform.position.y + wallNormal.y,
    player.transform.position.z), Color.blue);
        Debug.DrawLine(player.transform.position,
    new Vector3(player.transform.position.x + wallNormal.x * -1,
   player.transform.position.y + wallNormal.y * -1,
    player.transform.position.z), Color.red);
    }

    public void ApplyWallRunVelocity(Vector2 playerDownDirection)
    {

        Vector2 newMovementDir = Vector2.zero;
        if (player.horizontalMovement < 0)
        {
            newMovementDir = new Vector2(playerDownDirection.y, -1 * playerDownDirection.x);//90 degrees clockwise rotation
            player.body.AddForce(new Vector2(newMovementDir.normalized.x * moveSpeed * Time.fixedDeltaTime, newMovementDir.normalized.y * moveSpeed * Time.fixedDeltaTime));
        }
        else if (player.horizontalMovement > 0)
        {
            newMovementDir = new Vector2(playerDownDirection.y * -1, playerDownDirection.x);//90 degrees counterclockwise rotation
            player.body.AddForce(new Vector2(newMovementDir.normalized.x * moveSpeed * Time.fixedDeltaTime, newMovementDir.normalized.y * moveSpeed * Time.fixedDeltaTime));
        }
        player.body.velocity = Vector2.ClampMagnitude(player.body.velocity, maxVelocity * maxVelMult );

        Debug.DrawLine(player.transform.position,
new Vector3(player.transform.position.x + newMovementDir.x,
player.transform.position.y + newMovementDir.y,
player.transform.position.z), Color.yellow);
    }

    private const float maxVelMult = 1.3f;
}


public class FallingState : PlayerState
{
    public FallingState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        //Debug.Log("Entering Falling State");
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
        player.body.AddForce(new Vector2(player.horizontalMovement * moveSpeed * Time.fixedDeltaTime * 0.75f, 0));

    }
    //public override string GetState() { return "Falling"; }
}

public class SwingingState : PlayerState
{
    public SwingingState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        Debug.Log("Entering Swinging State");
        player.lineRenderer.enabled = true;

        if (player.springJoint == null)
        {
            player.springJoint = player.gameObject.AddComponent<DistanceJoint2D>();
            player.springJoint.connectedAnchor = player.swingPosition;
            player.springJoint.distance = Vector2.Distance(player.swingPosition, player.gameObject.transform.position);
            player.springJoint.enableCollision = false;
        }

        player.lineRenderer.SetPosition(0, new Vector3(player.swingPosition.x, player.swingPosition.y, -1f));

    }
    private float exitForce = 5f;
    public override void Exit()
    {
        player.body.AddForce(player.body.velocity.normalized * exitForce);
        player.canDash = true;
    }

    public override void Update()
    {
        player.canDash = false;
        Vector3 relativePos = new Vector3(player.swingPosition.x, player.swingPosition.y, 0) - player.transform.position;
        float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;
        angle += player.swingRotationOffset;
        player.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        player.lineRenderer.SetPosition(1, new Vector3(player.transform.position.x, player.transform.position.y, -1f));

        player.angularVelocityBeforeSwing = player.body.angularVelocity;

        if (player.body.velocity.magnitude < maxVelocity)
            player.body.AddForce(new Vector2(player.horizontalMovement * moveSpeed * Time.fixedDeltaTime * 2, 2 * player.verticalMovement * moveSpeed * Time.fixedDeltaTime));
    }
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
        //Debug.Log("Entering Dash State");
        player.body.angularVelocity = 0f;

        player.qtip.SetActive(true);

        Vector2 newVelocity = new Vector2(player.horizontalMovement, player.verticalMovement).normalized;

        //check if RMB is used
        if (m_btnUsed == "rightButton")
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 normalizedVector = mousePos - player.transform.position;

            newVelocity = normalizedVector.normalized;
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
        player.qtip.SetActive(false);
        //Debug.Log("Exiting Dash State");
    }

    public override void Update()
    {
        player.qtip.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(player.body.velocity.y, player.body.velocity.x) * Mathf.Rad2Deg);
        m_dashTimer += Time.deltaTime;

        if (m_dashTimer >= m_dashTime)
        {
            m_dashTimer = 0;
            if (player.groundCount > 0)
            {
                player.ChangeState(new GroundedState(player));
            }
            else
                player.ChangeState(new FallingState(player));
        }
        if (m_dashTimer > m_qtipTime)
        {
            player.qtip.SetActive(false);
        }
    }
    private const float m_dashTime = 0.8f;
    private const float m_qtipTime = 0.4f;
    private float m_dashTimer = 0f;

}
