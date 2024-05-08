using UnityEngine;

public class UIMainMenu : UIObject
{
    [SerializeField] private GameObject m_continueMenu;

    private void Awake()
    {
        int sceneData = PlayerPrefs.GetInt("SceneIndex", -1);
        if (sceneData == -1)
            m_continueMenu.SetActive(false);
        else
        {
            m_continueMenu.SetActive(true);
        }
    }

    public override void OnShow()
    {

    }

    public override void OnHide()
    {
    }

    public void OnClickNewGameButton()
    {
        PlayerPrefs.DeleteAll();
        ExtraSceneManager.Instance.LoadScene(1);
        Hide();
    }

    public void OnClickContinueButton()
    {
        //load saved data
        int sceneData = PlayerPrefs.GetInt("SceneIndex", -1);
        if (sceneData == -1)
            return;

        ExtraSceneManager.Instance.LoadScene(sceneData);
    }

    public void OnClickHelpButton()
    {
        UIManager.Instance.ShowPopup<UIPopUpHelp>();
    }

    public void OnClickExitButton()
    {
        Application.Quit();
    }
}