using UnityEngine;

public class LockTrigger : MonoBehaviour
{
    [SerializeField] private GameObject m_frontLockObj;
    [SerializeField] private GameObject m_BackLockObj;
    [SerializeField] private GameObject m_fadeTrigger;

    private FieldObject m_bossObj;

    private void Update()
    {
        if (m_bossObj.IsNull())
            return;

        if (m_bossObj.StatDic[StatValueEnum.CurrentHP] <= 0)
        {
            UpdateActiveLockObj(false);
            if (m_fadeTrigger.IsNotNull())
            {
                m_fadeTrigger.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        string objName = UnityUtil.GetObjectName(collider.gameObject);
        var fieldObj = FieldManager.Instance.GetFieldObject(objName);
        if (fieldObj.IsNull())
            return;

        if (fieldObj.IsPlayer == false)
            return;

        UpdateActiveLockObj(true);

        var list = FieldManager.Instance.GetFieldObjectList(FieldObjectTypeEnum.BossEnemy);
        foreach (var obj in list)
        {
            m_bossObj = new FieldObject();
            m_bossObj = obj;
        }
    }

    private void UpdateActiveLockObj(bool active)
    {
        if (active)
        {
            if (m_frontLockObj.activeSelf == false && m_BackLockObj.activeSelf == false)
            {
                m_frontLockObj.SetActive(true);
                m_BackLockObj.SetActive(true);
                SoundManager.Instance.PlayEffectAudio("SetLock", volume: 0.7f);
                CameraManager.Instance.Shake(0.3f, 0.7f);
            }
        }
        else
        {
            if (m_frontLockObj.activeSelf && m_BackLockObj.activeSelf)
            {
                m_frontLockObj.SetActive(false);
                m_BackLockObj.SetActive(false);
                SoundManager.Instance.PlayEffectAudio("UnLock", volume: 0.7f);
                CameraManager.Instance.Shake(0.3f, 0.7f);
            }
        }
    }
}