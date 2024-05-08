using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class FieldObject
{
    public FieldObjectTypeEnum ObjectType { get; private set; }
    public FieldObjectStateEnum FieldObjectState { get; private set; }

    public bool IsPlayer => ObjectType == FieldObjectTypeEnum.Player;
    public bool IsEnemy => ObjectType == FieldObjectTypeEnum.Enemy || ObjectType == FieldObjectTypeEnum.BossEnemy;
    public bool IsBossEnemy => ObjectType == FieldObjectTypeEnum.BossEnemy;
    public bool IsItem => ObjectType == FieldObjectTypeEnum.Item;

    public bool IsDead { get; private set; }
    private string m_objName;
    private string m_uid;
    private float m_destroyTime;

    private Vector2 m_lookDir = Vector2.right;

    private List<FieldObject_Components> m_components;

    private GameObject m_body;
    private GameObject m_leg;
    private GameObject m_firePoint;
    private Rigidbody2D m_rigidbody;
    private LayerMask m_lightLayerMask;

    private Collider2D m_collider;

    private LightAreaStateEnum m_lightAreaState = LightAreaStateEnum.None;
    public LightAreaStateEnum LightAreaState => m_lightAreaState;

    public GameObject GO { get; private set; }
    
    public string ObjName => m_objName;
    public string UID => m_uid;
    public Vector3 LookDir => m_lookDir;
    public GameObject Body => m_body;
    public GameObject Leg => m_leg;
    public GameObject FirePoint => m_firePoint;
    public CommandManager<FieldObject> CommandManager { get; private set; }

    //Components
    public Component_Move Move { get; private set; }
    public Component_Animation Anim { get; private set; }
    public Component_Attack Attack { get; private set; }
    public Component_Item Item { get; private set; }
    public Component_Shape Shape { get; private set; }
    public Component_Equip Equip { get; private set; }
    public Component_Effect Effect { get; private set; }
    public Component_AI AI { get; private set; }

    public FieldObject Init(FieldObjectTypeEnum objectType, string prefabName, Vector2 pos,
        Quaternion direction, string objName = null)
    {
        string objPath = UnityUtil.SetPathByType(objectType, prefabName);
        if (InitGameObjects(objPath, pos, direction) == false)
            return null;

        SetObjectState();
        SetObjectType(objectType);
        SetUid();
        SetObjectName(objName);

        SetChildObject();
        SetCollider();
        SetFirePoint();

        SetInternalData(objectType);
        SetExternalData();

        SetComponents();

        SetLightLayer();
        InitCommandManager();

        return this;
    }

    private void SetUid()
    {
        Guid guid = Guid.NewGuid();
        var guidArr = guid.ToString().Split('-');
        m_uid = guidArr[0];
    }

    private void InitCommandManager()
    {
        CommandManager = new CommandManager<FieldObject>();
    }

    private void SetComponents()
    {
        m_components = new List<FieldObject_Components>();

        switch (ObjectType)
        {
            case FieldObjectTypeEnum.Player:
                {
                    Move = new Component_Move(this);
                    Attack = new Component_Attack(this);
                    Equip = new Component_Equip(this);

                    Move.InitComponent();
                    Attack.InitComponent();
                    Equip.InitComponent();

                    m_components.Add(Move);
                    m_components.Add(Attack);
                    m_components.Add(Equip);
                }
                break;

            case FieldObjectTypeEnum.Enemy:
            case FieldObjectTypeEnum.BossEnemy:
                {
                    Move = new Component_Move(this);
                    Attack = new Component_Attack(this);
                    Equip = new Component_Equip(this);
                    AI = new Component_AI(this);

                    Move.InitComponent();
                    Attack.InitComponent();
                    Equip.InitComponent();
                    AI.InitComponent();

                    m_components.Add(Move);
                    m_components.Add(Attack);
                    m_components.Add(Equip);
                    m_components.Add(AI);
                }
                break;

            case FieldObjectTypeEnum.Item:
                {
                    Item = new Component_Item(this);
                    Move = new Component_Move(this);

                    Move.InitComponent();
                    Item.InitComponent();

                    m_components.Add(Item);
                    m_components.Add(Move);
                }
                break;
        }

        //Common component
        Anim = new Component_Animation(this);
        Shape = new Component_Shape(this);
        Effect = new Component_Effect(this);

        Anim.InitComponent();
        Shape.InitComponent();
        Effect.InitComponent();

        m_components.Add(Anim);
        m_components.Add(Shape);
        m_components.Add(Effect);
    }

    private void SetObjectName(string name) //use UID
    {
        string objName = name.IsNull() ? GO.transform.name : name;
        if (objName.EndsWith("(Clone)"))
        {
            objName = objName.Replace("(Clone)", "");
        }

        //Attach UID except player
        if (IsPlayer == false)
        {
            objName = objName + "_" + m_uid;
        }

        GO.transform.name = objName;
        m_objName = objName;
    }

    private void SetChildObject()
    {
        m_body = UnityUtil.FindChildObject(GO, DefineStrings.Body);
        m_leg = UnityUtil.FindChildObject(GO, DefineStrings.Leg);
    }

    private void SetInternalData(FieldObjectTypeEnum type)
    {
        switch (type)
        {
            case FieldObjectTypeEnum.Player:
                {
                    m_lightAreaState = LightAreaStateEnum.Normal;

                    PlayerInputController.Instance.SetPlayer(this);
                    CameraManager.Instance.SetTarget(this);
                }
                break;
        }
    }

    private void SetObjectType(FieldObjectTypeEnum type)
    {
        ObjectType = type;
    }

    private bool InitGameObjects(string objectPath, Vector2 pos, Quaternion direction)
    {
        GO = ResourceUtil.Instantiate(objectPath, pos, direction, FieldManager.Instance.FieldMaster.transform);
        if (GO.IsNull())
            return false;

        return true;
    }

    public void ObjectUpdate()
    {
        if (GO.activeSelf == false)
            return;

        if (IsPlayer)
        {
            if (HasContactObject(m_lightLayerMask))
            {
                UpdateLightAreaState(LightAreaStateEnum.Light);
            }
            else
            {
                UpdateLightAreaState(LightAreaStateEnum.Normal);
            }
        }

        UpdateComponent();
        CommandManager.Update();
    }

    private void UpdateComponent()
    {
        Anim?.ComponentUpdate();
        Attack?.ComponentUpdate();
        Item?.ComponentUpdate();
        AI?.ComponentUpdate();
    }

    public void ObjectFixedUpdate()
    {
        if (GO.activeSelf == false)
            return;

        if (GO.transform.position.z < 0)
        {
            Vector3 pos = new Vector3(GO.transform.position.x, GO.transform.position.y, 0f);
            GO.transform.position = pos;
        }

        FixedUpdateComponent();
    }

    private void FixedUpdateComponent()
    {
        Move?.ComponentFixedUpdate();
        Attack?.ComponentFixedUpdate();
    }

    public void SetObjectState(FieldObjectStateEnum state = FieldObjectStateEnum.Idle)
    {
        if (FieldObjectState == FieldObjectStateEnum.Dead)
            return;

        if (FieldObjectState == state)
            return;

        FieldObjectState = state;
    }

    public void MoveFirePoint(Vector3 lookDir, float dist)
    {
        if (lookDir.magnitude == 0.0f)
            return;

        lookDir.Normalize();
        m_firePoint.transform.localPosition = lookDir * dist;
        m_lookDir = new Vector2(Mathf.Cos(m_body.transform.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(m_body.transform.eulerAngles.z * Mathf.Deg2Rad));
    }

    public void SetLookDir(Vector3 dir)
    {
        m_lookDir = dir;
    }

    public void RotateObjectAngle(Vector3 lookDirection)
    {
        if (IsPlayer == false && IsEnemy == false)
            return;

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        m_body.transform.eulerAngles = new Vector3(0, 0, angle);

        if (m_leg.IsNotNull())
            m_leg.transform.eulerAngles = new Vector3(0, 0, angle);

        m_lookDir = new Vector2(Mathf.Cos(m_body.transform.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(m_body.transform.eulerAngles.z * Mathf.Deg2Rad));
    }

    public void RotateObjectAngle(GameObject gameObject, Vector3 lookDirection)
    {
        if (IsPlayer == false && IsEnemy == false)
            return;

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        gameObject.transform.eulerAngles = new Vector3(0, 0, angle);
    }

    public void RotateObjectAngle(float angle)
    {
        m_body.transform.eulerAngles = new Vector3(0, 0, angle);
        m_leg.transform.eulerAngles = new Vector3(0, 0, angle);

        m_lookDir = new Vector2(Mathf.Cos(m_body.transform.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(m_body.transform.eulerAngles.z * Mathf.Deg2Rad));
    }

    public void Die(float destroyTime = 0f)
    {
        if (IsDead)
            return;

        IsDead = true;
        m_destroyTime = destroyTime;

        SetObjectState(FieldObjectStateEnum.Dead);
        Effect.PlayEffect(FieldObjectStateEnum.Dead, m_body.transform.position);

        switch (ObjectType)
        {
            case FieldObjectTypeEnum.Player:
                {
                    m_collider.enabled = false;
                    Equip.Close();
                    CommandManager.ClearAllCmd();

                    var uiRestart = UIManager.Instance.GetUIObject<UIRestart>();
                    uiRestart.FadeInShow();
                    SoundManager.Instance.PlayEffectAudio("SlimeDead", volume: 0.5f);
                    var spriteRender = Body.GetComponent<SpriteRenderer>();
                    spriteRender.sortingOrder = 8;
                }
                break;

            case FieldObjectTypeEnum.Enemy:
            case FieldObjectTypeEnum.BossEnemy:
                {
                    AI.StopAI();
                }
                break;

            case FieldObjectTypeEnum.Item:
                {
                    if (m_destroyTime == 0f)
                    {
                        m_destroyTime = Item.DistroyTime;
                        DestroyObject(m_destroyTime);
                    }
                }
                break;
        }
    }

    public void DestroyObject(float time)
    {
        var timerCmd = CommandManager.GetCmd<FieldObjectInActiveCommand>((int)FieldObjectCmdType.FieldObjectInactiveTimer);
        if (timerCmd.IsNotNull())
            return;

        timerCmd = new FieldObjectInActiveCommand(this);
        timerCmd.Init(time);
        timerCmd.SetCallBack(() =>
        {
            FieldManager.Instance.DestroyFieldObject(this);
        });
    }

    private void MoveToPool(float time)
    {
        var timerCmd = CommandManager.GetCmd<FieldObjectInActiveCommand>((int)FieldObjectCmdType.FieldObjectInactiveTimer);
        if (timerCmd.IsNotNull())
            return;

        timerCmd = new FieldObjectInActiveCommand(this);
        timerCmd.Init(3.0f);
        timerCmd.SetCallBack(() =>
        {
            PoolManager.Instance.AllocateToPool(ObjName);
        });
    }

    private void UpdateHP()
    {
        switch (ObjectType)
        {
            case FieldObjectTypeEnum.BossEnemy:
                {
                    AI.UpdateBossHP();
                }
                break;
        }

        if (CurrentHP <= 0)
            Die();
    }

    public bool IsObjectInBossArea()
    {
        var layer = LayerMask.NameToLayer(DefineStrings.BossArea);
        return HasContactObject(layer);
    }

    private void SetCollider()
    {
        m_collider = m_body.GetComponent<BoxCollider2D>();
        if (m_collider.IsNull())
            m_collider = m_body.GetComponent<CircleCollider2D>();
    }

    private void SetLightLayer()
    {
        m_lightLayerMask = 1 << LayerMask.NameToLayer(DefineStrings.Light);
    }

    public bool HasContactObject(LayerMask targetLayerMask)
    {
        if (m_collider.IsNull())
            return false;

        float angle = Body.transform.eulerAngles.z;

        Collider2D[] contactObjArr = null;
        if (m_collider is BoxCollider2D)
        {
            BoxCollider2D boxCollider = m_collider as BoxCollider2D;
            contactObjArr = Physics2D.OverlapBoxAll(boxCollider.bounds.center, boxCollider.bounds.size, angle, targetLayerMask);
        }
        else if (m_collider is CircleCollider2D)
        {
            CircleCollider2D circleCollider = m_collider as CircleCollider2D;
            contactObjArr = Physics2D.OverlapCircleAll(circleCollider.bounds.center, circleCollider.radius, targetLayerMask);
        }

        if (contactObjArr.IsNull() || contactObjArr.Length == 0)
            return false;

        foreach (var obj in contactObjArr)
        {
            if (obj == m_collider)
                continue;

            return true;
        }

        return false;
    }

    public bool HasContactObject(out GameObject contactObj, LayerMask layerMask)
    {
        contactObj = null;
        if (m_collider.IsNull())
            return false;

        float angle = Body.transform.eulerAngles.z;

        Collider2D[] contactObjArr = null;
        if (m_collider is BoxCollider2D)
        {
            BoxCollider2D boxCollider = m_collider as BoxCollider2D;
            contactObjArr = Physics2D.OverlapBoxAll(boxCollider.bounds.center, boxCollider.bounds.size, angle, layerMask);
        }
        else if (m_collider is CircleCollider2D)
        {
            CircleCollider2D circleCollider = m_collider as CircleCollider2D;
            contactObjArr = Physics2D.OverlapCircleAll(circleCollider.bounds.center, circleCollider.radius, layerMask);
        }

        if (contactObjArr.IsNull() || contactObjArr.Length == 0)
            return false;

        GameObject gameObj = null;
        foreach (var obj in contactObjArr)
        {
            if (obj == m_collider)
                continue;

            gameObj = obj.gameObject;
            contactObj = gameObj;
            break;
        }

        if (gameObj.IsNull())
            return false;

        return true;
    }

    public bool HasContactObject(out GameObject contactObj, Vector2 dir, float distance)
    {
        contactObj = null;
        Vector3 rayPos = m_collider.bounds.center;

        const int raycastHitSize = 2;
        RaycastHit2D[] hitArr = new RaycastHit2D[raycastHitSize];

        LayerMask hitLayerMask = GetDefaultLayerMask();

        Physics2D.RaycastNonAlloc(rayPos, dir, hitArr,
            distance, hitLayerMask);

        GameObject gameObj = null;
        foreach (var hitObj in hitArr)
        {
            if (hitObj.collider.IsNull())
                continue;

            if (hitObj.collider.gameObject == Body)
                continue;

            gameObj = hitObj.collider.gameObject;
            contactObj = gameObj;

            return true;
        }

        return false;
    }

    public LayerMask GetDefaultLayerMask()
    {
        //ignore layermask List
        LayerMask itemLayerMask = LayerMask.GetMask(DefineStrings.Item);
        LayerMask patrolNodeLayerMask = LayerMask.GetMask(DefineStrings.PatrolNode);
        LayerMask lightLayerMask = LayerMask.GetMask(DefineStrings.Light);
        LayerMask bossAreaLayerMask = LayerMask.GetMask(DefineStrings.BossArea);
        LayerMask cameraBorderLayerMask = LayerMask.GetMask(DefineStrings.CameraBorder);
        LayerMask triggerLayerMask = LayerMask.GetMask(DefineStrings.Trigger);

        LayerMask layerMask = ~(itemLayerMask | patrolNodeLayerMask |
                                lightLayerMask | bossAreaLayerMask | cameraBorderLayerMask | triggerLayerMask);
        return layerMask;
    }

    public Vector2 GetColliderSize()
    {
        return m_collider.bounds.size;
    }

    public void UpdateActiveState(bool isActive)
    {
        if (GO.IsNull())
            return;

        GO.SetActive(isActive);
    }

    public bool IsSameType(FieldObjectTypeEnum objType, FieldObjectTypeEnum targetType)
    {
        return objType == targetType;
    }

    public Rigidbody2D GetRigidbody()
    {
        if (m_rigidbody.IsNull())
            m_rigidbody = GO.GetComponent<Rigidbody2D>();

        return m_rigidbody;
    }

    public BoxCollider2D GetBoxCollider()
    {
        if (m_collider.IsNull())
            m_collider = GO.GetComponent<Collider2D>();

        return m_collider as BoxCollider2D;
    }

    public Collider2D GetCollider2D()
    {
        return m_collider;
    }

    public GameObject GetFirePoint()
    {
        if (m_firePoint.IsNull())
            m_firePoint = UnityUtil.FindChildObject(GO, DefineStrings.FirePoint);

        return m_firePoint;
    }

    private void SetFirePoint()
    {
        if (IsPlayer == false && IsEnemy == false && IsBossEnemy == false)
            return;

        m_firePoint = UnityUtil.FindChildObject(GO, DefineStrings.FirePoint);
    }

    public void ResetPosData()
    {
        GO.transform.position = Vector3.zero;
        Body.transform.position = Vector3.zero;
        Body.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
    }

    public void SetActive(bool isActive)
    {
        GO.SetActive(isActive);
    }

    public void ReleaseFieldObject()
    {
        foreach (var component in m_components)
        {
            component.Close();
        }

        m_components.Clear();

        //managers
        EffectManager.Instance.RemoveObjectInfo(this);
        SoundManager.Instance.RemoveObjectInfo(this);
    }

    private void SetExternalData()
    {
        //stat
        if (ExternalDataManager.Instance.SetStatExtData(this) == false)
        {
            Debug.LogError($"Stat Ext Data Fail");
            return;
        }

        if (ExternalDataManager.Instance.SetObjSoundExtData(this) == false)
        {
            Debug.LogError($"Sound Ext Data Fail");
            return;
        }
    }

    public void ReceivePosition(Vector3 pos)
    {
        switch (ObjectType)
        {
            case FieldObjectTypeEnum.Enemy:
                {
                    AI.ReceivePosition(pos);
                }
                break;
        }
    }


    private void UpdateLightAreaState(LightAreaStateEnum state)
    {
        if (m_lightAreaState == state)
            return;

        switch (state)
        {
            case LightAreaStateEnum.Normal:
                {
                    ResetAllStats();
                    UpdateBodyColorInLightState(LightAreaStateEnum.Normal);
                    SoundManager.Instance.PlayEffectAudio("OutLight", volume: 0.5f);
                }
                break;

            case LightAreaStateEnum.Light:
                {
                    SetStat(StatValueEnum.MoveSpeed, m_statDic[StatValueEnum.ChangeMoveSpeed]);
                    SetStat(StatValueEnum.DodgeSpeed, m_statDic[StatValueEnum.ChangeDodgeSpeed]);
                    SetStat(StatValueEnum.ScaleValue, m_statDic[StatValueEnum.ChangeScaleValue]);

                    UpdateBodyColorInLightState(LightAreaStateEnum.Light);
                    SoundManager.Instance.PlayEffectAudio("InLight", volume: 0.5f);
                }
                break;
        }

        float newScaleValue = m_statDic[StatValueEnum.ScaleValue];
        m_body.transform.DOScale(new Vector3(newScaleValue, newScaleValue, newScaleValue), 0.5f); 
        m_lightAreaState = state;
    }

    private void UpdateBodyColorInLightState(LightAreaStateEnum lightState)
    {
        var spriteRenderer = Body.GetComponent<SpriteRenderer>();
        switch (lightState)
        {
            case LightAreaStateEnum.Normal:
                {
                    spriteRenderer.color = Color.white;
                }
                break;

            case LightAreaStateEnum.Light:
                {
                    spriteRenderer.color = Color.red;
                }
                break;
        }
    }

    public void ChangeTargetState(FieldObject obj, FieldObjectStateEnum state, float duringTime = 0f)
    {
        switch (state)
        {
            case FieldObjectStateEnum.Stun:
                {
                    obj.AI.SetStun(duringTime);
                }
                break;
        }

        obj.SetObjectState(state);
    }
}