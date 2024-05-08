using System;
using System.Collections.Generic;
using UnityEngine;

public partial class FieldObject
{
    //stat
    public int CurrentHP => (int)m_statDic[StatValueEnum.CurrentHP];

    private Dictionary<StatValueEnum, float> m_defaultStatDic;
    private Dictionary<StatValueEnum, float> m_statDic;

    public Dictionary<StatValueEnum, float> StatDic => m_statDic;

    public void InitStatValues(Dictionary<string, float> statDict)
    {
        m_defaultStatDic = new Dictionary<StatValueEnum, float>(); //deep copy
        m_statDic = new Dictionary<StatValueEnum, float>();

        //set m_statDic
        foreach (var stat in statDict)
        {
            StatValueEnum statType;
            if (Enum.TryParse(stat.Key, out statType) == false)
            {
                Debug.LogError($"Failed stat Data");
                return;
            }

            m_defaultStatDic.Add(statType, stat.Value);
            m_statDic.Add(statType, stat.Value);
        }

        //set Current HP
        float maxHP = m_statDic[StatValueEnum.MaxHP];
        m_statDic.Add(StatValueEnum.CurrentHP, maxHP);
        m_defaultStatDic.Add(StatValueEnum.CurrentHP, maxHP);
    }

    public void SetStat(StatValueEnum statType, float value)
    {
        if (m_statDic.ContainsKey(statType))
        {
            m_statDic[statType] = value;
        }
        else
        {
            Debug.LogError("Stat type not initialized: " + statType);
        }

        if (statType == StatValueEnum.CurrentHP)
            UpdateHP();
    }

    public void ResetStat(StatValueEnum statType)
    {
        if (m_defaultStatDic.ContainsKey(statType))
        {
            m_statDic[statType] = m_defaultStatDic[statType];
        }
        else
        {
            Debug.LogError("Default stat value not found for: " + statType);
        }
    }

    private void ResetAllStats()
    {
        m_statDic.Clear();
        foreach (var statType in m_defaultStatDic.Keys)
        {
            m_statDic[statType] = m_defaultStatDic[statType];
        }
    }
}