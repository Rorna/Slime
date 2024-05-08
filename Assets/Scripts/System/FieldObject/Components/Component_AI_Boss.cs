using System.Collections.Generic;
using UnityEngine;

public partial class Component_AI
{
    private BoxCollider2D m_bossAreaCollider;
    private List<EnemyAttackTypeEnum> m_attackTypeList;
    private int m_changeRate;

    public BoxCollider2D BossAreaCollider => m_bossAreaCollider;

    public void UpdateBossHP()
    {
        if (m_obj.CurrentHP <= 0)
            return;

        if (m_attackTypeList.Count == 0 || m_changeRate == 0)
            return;

        if (m_obj.IsBossEnemy == false)
            return;

        int currentIndex = ((int)m_obj.StatDic[StatValueEnum.MaxHP] - (int)m_obj.StatDic[StatValueEnum.CurrentHP]) / m_changeRate;
        if (currentIndex >= 0 && currentIndex < m_attackTypeList.Count)
        {
            var attackType = m_attackTypeList[currentIndex];
            if (attackType == m_enemyAttackType)
                return;

            SetInvincible();
            m_obj.AI.PauseAI(2.0f);
            m_obj.AI.SetEnemyAttackType(attackType);
        }
    }

    private FieldObjectStateEnum m_preState;

    private void SetInvincible(float duringTime = 2f)
    {
        m_preState = m_obj.FieldObjectState;
        m_obj.SetObjectState(FieldObjectStateEnum.Invincible);
        var cmd = new FieldObjectTimerCommand(m_obj);
        cmd.Init(duringTime);
        cmd.SetCallBack(() =>
        {
            m_obj.SetObjectState(m_preState);
        });
    }

    private void SetBossData(List<EnemyAttackTypeEnum> list)
    {
        if (list.Contains(EnemyAttackTypeEnum.None))
            return;

        m_attackTypeList = new List<EnemyAttackTypeEnum>(list);
        m_changeRate = (int)m_obj.StatDic[StatValueEnum.MaxHP] / m_attackTypeList.Count;
        SetBossAreaCollider();
    }

    private void SetBossAreaCollider()
    {
        Collider2D[] contactObjArr = null;
        float angle = m_obj.Body.transform.eulerAngles.z;
        var collider = m_obj.GetBoxCollider();
        contactObjArr = Physics2D.OverlapBoxAll(collider.bounds.center, collider.bounds.size, angle);

        if (contactObjArr.IsNull() || contactObjArr.Length == 0)
            return;

        foreach (var obj in contactObjArr)
        {
            if (obj == collider)
                continue;

            m_bossAreaCollider = obj as BoxCollider2D;
            break;
        }
    }
}