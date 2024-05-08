using UnityEngine;

public class Component_Animation : FieldObject_Components
{
    private Animator m_bodyAnimator;
    private Animator m_legAnimator;

    private int m_currentBodyLayerIndex;
    private string m_currentBodyLayerName;

    public Animator BodyAnimator => m_bodyAnimator;
    public Animator LegAnimator => m_legAnimator;

    public Component_Animation(FieldObject obj) : base(obj)
    {
    }

    public override void InitComponent()
    {
        SetAnimator();
    }

    public void ComponentUpdate()
    {
        UpdateAnimationByState();
    }

    public override void Close()
    {
    }

    private void UpdateAnimationByState()
    {
        switch (m_obj.ObjectType)
        {
            case FieldObjectTypeEnum.Player:
                {
                    UpdatePlayerAnimationByState();
                }
                break;

            case FieldObjectTypeEnum.Enemy:
            case FieldObjectTypeEnum.BossEnemy:
                {
                    UpdateEnemyAnimationByState();
                }
                break;

            case FieldObjectTypeEnum.Item:
                {
                    UpdateItemAnimationByState();
                }
                break;
        }
    }

    private void UpdatePlayerAnimationByState()
    {
        if (m_obj.IsPlayer == false)
            return;

        if (m_bodyAnimator.IsNull())
            return;

        string bodyState = m_obj.FieldObjectState.ToString();

        if (m_obj.FieldObjectState == FieldObjectStateEnum.MovingAttack)
        {
            bodyState = DefineStrings.Attack;
        }

        PlayAnimation(m_bodyAnimator, bodyState);
    }

    private void UpdateEnemyAnimationByState()
    {
        if (m_obj.IsEnemy == false)
            return;

        if (m_legAnimator.IsNull() && m_bodyAnimator.IsNull())
            return;

        string bodyState = m_obj.FieldObjectState.ToString();
        string legState = m_obj.FieldObjectState.ToString();

        if (m_obj.FieldObjectState == FieldObjectStateEnum.Attack)
            legState = DefineStrings.Idle;

        if (m_obj.FieldObjectState == FieldObjectStateEnum.MovingAttack)
        {
            bodyState = DefineStrings.Attack;
            legState = DefineStrings.Move;
        }

        PlayAnimation(m_bodyAnimator, bodyState);
        PlayAnimation(m_legAnimator, legState, DefineStrings.Default);
    }

    private void UpdateItemAnimationByState()
    {
        if (m_obj.IsItem == false)
            return;

        if (m_bodyAnimator.IsNull())
            return;

        string state = m_obj.FieldObjectState.ToString();
        PlayAnimation(m_bodyAnimator, state, DefineStrings.Default);
    }

    public void PlayAnimation(string animName)
    {
        m_bodyAnimator.Play(animName);
        if (m_legAnimator.IsNotNull())
            m_legAnimator.Play(animName);
    }

    public void PlayAnimation(Animator animator, string animName, string layerName = "")
    {
        if (animator.isActiveAndEnabled == false)
            return;

        if (layerName == string.Empty)
        {
            animator.Play(animName, m_currentBodyLayerIndex);
            return;
        }

        int layerIndex = animator.GetLayerIndex(layerName);
        animator.Play(animName, layerIndex);
    }

    private bool HasLayer(Animator animator, string layerName)
    {
        int index = animator.GetLayerIndex(layerName);
        return index != -1;
    }

    private void SetAnimator()
    {
        m_bodyAnimator = m_obj.Body.GetComponent<Animator>();
        if (m_obj.Leg.IsNotNull())
            m_legAnimator = m_obj.Leg.GetComponent<Animator>();

        m_currentBodyLayerIndex = m_bodyAnimator.GetLayerIndex(DefineStrings.Default);
        m_currentBodyLayerName = m_bodyAnimator.GetLayerName(m_currentBodyLayerIndex);
    }

    private void SetPlayerAnimator()
    {
        m_bodyAnimator = m_obj.Body.GetComponent<Animator>();
        m_legAnimator = m_obj.Leg.GetComponent<Animator>();

        m_currentBodyLayerIndex = m_bodyAnimator.GetLayerIndex(DefineStrings.Default);
        m_currentBodyLayerName = m_bodyAnimator.GetLayerName(m_currentBodyLayerIndex);
    }

    public void ChangeAnimatorLayer(string layerName)
    {
        int layerIndex = m_bodyAnimator.GetLayerIndex(layerName);

        m_bodyAnimator.SetLayerWeight(m_currentBodyLayerIndex, 0f);
        m_bodyAnimator.SetLayerWeight(layerIndex, 1f);

        m_currentBodyLayerIndex = layerIndex;
        m_currentBodyLayerName = layerName;
    }

    public void StopAnimation(Animator animator)
    {
        animator.enabled = false;
    }
}