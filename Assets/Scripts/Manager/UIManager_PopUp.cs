using System;
using System.Collections.Generic;
using UnityEngine;

public partial class UIManager
{
    private int m_defaultOrder = 10;
    private Dictionary<Type, UIPopUp> m_uiPopUpDic = new Dictionary<Type, UIPopUp>();
    private Stack<UIPopUp> m_popupStack = new Stack<UIPopUp>();

    private UIPopUp ShowPopup(Type type)
    {
        var popup = GetPopup(type);
        if (popup.IsNull())
            return null;

        popup.Show();

        m_defaultOrder++;
        popup.PopUpCanvas.sortingOrder = m_defaultOrder;

        m_popupStack.Push(popup);

        return popup;
    }

    public T ShowPopup<T>() where T : UIPopUp
    {
        return ShowPopup(typeof(T)) as T;
    }

    private void HidePopup(Type type)
    {
        if (m_popupStack.Count == 0)
            return;

        var popup = GetPopup(type);
        if (popup.IsNull())
            return;

        if (m_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!");
            return;
        }

        if (popup.isActiveAndEnabled)
        {
            popup.Hide();

            m_defaultOrder--;
            m_popupStack.Pop();
        }
    }

    public void HidePopup<T>() where T : UIPopUp
    {
        HidePopup(typeof(T));
    }

    private bool IsVisiblePopup(Type type)
    {
        foreach (var popup in Instance.m_popupStack)
        {
            if (popup.GetType() == type && popup.IsVisible())
                return true;
        }

        return false;
    }

    private UIPopUp GetVisiblePopUp(Type type)
    {
        foreach (var popup in Instance.m_popupStack)
        {
            if (popup.GetType() == type && popup.IsVisible())
                return popup;
        }

        return null;
    }

    public T GetVisiblePopUp<T>() where T : UIPopUp
    {
        return (T)Instance.GetVisiblePopUp(typeof(T));
    }

    public bool IsVisiblePopup<T>() where T : UIPopUp
    {
        return IsVisiblePopup(typeof(T));
    }

    public bool IsVisibleAnyPopup()
    {
        foreach (var popup in Instance.m_popupStack)
        {
            if (popup.IsVisible())
                return true;
        }

        return false;
    }

    /*public void HideAllPopup(Type type)
    {
        var typeStack = m_popupStack.Where(w => w.GetType() == type);
        foreach (var popup in typeStack)
        {
            if (popup.IsNull())
                continue;

            popup.Hide();
        }

        while (m_popupStack.Count > 0)
            ClosePopupUI();
    }
    보류, 특정 타입만 모두 하이드라...
    public void HideAllPopup<T>() where T : UIPopUp
    {
        HideAllPopup(typeof(T));
    }*/

    public void HideAllPopup()
    {
        foreach (var popup in Instance.m_popupStack)
        {
            if (popup.IsNull())
                continue;

            popup.Hide();
        }

        Instance.m_popupStack.Clear();
    }

    public T GetPopUp<T>() where T : UIPopUp
    {
        return Instance.GetPopup<T>();
    }

    private UIPopUp GetPopup(Type type)
    {
        if (m_uiPopUpDic.ContainsKey(type) == false)
        {
            LoadPopup(type);
        }

        if (m_uiPopUpDic.ContainsKey(type) == false)
        {
            Debug.LogError($"[UIManager] Failed to load UIObject! type: {type.Name}");
            return null;
        }

        return Instance.m_uiPopUpDic[type];
    }

    private T GetPopup<T>() where T : UIPopUp
    {
        return GetPopup(typeof(T)) as T;
    }

    private void LoadPopup(Type type)
    {
        if (m_uiPopUpDic.ContainsKey(type))
            return;

        string path = DefineStrings.UIPopUpPath + type.Name;
        var popUp = ResourceUtil.Instantiate(path, m_rootCanvasRect.transform);
        if (popUp.IsNull())
        {
            Debug.LogError($"[UIManager] Failed to load UIPopUp Prefab! type: {type.Name}, prefab: {popUp.name}");
            return;
        }

        var uiPopUp = popUp.GetComponent<UIPopUp>();
        if (uiPopUp.IsNull())
        {
            Debug.LogError($"[UIManager] Cannot find UIPopUp in prefab! type: {type.Name}, prefab: {popUp}");
            Destroy(uiPopUp);
            return;
        }

        m_uiPopUpDic.Add(type, uiPopUp);
        popUp.SetActive(false);
    }

    private void LoadPopup<T>() where T : UIPopUp
    {
        LoadPopup(typeof(T));
    }
}