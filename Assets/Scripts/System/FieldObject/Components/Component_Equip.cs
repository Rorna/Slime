using System;
using UnityEngine;

public class Component_Equip : FieldObject_Components
{
    private FieldObject m_currentEquipItem;
    private bool m_hasItem = false;
    private string m_itemName;

    public bool HasItem => m_hasItem;
    public string ItemName => m_itemName;
    public FieldObject CurrentEquipItem => m_currentEquipItem;

    public Component_Equip(FieldObject obj) : base(obj)
    {
    }

    public override void InitComponent()
    {
    }

    private void EquipItem(FieldObject itemObj)
    {
        if (itemObj.IsNull())
            return;

        m_currentEquipItem = itemObj;
        SetEquipItemData(m_currentEquipItem.ObjName);

        if (m_obj.IsEnemy)
        {
            string itemName = UnityUtil.RemoveGuid(m_itemName);
            m_obj.Anim.ChangeAnimatorLayer(itemName);
            m_obj.Shape.UpdateEquipColliderSize();
        }

        m_currentEquipItem.Item.UpdateItemEquipInfos(m_obj, true);
        PoolManager.Instance.AllocateToPool(m_obj.Equip.ItemName);
        SoundManager.Instance.PlayEffectAudio(m_obj, DefineStrings.Equip);
    }

    public void OnDecideEquip()
    {
        if (m_hasItem)
        {
            if (CanEquipItem(out var itemObj))
            {
                ChangeItem(itemObj);
            }
            else
            {
                UnequipItem();
            }
        }
        else
        {
            if (CanEquipItem(out var itemObj))
            {
                EquipItem(itemObj);
            }
            else
            {
                var dialog = UIManager.Instance.GetUIObject<UIHUD>();
                dialog.ShowFloatingText("There are no items to equip");
            }
        }
    }

    public bool OnUnequipItem()
    {
        if (m_obj.Equip.m_hasItem == false)
            return false;

        if (PoolManager.Instance.IsInPool(m_obj.Equip.ItemName) == false)
            return false;

        UnequipItem();
        return true;
    }

    private void ChangeItem(FieldObject itemObj) 
    {
        UnequipItem();
        EquipItem(itemObj); //equip new item
    }

    private void UnequipItem()
    {
        var item = PoolManager.Instance.AllocateToField(m_obj.Equip.ItemName,
            m_obj.GO.transform.position, m_obj.LookDir.normalized);
        if (item.IsNull())
            return;

        ResetEquipData();

        if (m_obj.IsEnemy)
        {
            m_obj.Anim.ChangeAnimatorLayer(DefineStrings.Default);
            m_obj.Shape.UpdateEquipColliderSize();
        }

        //item state change
        item.Item.UpdateItemEquipInfos(m_obj, false);
        SoundManager.Instance.PlayEffectAudio(m_obj, DefineStrings.Unequip);
    }

    private bool CanEquipItem(out FieldObject itemObj)
    {
        itemObj = null;
        LayerMask layer = 1 << LayerMask.NameToLayer(DefineStrings.Item);


        if (m_obj.HasContactObject(out GameObject contactObj, layer))
        {
            string objName = UnityUtil.GetObjectName(contactObj);
            var fieldObj = FieldManager.Instance.GetFieldObject(objName);
            if (fieldObj.IsNull())
                return false;

            if (fieldObj.ObjectType != FieldObjectTypeEnum.Item)
                return false;

            itemObj = fieldObj;
        }

        if (itemObj.IsNull())
            return false;

        return true;
    }

    private void SetEquipItemData(string itemName)
    {
        m_itemName = itemName;
        m_hasItem = true;
    }

    private void ResetEquipData()
    {
        m_hasItem = false;
        m_itemName = string.Empty;

        m_currentEquipItem = null;
    }

    public string GetWeaponName()
    {
        if (m_hasItem == false)
            return String.Empty;

        if (m_itemName == String.Empty)
            return String.Empty;

        return m_itemName;
    }

    //enemy Equip
    public void EnemyEquip(string itemName)
    {
        if (m_obj.IsEnemy == false)
            return;

        //create item
        var fieldObj = FieldManager.Instance.GetFieldObject(itemName);
        if (fieldObj.IsNull())
            return;

        EquipItem(fieldObj);
    }

    public override void Close()
    {
        m_currentEquipItem = null;
        m_hasItem = false;
        m_itemName = string.Empty;
    }
}