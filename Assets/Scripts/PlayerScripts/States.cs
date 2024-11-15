using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public enum State
{
    Idle = 0,
    Running,
    Falling,
    Jumping,
    Digging,
    Stuck,
    Dash
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

    //public abstract string GetState();
}

public class JumpingState : PlayerState
{
    public JumpingState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        //Consoler.WriteLine("Entering Jumping State");

    }

    public override void Exit()
    {
        //Consoler.WriteLine("Exiting Jumping State");
    }

    public override void Update()
    {
        player.m_stateTimer += Time.fixedDeltaTime;

        if (player.m_stateTimer < player.jumpMinTime)
            player.vel.y = player.jumpVel;
        if (player.vel.y <= 0)
        {
            player.m_stateTimer = 0;
            player.ChangeState(new IdleState(player));
        }
        player.vel.y += player.gravity * Time.fixedDeltaTime;

        player.ApplyVelocity();
    }
    //public override string GetState() { return "Jumping"; }
}

//Idle state
public class IdleState : PlayerState
{
    public IdleState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
       //Consoler.WriteLine("Entering Idle State");
    }

    public override void Exit()
    {
        //Consoler.WriteLine("Exiting Idle State");
    }

    public override void Update()
    {
        
    }
    //public override string GetState() { return "Idle"; }
}



public class RunningState : PlayerState
{
    public RunningState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        //Consoler.WriteLine("Entering Idle State");
    }

    public override void Exit()
    {
        //Consoler.WriteLine("Exiting Idle State");
    }

    public override void Update()
    {

    }
    //public override string GetState() { return "Running"; }
}

public class FallingState : PlayerState
{
    public FallingState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        //Consoler.WriteLine("Entering Idle State");
    }

    public override void Exit()
    {
        //Consoler.WriteLine("Exiting Idle State");
    }

    public override void Update()
    {

    }
    //public override string GetState() { return "Falling"; }
}

public class DiggingState : PlayerState
{
    public DiggingState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        //Consoler.WriteLine("Entering Idle State");
    }

    public override void Exit()
    {
        //Consoler.WriteLine("Exiting Idle State");
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
        //Consoler.WriteLine("Entering Idle State");
    }

    public override void Exit()
    {
        //Consoler.WriteLine("Exiting Idle State");
    }

    public override void Update()
    {

    }
}

public class DashState : PlayerState
{
    public DashState(BetterPlayerMovement player) : base(player) { }

    public override void Enter()
    {
        //Consoler.WriteLine("Entering Idle State");
    }

    public override void Exit()
    {
        //Consoler.WriteLine("Exiting Idle State");
    }

    public override void Update()
    {

    }
}
