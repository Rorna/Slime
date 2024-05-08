using System;
using UnityEngine;

public enum FieldObjectCmdType
{
    FieldObjectAttackTimer,
    FieldObjectInactiveTimer,
    FieldObjectDodgeTimer,
    None,
}

public abstract class FieldObjectCommand : Command<FieldObject>
{
    public FieldObjectCommand(FieldObject obj)
    {
        InitCommand(obj, obj.CommandManager);
    }
}

public class FieldObjectDodgeCommand : FieldObjectCommand
{
    private float m_exitTime;
    private Action m_callBack;
    private float m_remainTime;
    private float m_lifeTime;

    public float ExitTime => m_exitTime;
    public float RemainTime => m_remainTime;
    public float LifeTime => m_lifeTime;

    public FieldObjectDodgeCommand(FieldObject obj) : base(obj)
    {
    }

    public void Init(float lifeTime)
    {
        m_exitTime = Time.time + lifeTime;
        m_remainTime = lifeTime;
        m_lifeTime = lifeTime;
    }

    public void SetCallBack(Action callBack)
    {
        m_callBack = callBack;
    }

    public override int GetCmdType()
    {
        return (int)FieldObjectCmdType.FieldObjectDodgeTimer;
    }

    public override void OnUpdate()
    {
        m_remainTime -= Time.deltaTime;

        if (Time.time > m_exitTime || m_remainTime <= 0)
        {
            if (m_callBack.IsNotNull())
            {
                m_callBack.Invoke();
            }

            Exit();
        }
    }

    protected override void OnExit()
    {
    }
}

public class FieldObjectTimerCommand : FieldObjectCommand
{
    private float m_exitTime;
    private Action m_callBack;

    public float ExitTime => m_exitTime;

    public FieldObjectTimerCommand(FieldObject obj) : base(obj)
    {
    }

    public void Init(float lifeTime)
    {
        m_exitTime = Time.time + lifeTime;
    }

    public void SetCallBack(Action callBack)
    {
        m_callBack = callBack;
    }

    public override int GetCmdType()
    {
        return (int)FieldObjectCmdType.FieldObjectAttackTimer;
    }

    public override void OnUpdate()
    {
        if (Time.time > m_exitTime)
        {
            if (m_callBack.IsNotNull())
            {
                m_callBack.Invoke();
            }

            Exit();
        }
    }

    protected override void OnExit()
    {
    }
}

public class FieldObjectInActiveCommand : FieldObjectCommand
{
    private float m_exitTime;
    private Action m_callBack;

    public float ExitTime => m_exitTime;

    public FieldObjectInActiveCommand(FieldObject obj) : base(obj)
    {
    }

    public void Init(float lifeTime)
    {
        m_exitTime = Time.time + lifeTime;
    }

    public void SetCallBack(Action callBack)
    {
        m_callBack = callBack;
    }

    public override int GetCmdType()
    {
        return (int)FieldObjectCmdType.FieldObjectInactiveTimer;
    }

    public override void OnUpdate()
    {
        if (Time.time > m_exitTime)
        {
            if (m_callBack.IsNotNull())
                m_callBack.Invoke();

            Exit();
        }
    }

    protected override void OnExit()
    {
    }
}