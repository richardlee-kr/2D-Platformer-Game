using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minotaur : Entity
{
    public Mino_IdleState idleState { get; private set; }
    public Mino_MoveState moveState { get; private set; }
    public Mino_PlayerDetectedState playerDetectedState { get; private set; }
    public Mino_ChargeState chargeState { get; private set; }
    public Mino_LookForPlayerState lookForPlayerState { get; private set; }
    public Mino_MeleeAttackState meleeAttackState { get; private set; }
    public Mino_StunState stunState { get; private set; }
    public Mino_DeadState deadState { get; private set; }

    [SerializeField] private D_IdleState idleStateData;
    [SerializeField] private D_MoveState moveStateData;
    [SerializeField] private D_PlayerDetected playerDetectedData;
    [SerializeField] private D_ChargeState chargeStateData;
    [SerializeField] private D_LookForPlayer lookForPlayerStateData;
    [SerializeField] private D_MeleeAttack meleeAttackStateData;
    [SerializeField] private D_StunState stunStateData;
    [SerializeField] private D_DeadState deadStateData;

    [SerializeField] private Transform meleeAttackPosition;

    public override void Start()
    {
        base.Start();

        moveState = new Mino_MoveState(this, stateMachine, "move", moveStateData, this);
        idleState = new Mino_IdleState(this, stateMachine, "idle", idleStateData, this);
        playerDetectedState = new Mino_PlayerDetectedState(this, stateMachine, "playerDetected", playerDetectedData, this);
        chargeState = new Mino_ChargeState(this, stateMachine, "charge", chargeStateData, this);
        lookForPlayerState = new Mino_LookForPlayerState(this, stateMachine, "lookForPlayer", lookForPlayerStateData, this);
        meleeAttackState = new Mino_MeleeAttackState(this, stateMachine, "meleeAttack", meleeAttackPosition, meleeAttackStateData, this);
        stunState = new Mino_StunState(this, stateMachine, "stun", stunStateData, this);
        deadState = new Mino_DeadState(this, stateMachine, "dead", deadStateData, this);

        stateMachine.Initialize(moveState);
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawWireSphere(meleeAttackPosition.position, meleeAttackStateData.attackRadius);
    }

    public override void Damage(AttackDetails attackDetails)
    {
        base.Damage(attackDetails);


        if(isDead)
        {
            anim.SetBool("dead", true);
            Destroy(gameObject, 3f);
        }
        else if (isStunned && stateMachine.currentState != stunState)
        {
            stateMachine.ChangeState(stunState);
        }
        else if(!CheckPlayerInMinAgroRange())
        {
            lookForPlayerState.SetTurnImmediately(true);
            stateMachine.ChangeState(lookForPlayerState);
        }
    }

}
