using System;
using System.Collections.Generic;
using UnityEngine;

public class Component_Attack : FieldObject_Components
{
    private FieldObject m_contactObject;

    private bool m_isPlayingAttack = false;
    private Collider2D m_collider;
    private LayerMask m_targetLayerMask;
    private bool m_runThrow;
    private string m_equipItemName;

    //summon attack Variable
    private Stack<FieldObject> m_summonStack = new Stack<FieldObject>();
    private bool m_isCompleteSummon;
    private bool m_isSummon = false;
    private int m_currentSummonCount = 0;
    private Timer m_summonTimer;

    public bool IsPlayingAttack => m_isPlayingAttack;

    public Component_Attack(FieldObject obj) : base(obj)
    {
    }

    public override void InitComponent()
    {
        m_collider = m_obj.GetBoxCollider();
        if (m_obj.IsPlayer)
            m_targetLayerMask = 1 << LayerMask.NameToLayer(DefineStrings.Enemy);
        else
        {
            m_targetLayerMask = 1 << LayerMask.NameToLayer(DefineStrings.Player);
        }
    }

    public void ComponentUpdate()
    {
        if (m_obj.IsDead)
            return;

        if (m_isPlayingAttack == false)
            return;

        if (m_obj.FieldObjectState != FieldObjectStateEnum.Attack &&
            m_obj.FieldObjectState != FieldObjectStateEnum.MovingAttack)
            return;

        UpdateBossSummonObjectAttack();

        if (m_contactObject.IsNotNull())
            return;

        if (m_obj.HasContactObject(out GameObject obj, m_targetLayerMask))
        {
            string objName = UnityUtil.GetObjectName(obj);
            var fieldObj = FieldManager.Instance.GetFieldObject(objName);
            if (fieldObj.IsNull())
                return;

            OnTakeDamage(fieldObj);
        }
    }

    public void ComponentFixedUpdate()
    {
        if (m_obj.IsDead)
            return;

        ThrowAttack();
    }

    public void OnTakeDamage(FieldObject obj)
    {
        if (IsAttackable(obj) == false)
            return;

        //play sound
        FieldObject sourceObj = new FieldObject();
        sourceObj = m_obj.Equip.HasItem ? m_obj.Equip.CurrentEquipItem : m_obj;
        SoundManager.Instance.PlayEffectAudio(sourceObj, DefineStrings.Hit);

        TakeDamage();
    }

    private bool IsAttackable(FieldObject obj)
    {
        if (obj.IsDead)
            return false;

        if (obj.ObjectType == m_obj.ObjectType)
            return false;

        if (obj.LightAreaState == LightAreaStateEnum.Light &&
            obj.FieldObjectState == FieldObjectStateEnum.Dodge)
            return false;

        m_contactObject = obj;
        return true;
    }

    private void TakeDamage()
    {
        if (m_contactObject.IsNull())
            return;

        if (m_contactObject.FieldObjectState == FieldObjectStateEnum.Invincible)
            return;

        if (m_contactObject.FieldObjectState == FieldObjectStateEnum.Stun)
            return;

        int currentHP = m_contactObject.CurrentHP;
        m_contactObject.SetStat(StatValueEnum.CurrentHP, --currentHP);

        CameraManager.Instance.Shake(0.2f, 0.7f);
        ClearContactData();
    }

    public void OnThrowAttack()
    {
        m_equipItemName = m_obj.Equip.ItemName;

        if (m_obj.Equip.OnUnequipItem() == false)
            return;

        m_runThrow = true;
    }

    private void ThrowAttack()
    {
        if (m_runThrow == false)
            return;

        var itemObj = FieldManager.Instance.GetFieldObject(m_equipItemName);
        itemObj.Item.UpdateItemAttackInfos(m_obj);
        itemObj.Move.SetMoveDir(m_obj.LookDir.normalized);

        var itemRigid = itemObj.GetRigidbody();
        float throwSpeed = m_obj.StatDic[StatValueEnum.ThrowSpeed];

        itemRigid.AddForce(m_obj.LookDir.normalized * throwSpeed, ForceMode2D.Impulse);

        SoundManager.Instance.PlayEffectAudio(m_obj, DefineStrings.ThrowAttack);
        m_runThrow = false;
    }

    private void ClearContactData()
    {
        m_contactObject = null;
    }

    public void OnAttack()
    {
        if (m_obj.IsDead)
            return;

        if (m_obj.IsPlayer && m_obj.Equip.HasItem)
        {
            var item = m_obj.Equip.CurrentEquipItem;
            if (item.Item.ItemType == ItemTypeEnum.Slime)
            {
                StartRangedAttack();
                return;
            }
        }

        if (m_obj.IsEnemy)
        {
            switch (m_obj.AI.EnemyAttackType)
            {
                case EnemyAttackTypeEnum.Melee:
                    StartMeleeAttack();
                    break;

                case EnemyAttackTypeEnum.Ranged:
                    StartRangedAttack();
                    break;

                case EnemyAttackTypeEnum.Summon:
                    StartBossSummonObjectAttack();
                    break;
            }
        }
    }

    private void StartMeleeAttack()
    {
        var cmd = m_obj.CommandManager.GetCmd<FieldObjectTimerCommand>((int)FieldObjectCmdType.FieldObjectAttackTimer);
        if (cmd.IsNotNull())
            return;

        var bodyAnim = m_obj.Anim.BodyAnimator;
        m_obj.Anim.PlayAnimation(bodyAnim, FieldObjectStateEnum.Idle.ToString());

        cmd = new FieldObjectTimerCommand(m_obj);
        float attackDelayTime = m_obj.StatDic[StatValueEnum.AttackDelay];
        cmd.Init(attackDelayTime);
        cmd.SetCallBack(EndAttack);

        m_isPlayingAttack = true;

        var attackState = FieldObjectStateEnum.Attack;
        if (m_obj.FieldObjectState == FieldObjectStateEnum.Move)
            attackState = FieldObjectStateEnum.MovingAttack;

        m_obj.SetObjectState(attackState);
        m_obj.Shape.UpdateAttackColliderSize();

        string effectAudioName = DefineStrings.Attack;
        if (m_obj.Equip.HasItem)
        {
            effectAudioName = UnityUtil.RemoveGuid(m_obj.Equip.ItemName);
        }

        SoundManager.Instance.PlayEffectAudio(m_obj, effectAudioName);
    }

    private void StartRangedAttack()
    {
        var cmd = m_obj.CommandManager.GetCmd<FieldObjectTimerCommand>((int)FieldObjectCmdType.FieldObjectAttackTimer);
        if (cmd.IsNotNull())
            return;

        var bodyAnim = m_obj.Anim.BodyAnimator;
        m_obj.Anim.PlayAnimation(bodyAnim, FieldObjectStateEnum.Idle.ToString());

        cmd = new FieldObjectTimerCommand(m_obj);
        float attackDelayTime = m_obj.StatDic[StatValueEnum.AttackDelay];
        cmd.Init(attackDelayTime);
        cmd.SetCallBack(EndAttack);

        m_isPlayingAttack = true;

        var attackState = FieldObjectStateEnum.Attack;
        if (m_obj.FieldObjectState == FieldObjectStateEnum.Move)
            attackState = FieldObjectStateEnum.MovingAttack;

        m_obj.SetObjectState(attackState);
        m_obj.Shape.UpdateAttackColliderSize();

        CreateBullet();

        string itemName;
        if (m_obj.Equip.HasItem == false)
        {
            itemName = "WeaponRifle";
        }
        else
        {
            itemName = UnityUtil.RemoveGuid(m_obj.Equip.ItemName);
        }

        SoundManager.Instance.PlayEffectAudio(m_obj, itemName);

        //alert, destroy slime
        if (m_obj.IsPlayer)
        {
            BroadcastPosition(m_obj.GO.transform.position, FieldObjectTypeEnum.Enemy, 10f);
            var slimeItem = m_obj.Equip.CurrentEquipItem;
            m_obj.Equip.OnUnequipItem();
            FieldManager.Instance.DestroyFieldObject(slimeItem);
        }
    }

    private void CreateBullet()
    {
        var equipItem = m_obj.Equip.CurrentEquipItem;
        var firePoint = m_obj.GetFirePoint();
        var bulletObj = new FieldObject();

        switch (equipItem.Item.ItemType)
        {
            case ItemTypeEnum.Ranged:
                {
                    bulletObj = FieldManager.Instance.CreateObject(FieldObjectTypeEnum.Item,
                        "Bullet", firePoint.transform.position, Quaternion.identity);
                }
                break;

            case ItemTypeEnum.Slime:
                {
                    bulletObj = FieldManager.Instance.CreateObject(FieldObjectTypeEnum.Item,
                        "SlimeBullet", firePoint.transform.position, Quaternion.identity);
                }
                break;
        }

        bulletObj.Anim.StopAnimation(bulletObj.Anim.BodyAnimator);
        bulletObj.Item.UpdateItemAttackInfos(m_obj);

        m_obj.RotateObjectAngle(bulletObj.GO, m_obj.LookDir);
        var itemRigid = bulletObj.GetRigidbody();
        float bulletSpeed = DefineTable.GetFloat(DefineValueEnums.BulletSpeed);

        itemRigid.AddForce(m_obj.LookDir.normalized * bulletSpeed, ForceMode2D.Impulse);
    }

    private void BroadcastPosition(Vector3 pos, FieldObjectTypeEnum objType, float radius)
    {
        //boss prevent chasing.
        if (m_obj.IsObjectInBossArea())
            return;

        string type = Enum.GetName(typeof(FieldObjectTypeEnum), objType);
        LayerMask layer = 1 << LayerMask.NameToLayer(type);
        Collider2D[] contactObjArr = Physics2D.OverlapCircleAll(pos, radius, layer);
        foreach (var obj in contactObjArr)
        {
            string objName = UnityUtil.GetObjectName(obj.gameObject);
            var fieldObj = FieldManager.Instance.GetFieldObject(objName);
            if (fieldObj.IsNull())
                continue;

            fieldObj.ReceivePosition(pos);
        }
    }

    //summon attack
    private void StartBossSummonObjectAttack()
    {
        if (m_obj.AI.TargetObj.IsNull())
            return;

        if (m_isPlayingAttack)
            return;

        if (m_obj.IsBossEnemy == false && m_obj.AI.BossAreaCollider.IsNull())
            return; 

        m_isPlayingAttack = true;

        var attackState = FieldObjectStateEnum.Attack;

        if (m_obj.FieldObjectState == FieldObjectStateEnum.Move)
            attackState = FieldObjectStateEnum.MovingAttack;

        m_obj.SetObjectState(attackState);

        m_isCompleteSummon = false;
        m_isSummon = true;

        m_summonStack.Clear();

        m_summonTimer = new Timer();
        m_summonTimer.Init(0.3f, true);
        m_summonTimer.SetCallBack(BossSummonObject);
    }

    private void UpdateBossSummonObjectAttack()
    {
        if (m_isSummon == false)
            return;

        if (m_summonTimer.IsFinished == false)
            m_summonTimer.UpdateTimer();

        if (m_isCompleteSummon && m_summonTimer.IsFinished)
        {
            var cmd = m_obj.CommandManager.GetCmd<FieldObjectTimerCommand>((int)FieldObjectCmdType.FieldObjectAttackTimer);
            if (cmd.IsNotNull())
                return;

            cmd = new FieldObjectTimerCommand(m_obj);
            float attackDelayTime = 1.0f;
            cmd.Init(attackDelayTime);
            cmd.SetCallBack(ApplyForceToAllItems);
        }
    }

    private void ApplyForceToAllItems()
    {
        while (m_summonStack.Count > 0)
        {
            var obj = m_summonStack.Pop();
            if (obj.IsNotNull())
            {
                var rigid = obj.GO.GetComponent<Rigidbody2D>();
                if (rigid.IsNotNull())
                {
                    obj.Item.UpdateItemAttackInfos(m_obj);
                    var dir = (m_obj.AI.TargetObj.GO.transform.position - obj.GO.transform.position).normalized;
                    rigid.AddForce(dir * m_obj.StatDic[StatValueEnum.ThrowSpeed], ForceMode2D.Impulse);
                }
            }
        }

        if (m_summonStack.Count == 0)
        {
            m_summonTimer.Exit();
            m_summonTimer = null;

            m_isCompleteSummon = false;
            m_isSummon = false;

            EndAttack();
        }

        SoundManager.Instance.PlayEffectAudio("SummonAttack");
    }

    private void BossSummonObject()
    {
        if (m_currentSummonCount >= m_obj.StatDic[StatValueEnum.MaxSummonCount])
        {
            m_isCompleteSummon = true;
            m_currentSummonCount = 0;
            m_summonTimer.StopTimer();

            return;
        }

        Vector2 spawnPosition = new Vector2(
            UnityEngine.Random.Range(m_obj.AI.BossAreaCollider.bounds.min.x, m_obj.AI.BossAreaCollider.bounds.max.x),
            UnityEngine.Random.Range(m_obj.AI.BossAreaCollider.bounds.min.y, m_obj.AI.BossAreaCollider.bounds.max.y)
        );

        var obj = FieldManager.Instance.CreateObject(FieldObjectTypeEnum.Item, "ItemLight", spawnPosition, Quaternion.identity);

        m_summonStack.Push(obj);
        m_currentSummonCount++;

        SoundManager.Instance.PlayEffectAudio("Summon");
    }

    public void EndAttack()
    {
        m_isPlayingAttack = false;

        if (m_obj.FieldObjectState != FieldObjectStateEnum.Stun)
            m_obj.SetObjectState(FieldObjectStateEnum.Idle);

        m_obj.Shape.UpdateEquipColliderSize();

        if (m_obj.ObjectType == FieldObjectTypeEnum.Enemy ||
            m_obj.ObjectType == FieldObjectTypeEnum.BossEnemy)
            m_obj.AI.EndAttack();

        var bodyAnim = m_obj.Anim.BodyAnimator;
        m_obj.Anim.PlayAnimation(bodyAnim, FieldObjectStateEnum.Idle.ToString());
    }

    public override void Close()
    {
    }
}