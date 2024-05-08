using UnityEngine;

public class UIPopUp : UIObject
{
    public Canvas PopUpCanvas
    {
        get;
        private set;
    }

    protected override void OnAwake()
    {
        Init();
    }

    public void Init()
    {
        PopUpCanvas = gameObject.GetComponent<Canvas>();
    }
}