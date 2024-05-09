using System.Collections.Generic;
using UnityEngine;

public class FieldManager : BaseManager
{
    public static FieldManager Instance;

    private GameObject m_fieldMaster;
    private GameObject m_player;
    private static int m_fieldObjCount = 0;
    private static Dictionary<string, FieldObject> m_fieldObjDic = new Dictionary<string, FieldObject>();

    public GameObject FieldMaster => m_fieldMaster;

    public int FieldObjCount => m_fieldObjCount;

    public override void Init()
    {
        if (Instance.IsNull())
            Instance = this;

        CreateFieldMaster();

        Managers.ClearEvent += Clear;
        Managers.UpdateManagersEvent += UpdateManager;
    }

    private void CreateFieldMaster()
    {
        if (m_fieldMaster.IsNotNull())
            return;

        GameObject fieldMaster = new GameObject { name = "FieldMaster" };
        fieldMaster.transform.SetParent(transform);
        m_fieldMaster = fieldMaster;
    }

    public GameObject GetPlayer()
    {
        if (m_player.IsNull())
        {
            foreach (var obj in m_fieldObjDic)
            {
                if (obj.Value.IsPlayer)
                {
                    m_player = obj.Value.GO;
                    break;
                }
            }
        }

        return m_player;
    }

    public FieldObject CreateObject(FieldObjectTypeEnum objectType, string prefabName, Vector2 pos, Quaternion direction, Transform parent = null)
    {
        FieldObject fieldObj = new FieldObject();
        fieldObj.Init(objectType, prefabName, pos, direction);
        if (fieldObj.GO.IsNull())
            return null;

        if (m_fieldObjDic.ContainsKey(prefabName) == false)
        {
            m_fieldObjDic.Add(fieldObj.ObjName, fieldObj);
            m_fieldObjCount++;
        }

        return fieldObj;
    }

    public FieldObject CreateObject(FieldObjectTypeEnum objectType, string prefabName, string objName, Vector2 pos, Quaternion direction, Transform parent = null)
    {
        FieldObject fieldObj = new FieldObject();
        fieldObj.Init(objectType, prefabName, pos, direction, objName);
        if (fieldObj.GO.IsNull())
            return null;

        if (m_fieldObjDic.ContainsKey(prefabName) == false)
        {
            m_fieldObjDic.Add(fieldObj.ObjName, fieldObj);
            m_fieldObjCount++;
        }

        return fieldObj;
    }

    public struct ObjectExtData
    {
        public string FieldObjectName;
        public FieldObjectTypeEnum FieldObjectType;
        public Vector2 CreatePos;
        public float CreateRotation;
    }

    public struct EnemyInfo
    {
        public string BasePrefab;
        public EnemyAttackTypeEnum EnemyAttackType;
        public AIStateEnum AIState;
        public List<Vector3> AIRotateDirList;
        public bool HasWeapon;
        public string WeaponName;
        public List<EnemyAttackTypeEnum> AttackTypeList;
    }

    private List<ObjectExtData> m_objExtDataList;

    public bool CreateSceneObject(string sceneName)
    {
        if (ExternalDataManager.Instance.SetSceneObjectExtData(sceneName, out List<ObjectExtData> objList) == false)
            return false;

        if (objList.Count == 0)
            return false;

        m_objExtDataList = new List<ObjectExtData>(objList); //deep copy
        foreach (var objExtData in m_objExtDataList)
        {
            string objName = objExtData.FieldObjectName;
            var fieldObjType = objExtData.FieldObjectType;
            Vector2 createPos = objExtData.CreatePos;
            float createRotation = objExtData.CreateRotation;

            switch (fieldObjType)
            {
                case FieldObjectTypeEnum.Player:
                case FieldObjectTypeEnum.Item:
                    {
                        var rotate = createRotation == 0 ? Quaternion.identity : Quaternion.Euler(0, 0, createRotation);
                        CreateObject(fieldObjType, objName, createPos, rotate);
                    }
                    break;

                case FieldObjectTypeEnum.Enemy:
                case FieldObjectTypeEnum.BossEnemy:
                    {
                        /// 1. Create Object By BasePrefab
                        /// 2. Set Data
                        
                        var dic = ExternalDataManager.Instance.GetEnemyExtData(objName);
                        if (dic.Count == 0)
                            return false;

                        var enemyInfo = dic[objName];
                        var rotate = createRotation == 0 ? Quaternion.identity : Quaternion.Euler(0, 0, createRotation);
                        var fieldObj = CreateObject(fieldObjType, enemyInfo.BasePrefab, objName, createPos, rotate); 
                        if (fieldObj.IsNull())
                            return false;

                        FieldObject item = null;
                        if (enemyInfo.HasWeapon)
                        {
                            item = new FieldObject();
                            item = CreateObject(FieldObjectTypeEnum.Item, enemyInfo.WeaponName, Vector2.zero, Quaternion.identity);
                        }

                        fieldObj.AI.InitExtData(enemyInfo, item);
                    }
                    break;
            }
        }

        return true;
    }

    private void Update()
    {
        if (m_fieldObjCount == 0)
            return;

        var objDic = new List<FieldObject>(m_fieldObjDic.Values);
        foreach (var obj in objDic)
        {
            obj.ObjectUpdate();
        }

    }

    private void FixedUpdate()
    {
        if (m_fieldObjCount == 0)
            return;

        var objDic = new List<FieldObject>(m_fieldObjDic.Values);
        foreach (var obj in objDic)
        {
            obj.ObjectFixedUpdate();
        }
    }

    public FieldObject GetFieldObject(string objName)
    {
        if (m_fieldObjDic.TryGetValue(objName, out FieldObject obj))
        {
            return obj;
        }

        return null;
    }

    public List<FieldObject> GetFieldObjectList(FieldObjectTypeEnum type)
    {
        List<FieldObject> objList = new List<FieldObject>();
        foreach (var obj in m_fieldObjDic)
        {
            var fieldObj = obj.Value;
            if (fieldObj.ObjectType == type)
                objList.Add(fieldObj);
        }

        return objList;
    }

    public int GetSameObjectCount(string name)
    {
        int objCount = 0;
        foreach (var key in m_fieldObjDic.Keys)
        {
            string[] partStrings = key.Split('_');
            if (partStrings.Length > 0 && partStrings[0] == name)
            {
                objCount++;
            }
        }
        return objCount;
    }

    public void AllocateToFieldMaster(FieldObject obj, Vector2 pos, Vector3 lookDir)
    {
        obj.GO.transform.position = pos;
        obj.Body.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookDir);

        var gameObject = obj.GO.transform;
        gameObject.SetParent(m_fieldMaster.transform);

        gameObject.gameObject.SetActive(true);

        UpdateFieldObjData(true, obj);
    }

    public void UpdateFieldObjData(bool isAdd, FieldObject fieldObj)
    {
        if (isAdd)
        {
            m_fieldObjDic.Add(fieldObj.ObjName, fieldObj);
            m_fieldObjCount++;
        }
        else
        {
            m_fieldObjDic.Remove(fieldObj.ObjName);
            m_fieldObjCount--;
        }
    }

    public void DestroyFieldObject(FieldObject obj)
    {
        UpdateFieldObjData(false, obj);
        obj.ReleaseFieldObject();
        Destroy(obj.GO);
    }

    public override void UpdateManager()
    {
    }

    public override void Clear()
    {
        m_player = null;
        m_objExtDataList?.Clear();
        foreach (var fieldObj in m_fieldObjDic)
        {
            Destroy(fieldObj.Value.GO);
        }
        m_fieldObjDic?.Clear();
        m_fieldObjCount = 0;
    }

    public void OnDestroy()
    {
        Managers.ClearEvent -= Clear;
    }
}