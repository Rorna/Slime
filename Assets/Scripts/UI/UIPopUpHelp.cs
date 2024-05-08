public class UIPopUpHelp : UIPopUp
{
    public override void OnShow()
    {
    }

    public override void OnHide()
    {
    }

    public void OnClickExitButton()
    {
        UIManager.Instance.HidePopup<UIPopUpHelp>();
    }
}