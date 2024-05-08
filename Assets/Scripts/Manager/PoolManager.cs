using System.Collections.Generic;
using UnityEngine;

public class PoolManager : BaseManager
{
    public static PoolManager Instance;
    private GameObject m_poolMaster;
    private Dictionary<string, FieldObject> m_poolDictionary = new Dictionary<string, FieldObject>();

    public GameObject PoolMaster => m_poolMaster;

    public override void Init()
    {
        if (Instance.IsNotNull())
            return;

        Instance = this;
        CreatePoolMaster();

        Managers.ClearEvent += Clear;
        Managers.UpdateManagersEvent += UpdateManager;
    }

    private void InitChildObj<T>() where T : class
    {
    }

    private void CreatePoolMaster()
    {
        if (m_poolMaster.IsNotNull())
            return;

        GameObject poolMaster = new GameObject { name = "PoolMaster" };
        poolMaster.transform.SetParent(transform);
        m_poolMaster = poolMaster;
    }

    public bool IsInPool(string objName)
    {
        if (m_poolDictionary.ContainsKey(objName) == false)
            return false;

        return true;
    }

    public void AllocateToPool(string objName)
    {
        var obj = FieldManager.Instance.GetFieldObject(objName);
        if (obj.IsNull())
            return;

        FieldManager.Instance.UpdateFieldObjData(false, obj);
        obj.ResetPosData();

        var gameObject = obj.GO.transform;
        gameObject.SetParent(m_poolMaster.transform);
        gameObject.gameObject.SetActive(false);

        if (m_poolDictionary.ContainsKey(objName) == false)
            m_poolDictionary.Add(obj.ObjName, obj);
    }

    public FieldObject AllocateToField(string objName, Vector2 pos, Vector3 lookDir)
    {
        if (m_poolDictionary.TryGetValue(objName, out FieldObject obj) == false)
            return null;

        m_poolDictionary.Remove(obj.ObjName);
        FieldManager.Instance.AllocateToFieldMaster(obj, pos, lookDir);

        return obj;
    }

    public override void UpdateManager()
    {
    }

    public override void Clear()
    {
        m_poolDictionary.Clear();
    }

    public void OnDestroy()
    {
        Managers.ClearEvent -= Clear;
    }
}