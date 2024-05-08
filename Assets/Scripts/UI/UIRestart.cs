public class UIRestart : UIObject
{
    public override void OnShow()
    {
    }

    public override void OnHide()
    {
    }

    public void Restart()
    {
        ExtraSceneManager.Instance.RestartScene();
    }
}