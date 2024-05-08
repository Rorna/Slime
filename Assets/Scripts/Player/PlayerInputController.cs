using UnityEngine;

public partial class PlayerInputController : BaseManager
{
    public static PlayerInputController Instance;
    private FieldObject m_player;

    private GameObject m_cursor;

    private Vector2 m_cursorPos = Vector2.zero;
    private Collider2D m_hitCollider = null;

    public FieldObject Player => m_player;
    public Collider2D HitCollider => m_hitCollider;
    public GameObject CursorObject => m_cursor;

    private bool m_inputLock = false;

    public bool InputLock => m_inputLock;

    public override void Init()
    {
        if (Instance.IsNull())
            Instance = this;

        GameObject cursor = GameObject.FindWithTag(DefineStrings.Cursor);
        if (cursor.IsNull())
        {
            cursor = ResourceUtil.Instantiate(DefineStrings.Cursor);
            cursor.transform.SetParent(transform);

            m_cursor = cursor;
            Cursor.visible = false;
        }

        Managers.ClearEvent += Clear;
        Managers.UpdateManagersEvent += UpdateManager;
    }

    public void SetPlayer(FieldObject fieldObj)
    {
        if (m_player.IsNull())
        {
            m_player = fieldObj;
        }
    }

    private void Update()
    {
        UpdateInputKeys();
        UpdateCursorPos();
    }

    private void UpdateInputKeys()
    {
        if (m_player.IsNull())
            return;

        if (m_player.IsDead)
        {
            UpdateRestartInputKey();
            return;
        }

        UpdateCameraInputKey();
        UpdateTargetInputKey();
        UpdateAttackInputKey();
        UpdateEquipUnequipInputKey();
        UpdateThrowInputKey();
        UpdateMoveInputKey();
        UpdateInteractInputKey();
        UpdateSystemInputKey();
        UpdateFocusState();
        UpdateRestartInputKey();
    }

    private void UpdateRestartInputKey()
    {
        if (UIManager.Instance.IsVisibleUI<UIRestart>() == false)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            var uiRestart = UIManager.Instance.GetUIObject<UIRestart>();
            uiRestart.Restart();
        }
    }

    private void UpdateFocusState()
    {
        if (CameraManager.Instance.CameraFocusType != CameraFocusTypeEnum.FocusTarget &&
            CameraManager.Instance.CameraFocusType != CameraFocusTypeEnum.LookFocusedTarget)
            return;

        string objName = UnityUtil.GetObjectName(m_hitCollider.gameObject);
        var fieldObj = FieldManager.Instance.GetFieldObject(objName);
        if (fieldObj.IsNull() || fieldObj.IsDead)
        {
            CameraManager.Instance.SetCameraFocusType(CameraFocusTypeEnum.Normal);
            UpdateCursorPos();
            UpdateCursorShape();
        }
    }

    private void UpdateInteractInputKey()
    {
        if (Input.GetKeyDown((KeyCode)HotKeyCodeEnum.Interact))
        {
            var ui = UIManager.Instance.GetUIObject<UIDialog>();
            if (ui.IsVisible() && m_inputLock)
            {
                ui.ShowMessage();
                return;
            }

            var moveDir = m_player.Move.MoveDir;
            if (m_player.HasContactObject(out GameObject go, moveDir, 0.7f))
            {
                if (go.tag != "Interactive")
                    return;

                var interact = go.transform.parent.GetComponent<InteractableObject>();
                var list = interact.DialogsList;

                ui.Init(list);
                ui.Show();

                CameraManager.Instance.ZoomIn();

                UpdateInputLock(true);
            }
        }
    }

    private void TestCameraFunc()
    {
        if (Input.GetKeyDown((KeyCode.Alpha1)))
        {
            CameraManager.Instance.ZoomIn();
        }
        if (Input.GetKeyDown((KeyCode.Alpha2)))
        {
            CameraManager.Instance.ZoomOut();
        }

        if (Input.GetKeyDown((KeyCode.Alpha3)))
        {
            var list = FieldManager.Instance.GetFieldObjectList(FieldObjectTypeEnum.Enemy);
            foreach (var obj in list)
            {
                CameraManager.Instance.MoveToTarget(obj.Body.transform.position);
                break;
            }
        }
    }

    private void UpdateThrowInputKey()
    {
        if (m_inputLock)
            return;

        if (Input.GetKeyDown((KeyCode)HotKeyCodeEnum.ThrowAttack))
        {
            m_player.Attack.OnThrowAttack();
        }
    }

    private void UpdateAttackInputKey()
    {
        if (m_inputLock)
            return;

        if (m_player.Attack.IsPlayingAttack)
            return;

        if (Input.GetKey((KeyCode)HotKeyCodeEnum.Attack))
        {
            m_player.Attack.OnAttack();
        }
    }

    private void UpdateEquipUnequipInputKey()
    {
        if (m_inputLock)
            return;

        if (Input.GetKeyDown((KeyCode)HotKeyCodeEnum.Equip))
        {
            m_player.Equip.OnDecideEquip();
        }
    }

    private void UpdateCameraInputKey()
    {
        if (m_inputLock)
            return;

        //Focus Mouse
        if (Input.GetKeyDown((KeyCode)HotKeyCodeEnum.LookAround) || Input.GetKeyUp((KeyCode)HotKeyCodeEnum.LookAround))
        {
            CameraManager.Instance.SetCameraFocusType(CameraFocusTypeEnum.LookAround);
        }
    }

    private void UpdateMoveInputKey()
    {
        if (m_inputLock)
            return;

        Vector2 inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (inputDirection.magnitude > 1)
            inputDirection.Normalize();

        //move
        if (inputDirection != Vector2.zero)
        {
            m_player.Move.OnMovePlayer(inputDirection);
        }
        else
        {
            m_player.Move.StopPlayer();
        }

        //dodge
        if (Input.GetKeyDown((KeyCode)HotKeyCodeEnum.Dodge))
        {
            m_player.Move.OnDodge(inputDirection);
        }
    }

    private void UpdateSystemInputKey()
    {
        if (Input.GetKeyDown((KeyCode)HotKeyCodeEnum.Cancel))
        {
            if (m_inputLock)
            {
                UIManager.Instance.HidePopup<UIPopUpPause>();
                UpdateInputLock(false);
                return;
            }

            UIManager.Instance.ShowPopup<UIPopUpPause>();
            UpdateInputLock(true);
        }
    }

    public void UpdateInputLock(bool inputLock)
    {
        m_inputLock = inputLock;
    }

    public FieldObject GetObjectOnMousePos()
    {
        var hitCollider = Physics2D.OverlapPoint(m_cursorPos);
        if (hitCollider.IsNull())
            return null;

        string objName = UnityUtil.GetObjectName(hitCollider.gameObject);
        var fieldObj = FieldManager.Instance.GetFieldObject(objName);
        if (fieldObj.IsNull())
            return null;

        return fieldObj;
    }

    public override void UpdateManager()
    {
        if (ExtraSceneManager.Instance.SceneType == SceneTypeEnum.MainMenu)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
    }

    public override void Clear()
    {
        m_hitCollider = null;
        m_player = null;
    }

    public void OnDestroy()
    {
        Managers.ClearEvent -= Clear;
    }
}