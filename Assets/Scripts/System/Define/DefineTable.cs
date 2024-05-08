using System;
using System.Collections.Generic;

public static class DefineTable
{
    private static double[] m_datas;
    private static string[] m_stringDatas;
    private static int m_maxCount = (int)DefineValueEnums.Count;

    public static void Load(Dictionary<string, object> jsonData)
    {
        if (jsonData[DefineStrings.Type].ToString() != "DefineValue")
            return;

        m_maxCount = (int)DefineValueEnums.Count;
        m_datas = new double[m_maxCount];
        m_stringDatas = new string[m_maxCount];

        foreach (var data in jsonData)
        {
            if (data.Key == DefineStrings.Name || data.Key == DefineStrings.Type)
                continue;

            string dataName = data.Key;
            var index = Enum.Parse(typeof(DefineValueEnums), dataName);

            try
            {
                string dataValue = data.Value.ToString();
                if (double.TryParse(dataValue, out var value))
                {
                    m_datas[(int)index] = value;
                }

                m_stringDatas[(int)index] = dataValue;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public static int GetInt(DefineValueEnums value)
    {
        return (int)GetDouble(value);
    }

    public static float GetFloat(DefineValueEnums value)
    {
        return (float)GetDouble(value);
    }

    public static long GetLong(DefineValueEnums value)
    {
        return (long)GetDouble(value);
    }

    public static double GetDouble(DefineValueEnums value)
    {
        if (m_datas.IsNull())
            return 0;

        int index = (int)value;
        if (index >= m_maxCount)
            return 0;

        return m_datas[index];
    }

    public static string GetString(DefineValueEnums value)
    {
        if (m_stringDatas.IsNull())
            return "";

        int index = (int)value;
        if (index >= m_maxCount)
            return "";

        return m_stringDatas[index];
    }
}