using System;
using System.Collections.Generic;
using UnityEngine;

public partial class ExternalDataManager
{
    private void SetDefineTable()
    {
        if (m_jsonDataDictionary.TryGetValue("DefineValue", out Dictionary<string, object> jsonData) == false)
            return;

        DefineTable.Load(jsonData);
    }

    public bool SetSceneObjectExtData(string sceneName, out List<FieldManager.ObjectExtData> objList)
    {
        objList = new List<FieldManager.ObjectExtData>();

        string jsonDataName = sceneName + '_' + "FieldObject";
        if (m_jsonDataDictionary.TryGetValue(jsonDataName, out var sceneObjectData) == false)
            return false;

        foreach (var data in sceneObjectData)
        {
            if (data.Key == DefineStrings.Name || data.Key == DefineStrings.Type)
                continue;

            var objectData = data.Value;
            if (objectData is List<object> objectDataList == false)
                return false;

            foreach (var objData in objectDataList)
            {
                if (objData is Dictionary<string, object> objDataDic == false)
                    return false;

                string fieldObjStateStr = objDataDic["ObjectType"].ToString();
                if (Enum.TryParse(fieldObjStateStr, out FieldObjectTypeEnum fieldObjState) == false)
                    return false;

                Vector2 createPos = GetVector2(objDataDic, "CreatePosition");
                float createRotation = GetFloat(objDataDic, "CreateRotation");

                var extData = new FieldManager.ObjectExtData
                {
                    FieldObjectName = data.Key,
                    FieldObjectType = fieldObjState,
                    CreatePos = createPos,
                    CreateRotation = createRotation
                };

                objList.Add(extData);
            }
        }

        return true;
    }

    public bool SetStatExtData(FieldObject obj)
    {
        if (m_jsonDataDictionary.TryGetValue("ObjectStatus", out var statData) == false)
            return false;

        foreach (var data in statData)
        {
            if (data.Key == DefineStrings.Name || data.Key == DefineStrings.Type)
                continue;

            string objName = data.Key;
            if (objName != UnityUtil.RemoveGuid(obj.ObjName))
                continue;

            var statInfos = data.Value;
            if (statInfos is List<object> statList == false)
                return false;

            foreach (var stat in statList)
            {
                if (stat is Dictionary<string, object> statDic == false)
                    return false;

                Dictionary<string, float> statDictionary = new Dictionary<string, float>();
                foreach (var values in statDic)
                {
                    statDictionary.Add(values.Key, Convert.ToSingle(values.Value));
                }

                obj.InitStatValues(statDictionary);
                statDictionary.Clear();
            }
        }

        return true;
    }

    public bool SetEffectExtData(FieldObject obj)
    {
        if (m_jsonDataDictionary.TryGetValue("FieldObjectEffect", out var effectData) == false)
            return false;

        foreach (var data in effectData)
        {
            if (data.Key == DefineStrings.Name || data.Key == DefineStrings.Type)
                continue;

            string objName = data.Key;
            if (objName != UnityUtil.RemoveGuid(obj.ObjName))
                continue;

            var effectInfos = data.Value;
            if (effectInfos is List<object> effectList == false)
                return false;

            foreach (var effect in effectList)
            {
                Dictionary<FieldObjectStateEnum, EffectInfo> effectDic = new Dictionary<FieldObjectStateEnum, EffectInfo>();
                if (effect is Dictionary<string, object> effectByStateList == false)
                    return false;

                string state = effectByStateList["State"].ToString();
                if (Enum.TryParse(state, out FieldObjectStateEnum enumValue) == false)
                    return false;

                string effectName = effectByStateList["EffectName"].ToString();
                Vector2 pos = Vector2.zero;
                if (effectByStateList["Position"] is List<object> posList)
                {
                    pos.x = Convert.ToSingle(posList[0]);
                    pos.y = Convert.ToSingle(posList[1]);
                }

                float angle = Convert.ToSingle(effectByStateList["Angle"]);
                float destroyTime = Convert.ToSingle(effectByStateList["DestroyTime"]);

                EffectManager.Instance.LoadEffectExtData(obj, enumValue, effectName, pos, angle, destroyTime);
            }
        }

        return true;
    }

    public Dictionary<string, FieldManager.EnemyInfo> GetEnemyExtData(string fieldObjName)
    {
        if (m_jsonDataDictionary.TryGetValue("EnemyInfo", out var enemyInfoData) == false)
            return null;

        foreach (var data in enemyInfoData)
        {
            if (data.Key == DefineStrings.Name || data.Key == DefineStrings.Type)
                continue;

            string objName = data.Key;
            if (objName != fieldObjName)
                continue;

            if (data.Value is List<object> enemyDataList == false)
                return null;

            var enemyDic = new Dictionary<string, FieldManager.EnemyInfo>();
            foreach (var enemyInfo in enemyDataList)
            {
                if (enemyInfo is Dictionary<string, object> enemyInfoDic == false)
                    return null;

                string basePrefabName = enemyInfoDic["BasePrefab"].ToString();
                string enemyTypeStr = enemyInfoDic["EnemyAttackType"].ToString();
                if (Enum.TryParse(enemyTypeStr, out EnemyAttackTypeEnum enemyType) == false)
                    return null;

                string AITypeStr = enemyInfoDic["AIState"].ToString();
                if (Enum.TryParse(AITypeStr, out AIStateEnum AIState) == false)
                    return null;

                var rotateDirData = enemyInfoDic["AIRotateDirection"];
                if (rotateDirData is List<object> rotateDirList == false)
                    return null;

                var dirList = new List<Vector3>();
                foreach (var dir in rotateDirList)
                {
                    Vector3 dirVec = Vector3.zero;
                    switch (dir.ToString())
                    {
                        case "UP":
                            dirVec = Vector3.up;
                            break;

                        case "DOWN":
                            dirVec = Vector3.down;
                            break;

                        case "LEFT":
                            dirVec = Vector3.left;
                            break;

                        case "RIGHT":
                            dirVec = Vector3.right;
                            break;

                        case "None":
                            dirVec = Vector3.zero;
                            break;
                    }

                    dirList.Add(dirVec);
                }

                bool hasWeapon = GetBool(enemyInfoDic, "HasWeapon");
                string weaponName = enemyInfoDic["WeaponName"].ToString();

                var attackTypeData = enemyInfoDic["BossAttackType"];
                if (attackTypeData is List<object> attackTypeDataList == false)
                    return null;

                List<EnemyAttackTypeEnum> attackTypeList = new List<EnemyAttackTypeEnum>();
                foreach (var attackType in attackTypeDataList)
                {
                    string str = attackType.ToString();
                    if (Enum.TryParse(str, out EnemyAttackTypeEnum attackTypeEnum) == false)
                        return null;

                    attackTypeList.Add(attackTypeEnum);
                }

                var enemyInfoStruct = new FieldManager.EnemyInfo
                {
                    BasePrefab = basePrefabName,
                    EnemyAttackType = enemyType,
                    AIState = AIState,
                    AIRotateDirList = dirList,
                    HasWeapon = hasWeapon,
                    WeaponName = weaponName,
                    AttackTypeList = attackTypeList
                };

                enemyDic.Add(objName, enemyInfoStruct);
            }

            return enemyDic;
        }

        return null;
    }

    public bool SetItemExtData(FieldObject obj)
    {
        if (m_jsonDataDictionary.TryGetValue("ItemInfo", out var itemData) == false)
            return false;

        foreach (var data in itemData)
        {
            if (data.Key == DefineStrings.Name || data.Key == DefineStrings.Type)
                continue;

            string objName = data.Key;
            if (objName != UnityUtil.RemoveGuid(obj.ObjName))
                continue;

            var itemInfos = data.Value;
            if (itemInfos is List<object> itemInfoList == false)
                return false;

            foreach (var itemInfo in itemInfoList)
            {
                if (itemInfo is Dictionary<string, object> itemInfoDic == false)
                    return false;

                string state = itemInfoDic["ItemType"].ToString();
                if (Enum.TryParse(state, out ItemTypeEnum attackType) == false)
                    return false;

                string usePoolStr = itemInfoDic["UsePool"].ToString();
                bool.TryParse(usePoolStr, out bool usePool);

                string destroyAfterContactStr = itemInfoDic["DestroyAfterContact"].ToString();
                bool.TryParse(destroyAfterContactStr, out bool destroyAfterContact);

                string reequipableStr = itemInfoDic["Reequipable"].ToString();
                bool.TryParse(reequipableStr, out bool reequipable);

                float destroyTime = Convert.ToSingle(itemInfoDic["DestroyTime"]);
                obj.Item.SetExternalItemData(attackType, usePool, destroyTime, destroyAfterContact, reequipable);
            }
        }

        return true;
    }

    public bool SetEnemyColliderInfoExtData(FieldObject obj)
    {
        if (m_jsonDataDictionary.TryGetValue("EquipColliderSize", out var equipData) == false ||
            m_jsonDataDictionary.TryGetValue("AttackColliderSize", out var attackData) == false)
            return false;

        //Equip Collider Size
        foreach (var data in equipData)
        {
            if (data.Key == DefineStrings.Name || data.Key == DefineStrings.Type)
                continue;

            string dataName = data.Key;
            var value = data.Value;
            if (value is List<object> list == false)
                return false;

            float x = Convert.ToSingle(list[0]);
            float y = Convert.ToSingle(list[1]);

            var colliderSize = new Vector2(x, y);
            obj.Shape.AddEquipDictionaryData(dataName, colliderSize);
        }

        //Attack Collider Size
        foreach (var data in attackData)
        {
            if (data.Key == DefineStrings.Name || data.Key == DefineStrings.Type)
                continue;

            string dataName = data.Key;
            var value = data.Value;
            if (value is List<object> list == false)
                return false;

            float sizeX = Convert.ToSingle(list[0]);
            float sizeY = Convert.ToSingle(list[1]);

            float offX = Convert.ToSingle(list[2]);
            float offY = Convert.ToSingle(list[3]);

            var colliderSize = new Vector2(sizeX, sizeY);
            var offSize = new Vector2(offX, offY);

            obj.Shape.AddAttackDictionaryData(dataName, colliderSize, offSize);
        }
        return true;
    }

    public bool SetObjSoundExtData(FieldObject obj)
    {
        if (m_jsonDataDictionary.TryGetValue("FieldObjectSound", out var audioData) == false)
            return false;

        foreach (var data in audioData)
        {
            if (data.Key == DefineStrings.Name || data.Key == DefineStrings.Type)
                continue;

            string objName = data.Key;
            if (objName != UnityUtil.RemoveGuid(obj.ObjName))
                continue;

            var audioInfos = data.Value;
            if (audioInfos is List<object> audioList == false)
                return false;

            foreach (var audioClips in audioList)
            {
                if (audioClips is Dictionary<string, object> audioClipDic == false)
                    return false;
                var audioClipDictionary = new Dictionary<string, string>();
                foreach (var audioClip in audioClipDic)
                {
                    audioClipDictionary.Add(audioClip.Key, audioClip.Value.ToString());
                }

                SoundManager.Instance.LoadObjAudioClip(obj, audioClipDictionary);
                audioClipDictionary.Clear();
            }
        }

        return true;
    }

    public Dictionary<int, string> GetSceneNameExtData()
    {
        if (m_jsonDataDictionary.TryGetValue("SceneInfo", out var SceneData) == false)
            return null;

        var sceneDic = new Dictionary<int, string>();
        foreach (var data in SceneData)
        {
            if (data.Key == DefineStrings.Name || data.Key == DefineStrings.Type)
                continue;

            var sceneInfo = data.Value;
            if (sceneInfo is List<object> sceneInfoList == false)
                return null;

            foreach (var scene in sceneInfoList)
            {
                if (scene is Dictionary<string, object> sceneInfoDic == false)
                    return null;

                int index = Convert.ToInt32(sceneInfoDic["Index"]);

                sceneDic.Add(index, data.Key);
            }
        }

        if (sceneDic.Count == 0)
            return null;

        return sceneDic;
    }

    private float GetFloat(Dictionary<string, object> jsonData, string key)
    {
        if (jsonData.TryGetValue(key, out var value) == false)
            return 0f;

        return Convert.ToSingle(value);
    }

    private List<string> GetStringDic(Dictionary<string, object> jsonData, string key)
    {
        List<string> stringList = new List<string>();
        if (jsonData.TryGetValue(key, out var data) && data is List<object> objList)
        {
            foreach (var obj in objList)
            {
                stringList.Add(obj.ToString());
            }
        }

        return stringList;
    }

    private bool GetBool(Dictionary<string, object> jsonData, string key)
    {
        if (jsonData.TryGetValue(key, out var value) && value is bool data)
        {
            return data;
        }

        return false;
    }

    private Vector2 GetVector2(Dictionary<string, object> jsonData, string key)
    {
        if (jsonData.TryGetValue(key, out var dataList) && dataList is List<object> list && list.Count >= 2)
        {
            float x = Convert.ToSingle(list[0]);
            float y = Convert.ToSingle(list[1]);
            return new Vector2(x, y);
        }

        return Vector2.zero;
    }

    private Vector3 GetVector3(Dictionary<string, object> jsonData, string key)
    {
        if (jsonData.TryGetValue(key, out var dataList) && dataList is List<object> list && list.Count >= 3)
        {
            float x = Convert.ToSingle(list[0]);
            float y = Convert.ToSingle(list[1]);
            float z = Convert.ToSingle(list[2]);
            return new Vector3(x, y, z);
        }

        return Vector3.zero;
    }
}