using System;
using UnityEngine;

public class Timer
{
    private float m_exitTime;
    private Action m_callBack;
    private bool m_isFinished;
    private bool m_isInterval;
    private float m_intervalTime;
    public bool IsFinished => m_isFinished;

    public void Init(float lifeTime, bool isInterval = false)
    {
        m_isFinished = false;
        m_exitTime = Time.time + lifeTime;
        m_isInterval = isInterval;
        m_intervalTime = isInterval ? lifeTime : 0f;
    }

    public void SetCallBack(Action callBack)
    {
        m_callBack = callBack;
    }

    public void UpdateTimer()
    {
        if (m_isFinished)
            return;

        if (Time.time > m_exitTime)
        {
            if (m_callBack.IsNotNull())
                m_callBack.Invoke();

            if (m_isInterval)
            {
                m_exitTime = Time.time + m_intervalTime;
            }
            else
            {
                Exit();
            }
        }
    }

    public void Exit()
    {
        m_isFinished = true;
        m_callBack = null;
        m_exitTime = 0f;
        m_isInterval = false;
    }

    public void StopTimer()
    {
        m_exitTime = 0f;
        m_isFinished = true;
        m_isInterval = false;
    }
}