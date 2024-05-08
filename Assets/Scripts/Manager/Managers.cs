using UnityEngine;
using UnityEngine.EventSystems;

public class Managers : BaseManager
{
    public static Managers Instance;

    public delegate void DelegateClear();

    public delegate void DelegateUpdateManagers();

    public static event DelegateClear ClearEvent;

    public static event DelegateUpdateManagers UpdateManagersEvent;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        InitManager();
        InitDataManager();
        InitEventSystem();
        InitCameraManager();
        InitFieldManager();
        InitPlayerInputController();
        InitPool();
        InitEffectManager();
        InitSoundManager();
        InitUIManager();
        InitExtraSceneManager();

        ExtraSceneManager.Instance.StartGame();
    }

    public override void Clear()
    {
    }

    public override void UpdateManager()
    {
    }

    private void InitManager()
    {
        if (Instance.IsNotNull())
            return;

        GameObject go = GameObject.Find("@Managers"); 
        if (go.IsNull())
        {
            go = new GameObject { name = "@Managers" };
            go.AddComponent<Managers>();
        }

        DontDestroyOnLoad(go);
        Instance = go.GetComponent<Managers>();
    }

    private void InitFieldManager()
    {
        if (FieldManager.Instance.IsNotNull())
            return;

        string objName = "@FieldManager";
        var field = InitComponent<FieldManager>(objName, true);
        field.Init();
    }

    private void InitPlayerInputController()
    {
        if (PlayerInputController.Instance.IsNotNull())
            return;

        string objName = "@PlayerInputController";
        var inputController = InitComponent<PlayerInputController>(objName, true);
        inputController.Init();
    }

    private void InitPool()
    {
        if (PoolManager.Instance.IsNotNull())
            return;

        string objName = "@PoolManager";
        var poolManager = InitComponent<PoolManager>(objName, true);
        poolManager.Init();
    }

    private void InitDataManager()
    {
        if (ExternalDataManager.Instance.IsNotNull())
            return;

        string objName = "@ExternalDataManager";
        var externalDataManager = InitComponent<ExternalDataManager>(objName, true);
        externalDataManager.Init();
    }

    private void InitEffectManager()
    {
        if (EffectManager.Instance.IsNotNull())
            return;

        string objName = "@EffectManager";
        var effectManager = InitComponent<EffectManager>(objName, true);
        effectManager.Init();
    }

    private void InitSoundManager()
    {
        if (SoundManager.Instance.IsNotNull())
            return;

        string objName = "@SoundManager";
        var soundManager = InitComponent<SoundManager>(objName, true);
        soundManager.Init();
    }

    private void InitUIManager()
    {
        if (UIManager.Instance.IsNotNull())
            return;

        string objName = "@UIManager";
        var uiManager = InitComponent<UIManager>(objName, true);
        uiManager.Init();
    }

    private void InitCameraManager()
    {
        if (CameraManager.Instance.IsNotNull())
            return;

        string objName = "@CameraManager";
        var cameraManager = InitComponent<CameraManager>(objName, true);
        cameraManager.Init();
    }

    private void InitExtraSceneManager()
    {
        if (ExtraSceneManager.Instance.IsNotNull())
            return;

        string objName = "@ExtraSceneManager";
        var extraSceneManager = InitComponent<ExtraSceneManager>(objName, true);
        extraSceneManager.Init();
    }

    private void InitEventSystem()
    {
        Object obj = FindObjectOfType(typeof(EventSystem));
        if (obj.IsNull())
        {
            var eventSystem = ResourceUtil.Instantiate(DefineStrings.UIPath + "EventSystem");
            eventSystem.name = "@EventSystem";

            DontDestroyOnLoad(eventSystem);
        }
    }

    private T InitComponent<T>(string objName, bool dontDestroy) where T : Component
    {
        GameObject go = new GameObject { name = objName };
        T component = go.AddComponent<T>();

        if (dontDestroy)
            DontDestroyOnLoad(go);

        return component;
    }

    public void AllClear() //Clear All Manager
    {
        ClearEvent?.Invoke();
    }

    public void UpdateAllManagers()
    {
        UpdateManagersEvent?.Invoke();
    }
}