using UnityEngine;

public class UIPopUpPause : UIPopUp
{
    public void InitPopUp()
    {
        
    }

    public override void OnShow()
    {
        InitPopUp();
        Cursor.visible = true;
    }

    public override void OnHide()
    {
        Cursor.visible = false;
    }

    public void OnClickTitleButton()
    {
        ExtraSceneManager.Instance.LoadScene(999);
    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }

    public void OnClickCancelButton()
    {
        PlayerInputController.Instance.UpdateInputLock(false);
        UIManager.Instance.HidePopup<UIPopUpPause>();
    }
}