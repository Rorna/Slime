using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExtraSceneManager : BaseManager
{
    public static ExtraSceneManager Instance;

    private static int m_currentSceneIndex = 0;
    private SceneTypeEnum m_sceneType;
    private Dictionary<int, string> m_sceneDic = new Dictionary<int, string>();

    public SceneTypeEnum SceneType => m_sceneType;

    public override void Init()
    {
        if (Instance.IsNull())
            Instance = this;

        SetSceneExtData();

        Managers.ClearEvent += Clear;
        Managers.UpdateManagersEvent += UpdateManager;
    }

    private void SetSceneExtData()
    {
        var sceneList = new Dictionary<int, string>(ExternalDataManager.Instance.GetSceneNameExtData());
        if (sceneList.Count == 0)
            return;

        m_sceneDic = sceneList;
        m_currentSceneIndex = 0;
    }

    public void StartGame()
    {
        Managers.Instance.AllClear();
        StartCoroutine(LoadSceneAsync(999));
    }

    public void LoadScene()
    {
        Managers.Instance.AllClear();

        string sceneName = m_sceneDic[m_currentSceneIndex];
        StartCoroutine(LoadSceneAsync(m_currentSceneIndex));

        var loadingUI = UIManager.Instance.GetUIObject<UILoading>();
        loadingUI.Show();
    }

    public void LoadScene(int sceneIndex)
    {
        Managers.Instance.AllClear();
        var loadingUI = UIManager.Instance.GetUIObject<UILoading>();
        if (loadingUI.IsVisible() == false)
            loadingUI.Show();

        StartCoroutine(LoadSceneAsync(sceneIndex));
    }

    public void LoadNextScene()
    {
        Managers.Instance.AllClear();
        var loadingUI = UIManager.Instance.GetUIObject<UILoading>();
        loadingUI.Show();

        StartCoroutine(LoadSceneAsync(++m_currentSceneIndex));
    }

    public void RestartScene()
    {
        LoadScene(m_currentSceneIndex);
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        float minimumLoadingTime = 1.0f;
        float startTime = Time.time;

        string sceneName = m_sceneDic[sceneIndex];
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        yield return new WaitUntil(() => loadOperation.isDone);

        float loadingDuration = Time.time - startTime;
        if (loadingDuration < minimumLoadingTime)
        {
            yield return new WaitForSeconds(minimumLoadingTime - loadingDuration);  //Wait Until Minimum Loading Time
        }

        if (FieldManager.Instance.CreateSceneObject(sceneName) == false)
            yield return null;

        m_currentSceneIndex = sceneIndex;
        SetSceneType(sceneName);
        Managers.Instance.UpdateAllManagers();

        var loadingUI = UIManager.Instance.GetUIObject<UILoading>();
        if (loadingUI.IsVisible())
            loadingUI.FadeOutHide();

        if (sceneName != "MainMenu")
        {
            var hud = UIManager.Instance.GetUIObject<UIHUD>();
            if (hud.IsNotNull() && hud.IsVisible() == false)
            {
                var playerGameObj = FieldManager.Instance.GetPlayer();
                if (playerGameObj.IsNull())
                    yield return null;

                var playerFieldObj = FieldManager.Instance.GetFieldObject(UnityUtil.GetObjectName(playerGameObj));
                if (playerFieldObj.IsNull())
                    yield return null;

                hud.Init(playerFieldObj);
                hud.Show();
            }

            if (PlayerInputController.Instance.InputLock)
                PlayerInputController.Instance.UpdateInputLock(false);

            PlayerPrefs.SetInt("SceneIndex", m_currentSceneIndex);
        }

        //Play bgm
        switch (sceneName)
        {
            case "MainMenu":
                SoundManager.Instance.PlayBGM("MainMenu");
                break;

            case "Tutorial":
                SoundManager.Instance.PlayBGM("Tutorial");
                break;

            case "Stage1":
                SoundManager.Instance.PlayBGM("Stage1");
                break;
        }

        yield return null;
    }

    private void SetSceneType(string sceneName)
    {
        if (sceneName != "MainMenu")
            m_sceneType = SceneTypeEnum.InGame;
        else
        {
            m_sceneType = SceneTypeEnum.MainMenu;
        }
    }

    public int GetCurrentSceneIndex()
    {
        return m_currentSceneIndex;
    }

    public string GetCurrentSceneName()
    {
        return m_sceneDic[m_currentSceneIndex];
    }

    public override void UpdateManager()
    {
    }

    public override void Clear()
    {
    }

    public void OnDestroy()
    {
        m_currentSceneIndex = 0;
        Managers.ClearEvent -= Clear;
    }
}