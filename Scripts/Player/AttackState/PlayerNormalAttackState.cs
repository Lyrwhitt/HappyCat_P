using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public class PlayerNormalAttackState : PlayerAttackState
{
    private bool alreadyApplyForce;
    private bool alreadyApplyCombo;

    AttackInfoData attackInfoData;

    public PlayerNormalAttackState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
    {

    }

    public override void EnterState()
    {
        base.EnterState();

        stateMachine.player.animationEventReceiver.animationEvent += DamageEnemy;
        stateMachine.player.animationEventReceiver.SetAnimationEvent(stateMachine.player.data.attackData.attackID);

        SetAnimationBool(stateMachine.player.animationData.comboAttackParameterHash, true);

        alreadyApplyForce = false;
        alreadyApplyCombo = false;

        int comboIndex = stateMachine.comboIndex;
        attackInfoData = stateMachine.player.data.attackData.GetAttackInfo(comboIndex);
        stateMachine.player.animator.SetInteger("Combo", comboIndex);

        if(comboIndex == 0)
            SetRotationForward();
    }

    public override void ExitState()
    {
        base.ExitState();

        stateMachine.player.animationEventReceiver.animationEvent -= DamageEnemy;

        SetAnimationBool(stateMachine.player.animationData.comboAttackParameterHash, false);

        // �޺��� ���� ���� ��������
        if (!alreadyApplyCombo)
            stateMachine.comboIndex = 0;
    }

    public void DamageEnemy()
    {
        // �÷��̾��� ��ġ�� ������ �������� �簢�� ����� ������ �����մϴ�.
        Vector3 boxCenter = attackInfoData.hitBoxCenterOffset + stateMachine.player.transform.position + stateMachine.player.transform.forward *
            attackInfoData.hitBox.z / 2f;
        Quaternion boxRotation = Quaternion.LookRotation(stateMachine.player.transform.forward);

        stateMachine.player.testGizmo = new Test(true, attackInfoData.hitBox, boxCenter, boxRotation);

        // OverlapBox �Լ��� ���� �簢�� ����� ���� �ȿ� �ִ� ���� �����մϴ�.
        Collider[] colliders = Physics.OverlapBox(boxCenter, attackInfoData.hitBox / 2f, boxRotation);

        // ������ �浹�� ������Ʈ�� ���� ó���մϴ�.
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                if(collider.transform.TryGetComponent(out DamageReceiver damageReceiver))
                {
                    Vector3 force;
                    EffectManager.Instance.PlayEffect("PunchEffect", boxCenter);

                    if (!damageReceiver.isAirborne)
                    {
                        force = stateMachine.player.transform.forward;
                        damageReceiver.Damage(attackInfoData.damage, force);
                        damageReceiver.Stagger(0.5f);
                    }
                    else
                    {
                        force = stateMachine.player.transform.forward * 3f + collider.transform.up * 12f;
                        damageReceiver.Damage(attackInfoData.damage, force);
                    }
                }
            }
        }
    }

    private void TryComboAttack()
    { 
        if (alreadyApplyCombo) return;
        // ������ ���ݱ��� ���� ��
        if (attackInfoData.comboStateIndex == -1) return;
        if (!stateMachine.isAttacking) return;

        alreadyApplyCombo = true;
    }

    private void TryApplyForce()
    {
        if(alreadyApplyForce) return;

        alreadyApplyForce = true;

        stateMachine.player.forceReceiver.ResetForceReceiver();

        // ��������
        stateMachine.player.forceReceiver.AddForce(stateMachine.player.transform.forward * attackInfoData.force);
        stateMachine.player.forceReceiver.drag = attackInfoData.drag;
    }

    public override void UpdateState()
    {
        //base.UpdateState();

        ForceMove();

        float normalizedTime = GetNormalizedTime(stateMachine.player.animator, "Attack");

        if (normalizedTime < 1f)
        {
            // ������ Ʈ������ Ÿ���� ���� �� ��, �޺� ����
            if (normalizedTime >= attackInfoData.forceTransitionTime)
                TryApplyForce();

            if (normalizedTime >= attackInfoData.comboTransitionTime)
            {
                TryComboAttack();

                if (alreadyApplyCombo)
                {
                    // �޺��� �����ϴ� �� (AttackData�� �� ������ ���� ������ �ε����� �������ִ�)
                    stateMachine.comboIndex = attackInfoData.comboStateIndex;
                    stateMachine.ChangeState(stateMachine.normalAttackState);
                }
            }
        }
        // ��� �ٺ��� ����
        else
        {
            stateMachine.ChangeState(stateMachine.idleState);
        }
    }
}
