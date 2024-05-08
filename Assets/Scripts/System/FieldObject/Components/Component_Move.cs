using UnityEngine;

public class Component_Move : FieldObject_Components
{
    private LayerMask m_targetLayerMask;
    private FieldObjectStateEnum m_preState;
    private Rigidbody2D m_rigidbody;
    private Vector2 m_dodgeDir;
    private bool m_runDodge;
    private Vector3 m_moveDir;

    private bool m_superDodge = false;
    public Vector2 DodgeDir => m_dodgeDir;
    public Vector3 MoveDir => m_moveDir;
    public bool RunDodge => m_runDodge;

    public Component_Move(FieldObject obj) : base(obj)
    {
    }

    public void ComponentFixedUpdate()
    {
        if (m_obj.IsDead)
            return;

        if (m_obj.IsPlayer)
        {
            UpdatePlayerAim();
            UpdateDodgeState();
            Dodge();
        }
    }

    private void UpdateDodgeState()
    {
        if (m_obj.FieldObjectState != FieldObjectStateEnum.Dodge)
            return;

        if (m_rigidbody.velocity.magnitude < 0.5f)
            m_obj.SetObjectState(m_preState); //end dodge

        UpdateBounce();
    }

    private void UpdateBounce()
    {
        if (m_obj.HasContactObject(out GameObject obj, m_targetLayerMask) == false)
            return;

        string objName = UnityUtil.GetObjectName(obj);
        var fieldObj = FieldManager.Instance.GetFieldObject(objName);
        if (fieldObj.IsNotNull())
        {
            if (fieldObj.IsEnemy)
            {
                if (m_superDodge)
                {
                    m_obj.Attack.OnTakeDamage(fieldObj);
                    CameraManager.Instance.Shake(0.5f, 1.5f);
                }
                else
                {
                    m_obj.ChangeTargetState(fieldObj, FieldObjectStateEnum.Stun, fieldObj.StatDic[StatValueEnum.DuringStunTime]); //get stun
                }
            }
        }

        Bounce();

        if (m_obj.LightAreaState == LightAreaStateEnum.Light || m_superDodge)
        {
            SoundManager.Instance.PlayEffectAudio("Collision2", volume: 0.5f);
            CameraManager.Instance.Shake(0.5f, 1.5f);
            m_superDodge = false;
        }
        else
        {
            SoundManager.Instance.PlayEffectAudio("Collision", volume: 0.5f);
        }
    }

    public void ItemBounce(Vector2 dir, float speed = 0.5f)
    {
        m_rigidbody.velocity = Vector2.zero;
        m_rigidbody.AddForce(-dir * speed, ForceMode2D.Impulse);
    }

    public void Bounce(float speed = 0.5f)
    {
        m_rigidbody.velocity = Vector2.zero;
        m_rigidbody.AddForce(-m_dodgeDir * speed, ForceMode2D.Impulse);
    }

    public override void InitComponent()
    {
        m_targetLayerMask = m_obj.GetDefaultLayerMask();
        m_rigidbody = m_obj.GetRigidbody();
    }

    public override void Close()
    {
    }

    public void StopPlayer()
    {
        if (m_obj.FieldObjectState == FieldObjectStateEnum.Idle)
            return;

        if (m_obj.FieldObjectState == FieldObjectStateEnum.Dodge)
            return;

        var state = FieldObjectStateEnum.Idle;
        if (m_obj.FieldObjectState == FieldObjectStateEnum.MovingAttack)
        {
            if (m_obj.Attack.IsPlayingAttack)
                state = FieldObjectStateEnum.Attack;
        }
        else if (m_obj.FieldObjectState == FieldObjectStateEnum.Attack)
        {
            if (m_obj.Attack.IsPlayingAttack == false)
                state = FieldObjectStateEnum.Idle;
            else
            {
                state = FieldObjectStateEnum.Attack;
            }
        }

        m_obj.SetObjectState(state);
    }

    public void OnMovePlayer(Vector2 direction)
    {
        if (m_obj.IsDead)
            return;

        if (m_obj.FieldObjectState == FieldObjectStateEnum.Dodge)
            return;

        MovePlayer(direction);
    }

    private void MovePlayer(Vector2 direction)
    {
        SetMoveState();
        m_rigidbody.velocity = Vector2.zero;

        m_moveDir = new Vector3(direction.x, direction.y, 0).normalized;
        float moveSpeed = m_obj.StatDic[StatValueEnum.MoveSpeed];

        float dist = (m_obj.GetBoxCollider().size.x * Mathf.Sqrt(2)) * 0.5f;
        if (m_obj.HasContactObject(out GameObject go, direction, dist))
            return;

        m_obj.GO.transform.position += m_moveDir * moveSpeed * Time.deltaTime;
    }

    private void SetMoveState()
    {
        var state = m_obj.FieldObjectState;
        switch (m_obj.FieldObjectState)
        {
            case FieldObjectStateEnum.Idle:
                {
                    if (m_obj.Attack.IsPlayingAttack)
                        state = FieldObjectStateEnum.MovingAttack;
                    else
                    {
                        state = FieldObjectStateEnum.Move;
                    }
                    break;
                }

            case FieldObjectStateEnum.Attack:
                {
                    state = FieldObjectStateEnum.MovingAttack;
                    break;
                }

            case FieldObjectStateEnum.MovingAttack:
                {
                    if (m_obj.Attack.IsPlayingAttack == false)
                        state = FieldObjectStateEnum.Move;
                }
                break;
        }

        m_obj.SetObjectState(state);
    }

    private void UpdatePlayerAim()
    {
        if (PlayerInputController.Instance.InputLock)
            return;

        if (m_obj.IsPlayer == false)
            return;

        if (m_obj.IsDead)
            return;

        Vector3 mousePos = PlayerInputController.Instance.CursorObject.transform.position;

        // Calculate Look Dir
        Vector3 lookDirection = mousePos - m_obj.Body.transform.position;

        if (CameraManager.Instance.CameraFocusType == CameraFocusTypeEnum.FocusTarget ||
            CameraManager.Instance.CameraFocusType == CameraFocusTypeEnum.LookFocusedTarget)
        {
            var hitCollider = PlayerInputController.Instance.HitCollider;
            if (hitCollider.IsNotNull())
            {
                //set look dir to target
                lookDirection = hitCollider.gameObject.transform.position - m_obj.Body.transform.position;
            }
        }

        var firePoint = m_obj.GetFirePoint();
        if (firePoint.IsNull())
            return;

        m_obj.MoveFirePoint(lookDirection, 0.7f);
        m_obj.SetLookDir(lookDirection);
    }

    public void OnDodge(Vector2 direction)
    {
        if (m_obj.IsDead)
            return;

        var cmd = m_obj.CommandManager.GetCmd<FieldObjectDodgeCommand>((int)FieldObjectCmdType.FieldObjectDodgeTimer);
        if (cmd.IsNotNull())
        {
            var hud = UIManager.Instance.GetUIObject<UIHUD>();
            if (hud.IsVisible())
            {
                hud.ShowFloatingText("It is on cooldown");
                return;
            }
        }

        if (direction == Vector2.zero)
        {
            var hud = UIManager.Instance.GetUIObject<UIHUD>();
            if (hud.IsVisible())
            {
                hud.ShowFloatingText("Jumping on the spot is impossible");
                return;
            }
        }

        if (m_obj.FieldObjectState == FieldObjectStateEnum.Attack ||
            m_obj.FieldObjectState == FieldObjectStateEnum.MovingAttack ||
            m_obj.FieldObjectState == FieldObjectStateEnum.Dodge)
            return;

        cmd = new FieldObjectDodgeCommand(m_obj);
        float dodgeDelayTime = m_obj.StatDic[StatValueEnum.DodgeCooldown];
        cmd.Init(dodgeDelayTime);

        m_dodgeDir = direction.normalized;
        m_runDodge = true;
    }

    public void Dodge()
    {
        if (m_runDodge == false)
            return;

        m_preState = FieldObjectStateEnum.Idle;
        m_rigidbody.velocity = Vector2.zero;

        if (m_obj.LightAreaState == LightAreaStateEnum.Light)
        {
            m_superDodge = true;
        }

        float speed = m_obj.StatDic[StatValueEnum.DodgeSpeed];
        m_rigidbody.AddForce(m_dodgeDir * speed, ForceMode2D.Impulse);

        m_obj.SetObjectState(FieldObjectStateEnum.Dodge);
        m_runDodge = false;

        if (m_obj.LightAreaState == LightAreaStateEnum.Light)
            SoundManager.Instance.PlayEffectAudio("SuperDodge", volume: 0.5f);
        else
        {
            SoundManager.Instance.PlayEffectAudio("Dodge", volume: 0.5f);
        }
    }

    public void MovePosition(Vector2 pos)
    {
        m_obj.Body.transform.position = pos;
    }

    public void StopMove(float delayTime)
    {
        if (delayTime > 0f)
        {
            var cmd = new FieldObjectTimerCommand(m_obj);
            cmd.Init(delayTime);
            cmd.SetCallBack(() =>
            {
                m_rigidbody.velocity = Vector2.zero;
            });
        }

        m_rigidbody.velocity = Vector2.zero;
    }

    public void SetMoveDir(Vector3 dir)
    {
        m_moveDir = dir;
    }
}