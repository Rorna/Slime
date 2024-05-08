using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class ExternalDataManager : BaseManager
{
    public static ExternalDataManager Instance;
    private Dictionary<string, Dictionary<string, object>> m_jsonDataDictionary = new Dictionary<string, Dictionary<string, object>>();

    public override void Init()
    {
        if (Instance.IsNull())
            Instance = this;

        InitJsonDataDictionary();

        SetDefineTable();

        Managers.ClearEvent += Clear;
        Managers.UpdateManagersEvent += UpdateManager;
    }

    private void InitJsonDataDictionary()
    {
        string filePath = Application.dataPath + "/Resources/Jsons/";
        string[] jsonPaths = Directory.GetFiles(filePath, "*.json");
        foreach (var jsonPath in jsonPaths)
        {
            if (string.IsNullOrEmpty(jsonPath))
            {
                Debug.LogError($"Can't Find Path!");
                return;
            }

            if (File.Exists(jsonPath) == false)
            {
                Debug.LogError($"No json File In That Path!");
                return;
            }

            string jsonContent = File.ReadAllText(jsonPath);
            var jsonData = MiniJSON.Json.Deserialize(jsonContent) as Dictionary<string, object>;
            if (jsonData.IsNull())
            {
                Debug.LogError($"Can't Find Json Data!");
                return;
            }

            if (jsonData.ContainsKey(DefineStrings.Type) == false)
            {
                Debug.LogError($"Can't Find Type In Json Data!");
                return;
            }

            string key = (string)jsonData[DefineStrings.Name];
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"Can't Find key!");
                return;
            }

            m_jsonDataDictionary.Add(key, jsonData);
        }
    }

    public override void UpdateManager()
    {
    }

    public override void Clear()
    {
    }

    public void OnDestroy()
    {
        Managers.ClearEvent -= Clear;
    }
}