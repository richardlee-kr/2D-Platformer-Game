using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : Entity
{
    public BE_IdleState idleState { get; private set; }
    public BE_MoveState moveState { get; private set; }
    public BE_PlayerDetectedState playerDetectedState { get; private set; }
    public BE_ChargeState chargeState { get; private set; }
    public BE_LookForPlayerState lookForPlayerState { get; private set; }
    public BE_MeleeAttackState meleeAttackState { get; private set; }
    public BE_StunState stunState { get; private set; }
    public BE_DeadState deadState { get; private set; }

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

        moveState = new BE_MoveState(this, stateMachine, "move", moveStateData, this);
        idleState = new BE_IdleState(this, stateMachine, "idle", idleStateData, this);
        playerDetectedState = new BE_PlayerDetectedState(this, stateMachine, "playerDetected", playerDetectedData, this);
        chargeState = new BE_ChargeState(this, stateMachine, "charge", chargeStateData, this);
        lookForPlayerState = new BE_LookForPlayerState(this, stateMachine, "lookForPlayer", lookForPlayerStateData, this);
        meleeAttackState = new BE_MeleeAttackState(this, stateMachine, "meleeAttack", meleeAttackPosition, meleeAttackStateData, this);
        stunState = new BE_StunState(this, stateMachine, "stun", stunStateData, this);
        deadState = new BE_DeadState(this, stateMachine, "dead", deadStateData, this);

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
            stateMachine.ChangeState(deadState);
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
