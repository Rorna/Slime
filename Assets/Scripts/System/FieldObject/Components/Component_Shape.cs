using System.Collections.Generic;
using UnityEngine;

public class Component_Shape : FieldObject_Components
{
    private BoxCollider2D m_collider;
    private Dictionary<string, Vector2> m_equipCollSizeDic = new Dictionary<string, Vector2>();
    private Dictionary<string, Vector2> m_attackColliderDic = new Dictionary<string, Vector2>();
    private Dictionary<string, Vector2> m_attackOffsetDic = new Dictionary<string, Vector2>();

    public Vector2 m_defaultCollSize;
    public Vector2 m_defaultCollOffset;

    public Component_Shape(FieldObject obj) : base(obj)
    {
    }

    public override void Close()
    {
        m_equipCollSizeDic.Clear();
        m_attackColliderDic.Clear();
        m_attackOffsetDic.Clear();
    }

    public override void InitComponent()
    {
        switch (m_obj.ObjectType)
        {
            case FieldObjectTypeEnum.Enemy:
            case FieldObjectTypeEnum.BossEnemy:
                {
                    if (ExternalDataManager.Instance.SetEnemyColliderInfoExtData(m_obj) == false)
                    {
                        Debug.LogError($"Stat Ext Data Fail");
                        return;
                    }
                }
                break;
        }
    }

    public void UpdateEquipColliderSize()
    {
        if (m_collider.IsNull())
            m_collider = m_obj.GetBoxCollider();

        if (m_obj.IsEnemy)
        {
            if (m_obj.Equip.HasItem)
            {
                string objName = UnityUtil.RemoveGuid(m_obj.Equip.ItemName);
                var newSize = GetEquipDictionaryData(objName);
                m_collider.size = newSize;
            }
            else
            {
                var defaultSize = GetDefaultCollSize();
                var defaultOff = GetDefaultOffSize();

                m_collider.size = defaultSize;
                m_collider.offset = defaultOff;
            }
        }
    }

    public void UpdateAttackColliderSize()
    {
        if (m_collider.IsNull())
            m_collider = m_obj.GetBoxCollider();

        if (m_obj.FieldObjectState != FieldObjectStateEnum.Attack &&
            m_obj.FieldObjectState != FieldObjectStateEnum.MovingAttack)
            return;

        if (m_obj.IsEnemy)
        {
            string objName = m_obj.Equip.HasItem
                ? UnityUtil.RemoveGuid(m_obj.Equip.ItemName)
                : DefineStrings.Default;

            var attackSize = GetAttackCollSizeData(objName);
            var attackOff = GetAttackOffsetData(objName);

            m_collider.size = attackSize;
            m_collider.offset = attackOff;
        }
    }

    public void AddEquipDictionaryData(string sizeKey, Vector2 sizeVec)
    {
        if (m_equipCollSizeDic.ContainsKey(sizeKey))
            return;

        m_equipCollSizeDic.Add(sizeKey, sizeVec);
    }

    public void AddAttackDictionaryData(string key, Vector2 sizeVec, Vector2 offVec)
    {
        if (m_attackColliderDic.ContainsKey(key) || m_attackOffsetDic.ContainsKey(key))
            return;

        m_attackColliderDic.Add(key, sizeVec);
        m_attackOffsetDic.Add(key, offVec);
    }

    public Vector2 GetEquipDictionaryData(string key)
    {
        if (m_equipCollSizeDic.ContainsKey(key) == false)
            return Vector2.zero;

        return m_equipCollSizeDic[key];
    }

    public Vector2 GetAttackCollSizeData(string key)
    {
        if (m_attackColliderDic.ContainsKey(key) == false)
            return Vector2.zero;

        return m_attackColliderDic[key];
    }

    public Vector2 GetAttackOffsetData(string key)
    {
        if (m_attackOffsetDic.ContainsKey(key) == false)
            return Vector2.zero;

        return m_attackOffsetDic[key];
    }

    public Vector2 GetDefaultCollSize()
    {
        if (m_equipCollSizeDic.ContainsKey(DefineStrings.Default) == false)
            return Vector2.zero;

        return m_equipCollSizeDic[DefineStrings.Default];
    }

    public Vector2 GetDefaultOffSize()
    {
        return Vector2.zero;
    }

    public void SetDefaultColliderSize()
    {
        if (m_equipCollSizeDic.TryGetValue(DefineStrings.Default, out var size) == false)
            return;

        m_defaultCollSize = size;
    }

    public void SetDefaultOffset()
    {
        m_defaultCollSize = Vector2.zero;
    }

    public Vector2 GetDefaultColliderSize()
    {
        return m_defaultCollSize;
    }

    public Vector2 GetDefaultOffset()
    {
        return m_defaultCollOffset;
    }
}