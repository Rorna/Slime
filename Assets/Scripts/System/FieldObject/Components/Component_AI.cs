using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateEnum
{
    IdleStatic,
    IdlePatrol,
    Chase,
    Attack,
    None,
    Count
}

public enum EnemyAttackTypeEnum
{
    Melee,
    Ranged,
    Summon,
    None,
    Count
}

public partial class Component_AI : FieldObject_Components
{
    private NavMeshAgent m_agent;
    private AIStateEnum m_idleState = AIStateEnum.IdlePatrol;
    private AIStateEnum m_currentState = AIStateEnum.None;

    private delegate void InitStateDelegate();

    private delegate void UpdateStateDelegate();

    private delegate void EndStateDelegate();

    private InitStateDelegate m_initStateDelegate;
    private UpdateStateDelegate m_updateStateDelegate;
    private EndStateDelegate m_endStateDelegate;

    private Vector3 m_startPos = Vector3.zero;
    private Vector3 m_targetPos;

    private Timer m_chaseTimer = new Timer();
    private bool m_isStartChase;
    private PatrolNode m_patrolNode;

    private bool m_isStopAI = false;

    public Component_AI(FieldObject obj) : base(obj)
    {
    }

    public override void Close()
    {
    }

    public override void InitComponent()
    {
        InitSensor();
        InitAgent();
        m_startPos = m_obj.GO.transform.position;
    }

    //init Ext variable
    private EnemyAttackTypeEnum m_enemyAttackType;

    private List<Vector3> m_AIRotateDirList;
    private bool m_hasWeapon;
    private string m_weaponName;

    public EnemyAttackTypeEnum EnemyAttackType => m_enemyAttackType;

    public void SetEnemyAttackType(EnemyAttackTypeEnum attackType)
    {
        if (attackType == EnemyAttackTypeEnum.None || m_enemyAttackType == attackType)
            return;

        if (attackType == EnemyAttackTypeEnum.Melee && m_hasWeapon)
        {
            m_obj.Equip.OnUnequipItem();
        }

        m_enemyAttackType = attackType;
    }

    public void InitExtData(FieldManager.EnemyInfo enemyInfo, FieldObject itemObj = null)
    {
        m_enemyAttackType = enemyInfo.EnemyAttackType;
        m_idleState = enemyInfo.AIState;
        m_AIRotateDirList = enemyInfo.AIRotateDirList;
        m_weaponName = enemyInfo.WeaponName;
        SetBossData(enemyInfo.AttackTypeList);

        m_hasWeapon = enemyInfo.HasWeapon;
        if (m_hasWeapon && itemObj.IsNotNull())
        {
            m_obj.Equip.EnemyEquip(itemObj.ObjName);
        }

        SetAIState(m_idleState);
    }

    private void InitAgent()
    {
        m_agent = m_obj.GO.transform.GetComponent<NavMeshAgent>();
        m_agent.updateUpAxis = false;
        m_agent.updateRotation = false;
        m_agent.acceleration = 999f;
        m_agent.stoppingDistance = 0f;
    }

    public void ComponentUpdate()
    {
        if (m_obj.IsDead)
        {
            m_agent.velocity = Vector3.zero;
            m_agent.enabled = false;
            return;
        }

        if (m_idleState == AIStateEnum.None)
            return;

        if (m_isStopAI)
        {
            if (m_obj.FieldObjectState != FieldObjectStateEnum.Stun)
                m_obj.SetObjectState(FieldObjectStateEnum.Idle);

            return;
        }

        if (m_obj.FieldObjectState == FieldObjectStateEnum.Stun)
            return;

        UpdateSensor();
        m_updateStateDelegate();
    }

    public void StopAI()
    {
        if (m_idleState != AIStateEnum.None)
        {
            m_agent.isStopped = true;
            m_agent.velocity = Vector3.zero;
            m_agent.enabled = false;
            m_isStopAI = true;
        }

        var legObj = m_obj.Leg;
        if (legObj.IsNotNull())
        {
            legObj.SetActive(false);
        }

        var boxColl = m_obj.GetBoxCollider();
        boxColl.enabled = false;
    }

    public void SetStun(float duringTime = 0f)
    {
        PauseAI(duringTime, FieldObjectStateEnum.Stun);
    }

    public void PauseAI(float duringTime, FieldObjectStateEnum state = FieldObjectStateEnum.None)
    {
        if (m_obj.IsDead)
            return;

        if (m_isStopAI)
            return;

        //init timer set callback
        var cmd = new FieldObjectTimerCommand(m_obj);
        cmd.Init(duringTime);
        cmd.SetCallBack(ResumeAI);

        if (state != FieldObjectStateEnum.None)
            m_obj.SetObjectState(state);

        if (m_idleState == AIStateEnum.None)
            return;

        //end Attack
        m_obj.Attack.EndAttack();

        m_isStopAI = true;

        m_agent.velocity = Vector3.zero;
        m_agent.isStopped = true;
        m_agent.SetDestination(m_obj.GO.transform.position); //can't move anymore
    }

    private void ResumeAI()
    {
        if (m_obj.IsDead)
            return;

        m_obj.SetObjectState(FieldObjectStateEnum.Idle);

        if (m_idleState == AIStateEnum.None)
            return;

        m_isStopAI = false;
        m_agent.isStopped = false;
        SetAIState(m_idleState);
    }

    public void SetAIState(AIStateEnum aiState)
    {
        if (m_currentState == aiState)
            return;

        if (m_endStateDelegate.IsNotNull())
            m_endStateDelegate();

        switch (aiState)
        {
            case AIStateEnum.IdleStatic:
                {
                    m_initStateDelegate = InitState_IdleStatic;
                    m_updateStateDelegate = UpdateState_IdleStatic;
                    m_endStateDelegate = EndState_IdleStatic;

                    m_obj.SetObjectState(FieldObjectStateEnum.Idle);
                }
                break;

            case AIStateEnum.IdlePatrol:
                {
                    m_initStateDelegate = InitState_IdlePatrol;
                    m_updateStateDelegate = UpdateState_IdlePatrol;
                    m_endStateDelegate = EndState_IdlePatrol;

                    m_obj.SetObjectState(FieldObjectStateEnum.Move);
                }
                break;

            case AIStateEnum.Chase:
                {
                    m_initStateDelegate = InitState_Chase;
                    m_updateStateDelegate = UpdateState_Chase;
                    m_endStateDelegate = EndState_Chase;

                    m_obj.SetObjectState(FieldObjectStateEnum.Move);
                }
                break;

            case AIStateEnum.Attack:
                {
                    m_initStateDelegate = InitState_Attack;
                    m_updateStateDelegate = UpdateState_Attack;
                    m_endStateDelegate = EndState_Attack;
                }
                break;
        }

        m_initStateDelegate();
        m_currentState = aiState;
    }

    ////////////////////// idleStatic
    private Timer m_rotateTimer = new Timer();

    private int m_rotateIndex = 0;

    private void InitState_IdleStatic()
    {
        if (m_agent.isActiveAndEnabled == false)
            return;

        m_rotateTimer.Init(2.0f, true);
        m_rotateTimer.SetCallBack(UpdateRotate);

        m_agent.isStopped = false;
        m_agent.SetDestination(m_startPos);
    }

    private void UpdateRotate()
    {
        Vector3 rotateDir = m_AIRotateDirList[m_rotateIndex];
        if (m_rotateIndex == m_AIRotateDirList.Count - 1)
        {
            m_rotateIndex = 0;
        }
        else
        {
            m_rotateIndex++;
        }

        m_obj.RotateObjectAngle(rotateDir);
    }

    private void UpdateState_IdleStatic()
    {
        if (HasReachedDestination(0.2f) == false)
        {
            Vector3 direction = (m_startPos - m_obj.GO.transform.position).normalized;
            m_obj.RotateObjectAngle(direction);

            m_obj.SetObjectState(FieldObjectStateEnum.Move);
        }
        else
        {
            m_rotateTimer.UpdateTimer();

            if (m_obj.FieldObjectState != FieldObjectStateEnum.Stun)
                m_obj.SetObjectState(FieldObjectStateEnum.Idle);
        }
    }

    private void EndState_IdleStatic()
    {
        m_rotateTimer.StopTimer();
    }

    // IdlePatrol
    private void InitState_IdlePatrol()
    {
        m_agent.isStopped = false;

        //search Patrol Node
        if (m_patrolNode.IsNull())
        {
            if (GetPatrolNode() == false)
            {
                SetAIState(AIStateEnum.IdleStatic);
                return;
            }
        }

        m_agent.speed = 2.0f;
        m_agent.SetDestination(m_patrolNode.GetPosition());
    }

    private void UpdateState_IdlePatrol()
    {
        if (HasReachedDestination(0.1f))
        {
            if (m_obj.FieldObjectState != FieldObjectStateEnum.Stun)
                m_obj.SetObjectState(FieldObjectStateEnum.Idle);

            m_patrolNode = m_patrolNode.m_nextNode;
            m_agent.SetDestination(m_patrolNode.GetPosition());
        }
        else
        {
            Vector3 direction = (m_patrolNode.GetPosition() - m_obj.GO.transform.position).normalized;
            m_obj.RotateObjectAngle(direction);
            m_obj.SetObjectState(FieldObjectStateEnum.Move);
        }
    }

    private void EndState_IdlePatrol()
    {
    }

    private bool GetPatrolNode()
    {
        LayerMask layerMask = 1 << LayerMask.NameToLayer("PatrolNode");
        Collider2D[] colliderArr = Physics2D.OverlapCircleAll(m_obj.GO.transform.position, m_sightDistance, layerMask);
        foreach (var obj in colliderArr)
        {
            var node = obj.gameObject.GetComponent<PatrolNode>();
            if (node.IsNull())
                continue;

            m_patrolNode = node;
            return true;
        }

        return false;
    }

    ////////////////////// chase

    private void InitState_Chase()
    {
        if (m_agent.isActiveAndEnabled == false)
            return;

        m_agent.speed = 4.0f;
        m_agent.isStopped = false;
        m_isStartChase = false;
        m_chaseTimer.StopTimer();
    }

    private void SetIdleState()
    {
        SetAIState(m_idleState);
    }

    private void UpdateState_Chase()
    {
        if (m_obj.FieldObjectState == FieldObjectStateEnum.Stun)
            return;

        if (HasReachedDestination(0.4f))
        {
            if (m_isStartChase == false)
            {
                m_isStartChase = true;
                float chaseTime = m_obj.ObjectType == FieldObjectTypeEnum.BossEnemy ? 120.0f : 2.0f;
                m_chaseTimer.Init(3.0f);
                m_chaseTimer.SetCallBack(SetIdleState);
                return;
            }

            if (m_obj.FieldObjectState != FieldObjectStateEnum.Stun)
                m_obj.SetObjectState(FieldObjectStateEnum.Idle);
        }
        else
        {
            m_obj.SetObjectState(FieldObjectStateEnum.Move);
        }

        m_agent.SetDestination(m_targetPos);
        OnAttack();

        Vector3 direction = (m_targetPos - m_obj.GO.transform.position).normalized;
        m_obj.RotateObjectAngle(direction);

        if (m_isStartChase)
            m_chaseTimer.UpdateTimer();
    }

    private void EndState_Chase()
    {
    }

    ////////////////////// attack

    private void InitState_Attack()
    {
        m_agent.velocity = Vector3.zero;
        m_agent.isStopped = true;
    }

    private void UpdateState_Attack()
    {
        if (m_obj.FieldObjectState == FieldObjectStateEnum.Stun)
            return;

        m_obj.Attack.OnAttack();
    }

    private void EndState_Attack()
    {
    }

    public FieldObject TargetObj { get; private set; }

    private void SetTargetObject(FieldObject targetObj)
    {
        TargetObj = targetObj;
    }

    private void SetTargetPosition(Vector3 pos)
    {
        m_targetPos = pos;
        if (m_currentState != AIStateEnum.Attack)
        {
            SetAIState(AIStateEnum.Chase);
        }
    }

    public bool HasReachedDestination(float minDist = 1.5f)
    {
        float dist = Vector2.Distance(m_obj.GO.transform.position, m_agent.destination);
        if (dist <= minDist)
        {
            return true;
        }

        return false;
    }

    private void OnAttack()
    {
        const int raycastHitSize = 2;
        RaycastHit2D[] hitArr = new RaycastHit2D[raycastHitSize];

        LayerMask hitLayerMask = m_obj.GetDefaultLayerMask();

        //Set Attack Range
        float attackDistance = 0f;
        switch (m_enemyAttackType)
        {
            case EnemyAttackTypeEnum.Melee:
                {
                    var bounds = m_obj.GetBoxCollider();
                    var collSize = bounds.size;
                    attackDistance = MathF.Abs(collSize.x);
                }
                break;

            case EnemyAttackTypeEnum.Ranged:
            case EnemyAttackTypeEnum.Summon:
                {
                    attackDistance = m_obj.StatDic[StatValueEnum.AttackRange];
                }
                break;
        }

        Physics2D.RaycastNonAlloc(m_obj.GO.transform.position, m_obj.LookDir, hitArr, attackDistance, hitLayerMask);
        foreach (var hitObj in hitArr)
        {
            if (hitObj.IsNull())
                continue;

            if (hitObj.collider.IsNull())
                continue;

            if (hitObj.collider.gameObject == m_obj.Body)
                continue;

            string objName = UnityUtil.GetObjectName(hitObj.collider.gameObject);
            var fieldObj = FieldManager.Instance.GetFieldObject(objName);
            if (fieldObj.IsNull())
                continue;

            if (fieldObj.ObjectType == FieldObjectTypeEnum.Player)
            {
                SetAIState(AIStateEnum.Attack);
            }
        }
    }

    public void EndAttack()
    {
        if (m_obj.FieldObjectState == FieldObjectStateEnum.Stun)
            return;

        SetAIState(AIStateEnum.Chase);
    }

    public void ReceivePosition(Vector3 pos)
    {
        if (m_idleState == AIStateEnum.None)
            return;

        SetTargetPosition(pos);
    }
}