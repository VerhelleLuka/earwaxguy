using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
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
    public const float moveSpeed = 300f;

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
        // Debug.Log("Entering Jumping State");

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
            player.body.AddForce(new Vector2(player.horizontalMovement * moveSpeed * Time.fixedDeltaTime, 0));

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
        //Debug.Log("Entering Grounded State");
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Idle State");
    }


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
    private Vector2 initialInputDir;
    private bool isFollowingWall = false;

    private const float followThreshold = 0.8f; //threshold for how far the player should the the joystick direction to maintain following of wall
    private const float wallStickForce = 40f;
    private const float wallFollowSpeed = moveSpeed;
    private const float reattachSpeed = moveSpeed * 0.75f;
    private const float stopThreshold = 0.2f;//to determine if player has stopped holding joystick
    private const float maxVelMult = 0.75f;

    //Prevent player not latching to wall
    private float wallStickGraceTimer = 0.1f;

    public WallRunState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        player.body.gravityScale = 0;
        player.canDash = true;
        wallStickGraceTimer = 0.1f;

        Vector2 input = new Vector2(player.horizontalMovement, player.verticalMovement);
        if (input.magnitude > stopThreshold)
        {
            initialInputDir = input.normalized;
            isFollowingWall = true;
        }
        else
        {
            initialInputDir = Vector2.zero;
            isFollowingWall = false;
        }
        //Debug.Log("ENTER WALLRUN");
    }

    public override void Exit()
    {
        player.body.gravityScale = 1.5f;
        //Debug.Log("EXIT WALLRUN");

    }
    IEnumerator AddForceToStick()
    {
        float extraWallStickForce = 10f;
        float duration = 0.1f;
        float timer = 0f;

        while (timer < duration)
        {
            player.body.AddForce(-wallNormal * wallStickForce * extraWallStickForce * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate(); // Wait until next physics update
        }
    }

    public override void Update()
    {
        Vector2 input = new Vector2(player.horizontalMovement, player.verticalMovement);
        float inputMag = input.magnitude;
        // Check still attached
        RaycastHit2D hit = Physics2D.Raycast(player.transform.position, -wallNormal, 0.6f, layerMask);
        if (wallNormal == Vector2.zero)
            hit = Physics2D.CircleCast(player.transform.position, 5f, Vector2.right, 0.6f, layerMask);
        if (!hit)
        {
            wallStickGraceTimer -= Time.deltaTime;
            if (wallStickGraceTimer <= 0f)
            {
                player.ChangeState(new FallingState(player));
                return;
            }
        }
        wallNormal = hit.normal;
        // Compute tangent from wall normal
        Vector2 tangent = new Vector2(-wallNormal.y, wallNormal.x).normalized;
        // Stick to wall
        player.body.AddForce(-wallNormal * wallStickForce);

        if(player.groundObjects.Count == 0)
        {
            player.StartCoroutine(AddForceToStick());
        }
        // If player released stick, exit follow mode
        if (inputMag < stopThreshold)
        {
            isFollowingWall = false;
        }
        Vector2 moveDir;
        if (isFollowingWall)
        {
            // Compare input direction to initial direction
            float similarity = Vector2.Dot(input.normalized, initialInputDir);
            if (similarity > followThreshold)
            {
                // Continue following tangent
                if (Vector2.Dot(player.body.velocity.normalized, tangent) < 0)
                    tangent = -tangent;
                moveDir = tangent;


            }
            else
            {
                // Player changed input direction → free mode
                isFollowingWall = false;
                initialInputDir = input.normalized;
                moveDir = input.normalized;
                // Redirect velocity to new input
                Vector2 newMoveDir = input.normalized;

                // Optional: blend current velocity and new input for smooth transition
                player.body.velocity = newMoveDir * player.body.velocity.magnitude * 0.75f;
                player.body.velocity = Vector2.Lerp(player.body.velocity, newMoveDir * player.body.velocity.magnitude, 0.2f);

            }
        }
        else
        {
            // Free mode — apply joystick direction directly (world space)

            if (inputMag > stopThreshold)
            {
                moveDir = input.normalized;
                //initialInputDir = input.normalized;
                isFollowingWall = true;
            }
            else
            {
                moveDir = Vector2.zero;
            }
        }
        // Apply movement
        if (moveDir != Vector2.zero)
        {
            float force = isFollowingWall ? wallFollowSpeed : reattachSpeed;
            player.body.AddForce(moveDir * force * Time.fixedDeltaTime);
        }
        // Clamp velocity
        player.body.velocity = Vector2.ClampMagnitude(player.body.velocity, maxVelocity * maxVelMult);
        // Debug visuals
        Debug.DrawLine(player.transform.position, player.transform.position + (Vector3)wallNormal, Color.blue);
        Debug.DrawLine(player.transform.position, player.transform.position + (Vector3)moveDir, Color.yellow);
    }
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
        //Debug.Log("Exiting Falling State");
        player.jumpGraceTime = 0.15f;

    }

    public override void Update()
    {

        player.jumpGraceTime -= Time.fixedDeltaTime;
        if (player.groundObjects.Count > 0)
        {
            player.ChangeState(new GroundedState(player));
        }
        //player.body.AddForce(new Vector2(player.horizontalMovement * moveSpeed * Time.fixedDeltaTime * 0.75f, 0));
        else if ((player.horizontalMovement < 0 && player.body.velocity.x > 0) || (player.horizontalMovement > 0 && player.body.velocity.x < 0))
        {
            player.body.AddForce(new Vector2(player.horizontalMovement * moveSpeed * player.brakeSpeed * Time.fixedDeltaTime, 0));
        }

        else
        {
            player.body.AddForce(new Vector2(player.horizontalMovement * moveSpeed * Time.fixedDeltaTime, 0));
        }
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
            //Debug.Log(Vector2.Dot(normalizedVector.normalized, player.body.velocity.normalized));

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
