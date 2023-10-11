using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public abstract class PlayerState
{
    protected PlayerStateMachine playerStateMachine;
    protected EnvironmentStateMachine environment;
    protected CharacterController2D controller;
    protected Inputs input;
    protected Animator animator;

    public PlayerState(PlayerStateMachine player)
    {
        playerStateMachine = player;
        environment = player.environment;
        controller = player.controller;
        input = player.input;
        animator = player.animator;
    }

    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();

    protected const string ANIM_PARAM_SPEED = "Speed";
    protected const string ANIM_PARAM_ATTACK_SPEED = "AttackSpeed";

    protected const string ANIM_PARAM_IS_JUMPING = "IsJumping";
    protected const string ANIM_PARAM_IS_WALL_SLIDING = "IsWallSliding";
    protected const string ANIM_PARAM_IS_DEAD = "IsDead";

    protected const string ANIM_PARAM_JUMP = "Jump";
    protected const string ANIM_PARAM_DOUBLE_JUMP = "DoubleJump";
    protected const string ANIM_PARAM_DASH = "Dash";
    protected const string ANIM_PARAM_ATTACK = "Attack";
    protected const string ANIM_PARAM_HIT = "Hit";

}

public class WalkingState : PlayerState
{
    public WalkingState(PlayerStateMachine player) : base(player) { }

    public override void OnEnter()
    {

    }

    public override void OnUpdate()
    {
        controller.Move(input.move.x);
        animator.SetFloat(ANIM_PARAM_ATTACK_SPEED, Mathf.Abs(input.move.x * controller.speed * Time.deltaTime));

        if (input.primaryAttack || input.secondaryAttack)
            playerStateMachine.SwitchState(playerStateMachine.attackingState);
        else if (input.dash && controller.canDash)
            playerStateMachine.SwitchState(playerStateMachine.dashingState);
        else if (input.jump && controller.grounded || input.jump && controller.doubleJump)
            playerStateMachine.SwitchState(playerStateMachine.jumpingState);
        else if (environment.isWallState && input.move.x * controller.transform.localScale.x > controller.deadZone || Mathf.Abs(input.move.x) <= controller.deadZone)
            playerStateMachine.SwitchState(playerStateMachine.idlingState);
    }


    public override void OnExit()
    {
        animator.SetFloat(ANIM_PARAM_ATTACK_SPEED, 0);
    }
}

public class IdlingState : PlayerState
{
    public IdlingState(PlayerStateMachine player) : base(player) { }

    public override void OnEnter()
    {

    }

    public override void OnUpdate()
    {
        if (environment.isWallState && input.move.x * controller.transform.localScale.x > controller.deadZone)
            controller.m_Rigidbody2D.velocity = new Vector3(0, 0);

        if (input.primaryAttack || input.secondaryAttack)
            playerStateMachine.SwitchState(playerStateMachine.attackingState);
        else if (input.dash && controller.canDash)
            playerStateMachine.SwitchState(playerStateMachine.dashingState);
        else if (input.jump)
        {
            if (controller.grounded || controller.doubleJump || environment.isWallState && input.move.x * controller.transform.localScale.x > controller.deadZone)
                playerStateMachine.SwitchState(playerStateMachine.jumpingState);
            else
                input.jump = false;
        }
        else if (Mathf.Abs(input.move.x) > controller.deadZone && !environment.isWallState || environment.isWallState && input.move.x * controller.transform.localScale.x < controller.deadZone)
            playerStateMachine.SwitchState(playerStateMachine.walkingState);


    }

    public override void OnExit()
    {

    }
}

public class JumpingState : PlayerState
{
    public JumpingState(PlayerStateMachine player) : base(player) { }

    float timePassed = 0f;


    public override void OnEnter()
    {
        if (!controller.grounded)
            controller.doubleJump = false;



        controller.Jump(isDoubleJump: !controller.doubleJump,
            isWallJump: environment.isWallState && input.move.x * controller.transform.localScale.x > controller.deadZone);

        timePassed = 0f;

        if (controller.doubleJump)
            input.jump = false;
        animator.SetTrigger(ANIM_PARAM_JUMP);
    }

    public override void OnUpdate()
    {
        controller.Move(input.move.x);
        if (!controller.doubleJump)
            input.jump = false;

        if (input.primaryAttack || input.secondaryAttack)
            playerStateMachine.SwitchState(playerStateMachine.attackingState);
        else if (input.dash && controller.canDash)
            playerStateMachine.SwitchState(playerStateMachine.dashingState);
        else if (input.jump)
        {
            playerStateMachine.SwitchState(playerStateMachine.jumpingState);
            animator.SetTrigger(ANIM_PARAM_DOUBLE_JUMP);
        }

        timePassed += Time.deltaTime;

        if (timePassed > controller.jumpTimeout)
            if (!environment.isAirboneState)
                if (Mathf.Abs(input.move.x) > controller.deadZone)
                    playerStateMachine.SwitchState(playerStateMachine.walkingState);
                else
                    playerStateMachine.SwitchState(playerStateMachine.idlingState);

    }

    public override void OnExit()
    {

    }
}

public class DashingState : PlayerState
{
    public DashingState(PlayerStateMachine player) : base(player) { }
    float timePassed = 0f;


    public override void OnEnter()
    {
        timePassed = 0f;
        controller.Move(input.move.x);
        controller.Dash();
        animator.SetTrigger(ANIM_PARAM_DASH);
    }

    public override void OnUpdate()
    {
        timePassed += Time.deltaTime;
        controller.Move(input.move.x);

        if (timePassed < controller.dashTimeout)
            return;

        if (input.primaryAttack || input.secondaryAttack)
            playerStateMachine.SwitchState(playerStateMachine.attackingState);
        else if (input.jump && controller.grounded || input.jump && controller.doubleJump)
            playerStateMachine.SwitchState(playerStateMachine.jumpingState);
        else if (Mathf.Abs(input.move.x) > controller.deadZone)
            playerStateMachine.SwitchState(playerStateMachine.walkingState);
        else if (Mathf.Abs(input.move.x) <= controller.deadZone)
            playerStateMachine.SwitchState(playerStateMachine.idlingState);
    }

    public override void OnExit()
    {
        if (!environment.isAirboneState)
            controller.canDash = true;
        controller.m_Rigidbody2D.velocity = Vector3.zero;
        input.dash = false;
    }
}

public class AttackingState : PlayerState
{
    public AttackingState(PlayerStateMachine player) : base(player) { }

    float timePassed = 0f;
    float currentAttackSpeed = 0f;
    float currentAttackTimeout = 0f;
    float currentAttackDamage = 0f;

    public override void OnEnter()
    {
        timePassed = 0f;
        if (environment.isAirboneState)
            controller.m_Rigidbody2D.velocity = new Vector3(0, 0);
        if (input.primaryAttack)
        {
            currentAttackTimeout = controller.primaryAttackTimeout;
            currentAttackSpeed = controller.primaryAttackSpeed;
            currentAttackDamage = controller.primaryAttackDamage;
        }
        else
        {
            currentAttackTimeout = controller.secondaryAttackTimeout;
            currentAttackSpeed = controller.secondaryAttackSpeed;
            currentAttackDamage = controller.secondaryAttackDamage;
        }


        animator.SetTrigger(ANIM_PARAM_ATTACK);
        animator.SetFloat(ANIM_PARAM_ATTACK_SPEED, currentAttackSpeed);

        if (environment.isAirboneState)
            controller.m_Rigidbody2D.velocity = new Vector2(controller.m_Rigidbody2D.velocity.x, controller.attackGravityCancel);

        controller.Attack(currentAttackDamage);
    }

    public override void OnUpdate()
    {

        timePassed += Time.deltaTime;

        if (timePassed > currentAttackTimeout)
            if (input.dash && controller.canDash)
                playerStateMachine.SwitchState(playerStateMachine.dashingState);
            else if (input.jump && controller.grounded || input.jump && controller.doubleJump)
                playerStateMachine.SwitchState(playerStateMachine.jumpingState);
            else if (Mathf.Abs(input.move.x) > controller.deadZone)
                playerStateMachine.SwitchState(playerStateMachine.walkingState);
            else if (Mathf.Abs(input.move.x) <= controller.deadZone)
                playerStateMachine.SwitchState(playerStateMachine.idlingState);
    }

    public override void OnExit()
    {
        input.primaryAttack = false;
        input.secondaryAttack = false;
    }
}


public class HurtState : PlayerState
{
    public HurtState(PlayerStateMachine player) : base(player) { }

    float timePassed = 0f;

    public override void OnEnter()
    {
        animator.SetTrigger(ANIM_PARAM_HIT);
        timePassed = 0f;
    }

    public override void OnUpdate()
    {
        timePassed += Time.deltaTime;

        if (timePassed > controller.hurtTimeout)
            if (input.primaryAttack || input.secondaryAttack)
                playerStateMachine.SwitchState(playerStateMachine.attackingState);
            else if (input.dash && controller.canDash)
                playerStateMachine.SwitchState(playerStateMachine.dashingState);
            else if (input.jump && controller.grounded || input.jump && controller.doubleJump)
                playerStateMachine.SwitchState(playerStateMachine.jumpingState);
            else if (Mathf.Abs(input.move.x) > controller.deadZone)
                playerStateMachine.SwitchState(playerStateMachine.walkingState);
            else if (Mathf.Abs(input.move.x) <= controller.deadZone)
                playerStateMachine.SwitchState(playerStateMachine.idlingState);

    }

    public override void OnExit()
    {

    }
}

public class DeathState : PlayerState
{
    public DeathState(PlayerStateMachine player) : base(player) { }

    float timePassed = 0f;

    public override void OnEnter()
    {
        animator.SetBool(ANIM_PARAM_IS_DEAD, true);
        timePassed = 0f;
    }

    public override void OnUpdate()
    {
        timePassed += Time.deltaTime;

        if (timePassed > controller.deathTimeout)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    public override void OnExit()
    {
    }
}