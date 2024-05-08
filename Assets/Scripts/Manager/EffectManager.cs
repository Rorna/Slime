using System.Collections.Generic;
using UnityEngine;

public struct EffectInfo
{
    public string m_effectFileName;
    public Vector2 m_pos;
    public float m_angle;
    public float m_destroyTime;

    public void Init(string effectName, Vector2 pos, float angle = 0f, float destroyTime = 0f)
    {
        m_effectFileName = effectName;
        m_pos = pos;
        m_angle = angle;
        m_destroyTime = destroyTime;
    }
}

public class EffectManager : BaseManager
{
    public static EffectManager Instance;

    private Dictionary<FieldObject, Dictionary<FieldObjectStateEnum, EffectInfo>> m_objEffectDic;
    private Dictionary<FieldObjectStateEnum, EffectInfo> m_effectDic;

    public override void Init()
    {
        if (Instance.IsNotNull())
            return;

        Instance = this;
        m_objEffectDic = new Dictionary<FieldObject, Dictionary<FieldObjectStateEnum, EffectInfo>>();

        Managers.ClearEvent += Clear;
        Managers.UpdateManagersEvent += UpdateManager;
    }

    public void PlayEffect(FieldObject obj, FieldObjectStateEnum state, Vector2 pos)
    {
        if (m_objEffectDic.TryGetValue(obj, out var effectInfoDic) == false)
            return;

        if (effectInfoDic.TryGetValue(state, out var effectInfo) == false)
            return;

        var effect = FieldManager.Instance.CreateObject(FieldObjectTypeEnum.Effect, effectInfo.m_effectFileName, pos,
            Quaternion.identity);

        effect.DestroyObject(effectInfo.m_destroyTime);
    }

    public void LoadEffectExtData(FieldObject obj, FieldObjectStateEnum effectState, string effectName, Vector2 pos,
        float angle, float destroyTime)
    {
        var effectInfo = new EffectInfo();
        effectInfo.Init(effectName, pos, angle, destroyTime);

        if (m_objEffectDic.TryGetValue(obj, out var effect))
        {
            effect.Add(effectState, effectInfo);
        }
        else
        {
            Dictionary<FieldObjectStateEnum, EffectInfo> effectDic =
                new Dictionary<FieldObjectStateEnum, EffectInfo>();

            effectDic.Add(effectState, effectInfo);
            m_objEffectDic.Add(obj, effectDic);
        }
    }

    public void RemoveObjectInfo(FieldObject obj)
    {
        if (m_objEffectDic.ContainsKey(obj) == false)
            return;

        m_objEffectDic.Remove(obj);
    }

    public override void UpdateManager()
    {
    }

    public override void Clear()
    {
        m_objEffectDic.Clear();
    }

    public void OnDestroy()
    {
        Managers.ClearEvent -= Clear;
    }
}