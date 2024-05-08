using System;
using UnityEngine;

public enum ItemTypeEnum
{
    Ranged,
    Melee,
    Bullet,
    Light,
    Slime,
    None,
    Count,
}

public class Component_Item : FieldObject_Components
{
    private bool m_isRotate = false;
    private FieldObject m_owner;
    private LayerMask m_layerMask = 0;
    private ItemTypeEnum m_itemType;
    private bool m_usePool = false;
    private float m_destroyTime;
    private bool m_destroyAfterContact = false;
    private bool m_reequipable = false;

    public ItemTypeEnum ItemType => m_itemType;
    public float DistroyTime => m_destroyTime;

    public Component_Item(FieldObject obj) : base(obj)
    {
    }

    public override void InitComponent()
    {
        if (ExternalDataManager.Instance.SetItemExtData(m_obj) == false)
        {
            Debug.LogError($"Item Ext Data Fail");
            return;
        }
    }

    public void ComponentUpdate()
    {
        if (m_obj.FieldObjectState != FieldObjectStateEnum.Attack)
            return;

        if (m_obj.GetRigidbody().velocity.magnitude < 0.1f)
        {
            m_isRotate = false;
            m_obj.GetRigidbody().velocity = Vector3.zero;
            m_obj.SetObjectState(FieldObjectStateEnum.Idle);
            return;
        }

        if (m_isRotate)
        {
            float rotateSpeed = DefineTable.GetFloat(DefineValueEnums.ItemRotateSpeed);
            m_obj.Body.transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        }

        OnItemAttack();
    }

    private void OnItemAttack()
    {
        if (m_obj.HasContactObject(out GameObject contactObj, m_layerMask) == false)
            return;

        if (contactObj.IsNull())
            return;

        HandleCollision(contactObj);

        if (m_destroyAfterContact)
            m_obj.Die();

        if (m_reequipable == false)
            m_obj.GetCollider2D().enabled = false;

        m_obj.Move.ItemBounce(m_obj.Move.MoveDir);

        ResetItemData();
    }

    private void HandleCollision(GameObject contactObj)
    {
        string objName = UnityUtil.GetObjectName(contactObj);
        var fieldObj = FieldManager.Instance.GetFieldObject(objName);
        if (fieldObj.IsNotNull())
        {
            if (m_owner.ObjectType == fieldObj.ObjectType)
                return;

            if (fieldObj.ObjectType == FieldObjectTypeEnum.Item)
                return;

            if (m_itemType == ItemTypeEnum.Melee || m_itemType == ItemTypeEnum.Ranged || m_itemType == ItemTypeEnum.Light || m_itemType == ItemTypeEnum.Slime)
            {
                if (m_owner.ObjectType == FieldObjectTypeEnum.Player)
                    fieldObj.AI.SetStun(fieldObj.StatDic[StatValueEnum.DuringStunTime]);
                else
                {
                    m_owner.Attack.OnTakeDamage(fieldObj);
                }
            }
            else if (m_itemType == ItemTypeEnum.Bullet)
            {
                m_owner.Attack.OnTakeDamage(fieldObj);
            }
        }

        if (m_itemType == ItemTypeEnum.Light)
        {
            var lightObj = UnityUtil.FindChildObject(m_obj.GO, DefineStrings.Light, true);
            if (lightObj.IsNull())
                return;

            lightObj.gameObject.SetActive(true);
            m_obj.Body.SetActive(false);
        }

        SoundManager.Instance.PlayEffectAudio("Collision", volume: 0.5f);
    }

    private void UpdateOwnerInfos(FieldObject owner, bool isEquip)
    {
        m_owner = isEquip ? owner : null;

        switch (owner.ObjectType)
        {
            case FieldObjectTypeEnum.Enemy:
            case FieldObjectTypeEnum.BossEnemy:
                {
                    if (isEquip)
                    {
                        var enemyType = EnemyAttackTypeEnum.None;
                        switch (m_itemType)
                        {
                            case ItemTypeEnum.Melee:
                                enemyType = EnemyAttackTypeEnum.Melee;
                                break;

                            case ItemTypeEnum.Ranged:
                                enemyType = EnemyAttackTypeEnum.Ranged;
                                break;
                        }
                        owner.AI.SetEnemyAttackType(enemyType);
                    }
                    else
                    {
                        owner.AI.SetEnemyAttackType(EnemyAttackTypeEnum.Melee);
                    }
                }
                break;
        }
    }

    public void UpdateItemEquipInfos(FieldObject owner, bool isEquip)
    {
        ResetItemData(); //clear data before equip and unequip

        UpdateOwnerInfos(owner, isEquip);
        if (isEquip == false)
            ResetLayerMask();

        UpdateLayerMask();
        UpdateObjState(FieldObjectStateEnum.Idle);
    }

    private void UpdateLayerMask()
    {
        if (m_owner.IsNull())
            return;

        LayerMask patrolNodeLayer = LayerMask.GetMask(DefineStrings.PatrolNode);
        LayerMask itemLayer = LayerMask.GetMask(DefineStrings.Item);
        LayerMask lightLayer = LayerMask.GetMask(DefineStrings.Light);
        LayerMask bossAreaLayerMask = LayerMask.GetMask(DefineStrings.BossArea);
        LayerMask cameraBorderLayerMask = LayerMask.GetMask(DefineStrings.CameraBorder);
        LayerMask triggerLayerMask = LayerMask.GetMask(DefineStrings.Trigger);

        string type = Enum.GetName(typeof(FieldObjectTypeEnum), m_owner.ObjectType);
        int sameTypeLayer = LayerMask.GetMask(type);
        m_layerMask = ~(patrolNodeLayer | itemLayer | lightLayer | sameTypeLayer | bossAreaLayerMask | cameraBorderLayerMask | triggerLayerMask);
    }

    private void ResetLayerMask()
    {
        m_layerMask = 0;
    }

    public void UpdateItemAttackInfos(FieldObject owner)
    {
        m_owner = owner;
        m_isRotate = m_itemType != ItemTypeEnum.Bullet;
        UpdateLayerMask();

        UpdateObjState(FieldObjectStateEnum.Attack);
    }

    private void UpdateObjState(FieldObjectStateEnum state)
    {
        if (state == FieldObjectStateEnum.None)
            return;

        m_obj.SetObjectState(state);
    }

    private void SetItemType(ItemTypeEnum type)
    {
        m_itemType = type;
    }

    public void SetExternalItemData(ItemTypeEnum type, bool usePool, float destroyTime, bool destroyAfterContact, bool reequipable)
    {
        SetItemType(type);
        m_usePool = usePool;
        m_destroyTime = destroyTime;
        m_destroyAfterContact = destroyAfterContact;
        m_reequipable = reequipable;
    }

    private void ResetItemData()
    {
        m_isRotate = false;
        m_owner = null;

        UpdateObjState(FieldObjectStateEnum.Idle);
    }

    public override void Close()
    {
    }
}