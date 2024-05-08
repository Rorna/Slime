using System;
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager : BaseManager
{
    public static UIManager Instance;

    private Dictionary<Type, UIObject> m_uiObjectDic = new Dictionary<Type, UIObject>();
    private Dictionary<GameTypeEnum, List<UIObject>> m_shownUIObjectsByGameTypeDic = new Dictionary<GameTypeEnum, List<UIObject>>();
    public Canvas RootCanvas { get; private set; }

    public RectTransform RootCanvasRect
    {
        get => m_rootCanvasRect;
    }

    private RectTransform m_rootCanvasRect;

    public Dictionary<Type, UIObject> GetUIObjectDic() => m_uiObjectDic;

    public override void Init()
    {
        Instance = this;
        InitRootCanvas();

        Managers.ClearEvent += Clear;
        Managers.UpdateManagersEvent += UpdateManager;
    }

    private void InitRootCanvas()
    {
        if (m_rootCanvasRect.IsNotNull())
            return;

        GameObject rootCanvas = new GameObject { name = "Root" };
        rootCanvas.transform.SetParent(transform);

        var canvasRect = rootCanvas.GetOrAddComponent<RectTransform>();
        m_rootCanvasRect = canvasRect;
    }

    public T Show<T>() where T : UIObject
    {
        var uiObject = GetUIObject<T>();
        if (uiObject.IsNull())
            return null;

        uiObject.Show();
        return uiObject;
    }

    public UIObject Show(Type type)
    {
        var uiObject = GetUIObject(type);
        if (uiObject.IsNull())
            return null;

        uiObject.Show();
        return uiObject;
    }

    public void Hide<T>() where T : UIObject
    {
        var uiObject = GetUIObject<T>();
        if (uiObject.IsNull())
            return;

        if (uiObject.isActiveAndEnabled)
            uiObject.Hide();
    }

    public void Hide(Type type)
    {
        var uiObject = GetUIObject(type);
        if (uiObject.IsNull())
            return;

        if (uiObject.isActiveAndEnabled)
            uiObject.Hide();
    }

    public void Destroy<T>() where T : UIObject
    {
        var uiObject = GetUIObject<T>();
        if (uiObject.IsNull())
            return;

        if (uiObject.isActiveAndEnabled)
            uiObject.Hide();

        var obj = uiObject.gameObject;
        Instance.m_uiObjectDic.Remove(uiObject.GetType());
        Debug.Log($"[UIManager] Destroy UIObject! type: {uiObject.GetType()}, prefab: {obj.name}");
        Destroy(obj);
    }

    public void HideAll()
    {
        foreach (var uiObject in Instance.m_uiObjectDic.Values)
        {
            if (uiObject.isActiveAndEnabled)
            {
                uiObject.Hide();
            }
        }
    }

    public bool IsVisibleUI<T>() where T : UIObject
    {
        var type = typeof(T);
        if (Instance.m_uiObjectDic.ContainsKey(type))
            return Instance.m_uiObjectDic[type].IsVisible();

        return false;
    }

    public bool IsVisibleUI<T>(out T ui) where T : UIObject
    {
        ui = null;

        var type = typeof(T);
        if (Instance.m_uiObjectDic.ContainsKey(type))
        {
            ui = Instance.m_uiObjectDic[type] as T;
            return ui.IsVisible();
        }

        return false;
    }

    public UIManager GetInst()
    {
        return Instance;
    }

    public T GetUIObject<T>() where T : UIObject
    {
        var type = typeof(T);
        return GetUIObject(type) as T;
    }

    public UIObject GetUIObject(Type type)
    {
        if (Instance.m_uiObjectDic.ContainsKey(type) == false)
        {
            LoadUIObject(type);

            if (Instance.m_uiObjectDic.ContainsKey(type) == false)
            {
                Debug.LogError($"[UIManager] Failed to load UIObject! type: {type.Name}");
                return null;
            }
        }

        return Instance.m_uiObjectDic[type];
    }

    private void LoadUIObject(Type type)
    {
        if (Instance.m_uiObjectDic.ContainsKey(type))
            return;

        string path = DefineStrings.UIPath + type.Name;
        var ui = ResourceUtil.Instantiate(path, m_rootCanvasRect.transform);
        if (ui.IsNull())
        {
            Debug.LogError($"[UIManager] Failed to load UI Prefab! type: {type.Name}, prefab: {ui.name}");
            return;
        }

        var uiObject = ui.GetComponent<UIObject>();
        if (uiObject.IsNull())
        {
            Debug.LogError($"[UIManager] Cannot find UIObject in prefab! type: {type.Name}, prefab: {uiObject}");
            Destroy(ui);
            return;
        }

        ui.SetActive(false);
        AddUIObjectDic(uiObject);
    }

    public void AddUIObjectDic(UIObject uiObject)
    {
        if (Instance.m_uiObjectDic.ContainsKey(uiObject.GetType()))
            return;

        Instance.m_uiObjectDic.Add(uiObject.GetType(), uiObject);
    }

    public override void UpdateManager()
    {
        //Hide all existing ui and search if there is a ui in the current hierarchy, add it
        //If the UI is not attached to the ui Manager, add it
        UIObject[] uiObjects = FindObjectsOfType<UIObject>();
        foreach (UIObject uiObject in uiObjects)
        {
            var obj = UnityUtil.FindChildObject(m_rootCanvasRect.gameObject, uiObject.gameObject.name);
            if (obj.IsNull())
                uiObject.transform.SetParent(m_rootCanvasRect);

            AddUIObjectDic(uiObject);
        }
    }

    public override void Clear()
    {
        UIObject[] uiObjects = FindObjectsOfType<UIObject>();
        if (uiObjects.Length > 0)
        {
            foreach (UIObject uiObject in uiObjects)
            {
                uiObject.Hide();
            }
        }

        //Hide UI except Loading UI
        var uiLoadingType = typeof(UILoading);
        foreach (var ui in m_uiObjectDic)
        {
            if (ui.Key == uiLoadingType && ui.Key.IsVisible)
                continue;

            if (ui.Key.IsVisible)
                ui.Value.Hide();
        }

        HideAllPopup();

        m_popupStack.Clear();
        m_uiPopUpDic.Clear();
        m_shownUIObjectsByGameTypeDic.Clear();
    }

    public void OnDestroy()
    {
        Managers.ClearEvent -= Clear;
    }
}