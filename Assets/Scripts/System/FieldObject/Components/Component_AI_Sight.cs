using UnityEngine;

public partial class Component_AI
{
    private float m_sightAngle;
    private float m_sightDistance;
    private LayerMask m_layerMask;

    public void InitSensor()
    {
        m_sightAngle = m_obj.StatDic[StatValueEnum.SightAngle];
        m_sightDistance = m_obj.StatDic[StatValueEnum.SightRange];
        m_layerMask = 1 << LayerMask.NameToLayer(DefineStrings.Player);
    }

    public void UpdateSensor()
    {
        GetTargetInSight();
    }

    private void GetTargetInSight()
    {
        switch (m_obj.ObjectType)
        {
            case FieldObjectTypeEnum.Enemy:
                {
                    Collider2D[] contactObjArr = Physics2D.OverlapCircleAll(m_obj.GO.transform.position, m_sightDistance, m_layerMask);
                    foreach (var obj in contactObjArr)
                    {
                        m_obj.GO.transform.position = new Vector3(m_obj.GO.transform.position.x, m_obj.GO.transform.position.y, 0f);
                        Vector3 direction = (obj.transform.position - m_obj.GO.transform.position).normalized;
                        Vector2 lookDir = m_obj.LookDir;
                        float objAngle = Vector2.Angle(direction, lookDir);

                        if (objAngle < (m_sightAngle / 2))
                        {
                            float dist = Vector2.Distance(obj.transform.position, m_obj.GO.transform.position);
                            if (dist < m_sightDistance && TargetInSight(obj.transform, m_sightDistance))
                            {
                                string objName = UnityUtil.GetObjectName(obj.gameObject);
                                var fieldObj = FieldManager.Instance.GetFieldObject(objName);

                                SetTargetObject(fieldObj);
                                SetTargetPosition(obj.transform.position);
                                m_obj.RotateObjectAngle(direction);
                            }
                        }
                    }
                }
                break;

            case FieldObjectTypeEnum.BossEnemy:
                {
                    if (m_bossAreaCollider.IsNull())
                        return;

                    var contactObjArr = Physics2D.OverlapBoxAll(m_obj.GO.transform.position, m_bossAreaCollider.size, 0, m_layerMask);
                    foreach (var obj in contactObjArr)
                    {
                        string objName = UnityUtil.GetObjectName(obj.gameObject);
                        var fieldObj = FieldManager.Instance.GetFieldObject(objName);
                        if (fieldObj.IsPlayer == false)
                            return;

                        Vector3 direction = (obj.transform.position - m_obj.GO.transform.position).normalized;
                        SetTargetObject(fieldObj);
                        SetTargetPosition(obj.transform.position);
                        m_obj.RotateObjectAngle(direction);
                    }
                }
                break;
        }
    }

    private bool TargetInSight(Transform target, float distance)
    {
        Vector3 sightPosition = m_obj.GO.transform.position;
        Vector3 dir = target.position - sightPosition;

        const int raycastHitSize = 2;
        RaycastHit2D[] hitArr = new RaycastHit2D[raycastHitSize];

        //Ignore LayerMask List
        LayerMask patrolNodeLayer = LayerMask.GetMask(DefineStrings.PatrolNode);
        LayerMask itemLayer = LayerMask.GetMask(DefineStrings.Item);
        LayerMask lightLayer = LayerMask.GetMask(DefineStrings.Light);
        LayerMask bossAreaMask = LayerMask.GetMask(DefineStrings.BossArea);

        LayerMask hitLayerMask = ~(patrolNodeLayer | itemLayer | lightLayer | bossAreaMask);
        Physics2D.RaycastNonAlloc(sightPosition, dir, hitArr, distance, hitLayerMask);
        foreach (var hitObj in hitArr)
        {
            if (hitObj.collider.gameObject == m_obj.Body)
                continue;

            if (target.gameObject == hitObj.collider.gameObject)
            {
                return true;
            }
        }

        return false;
    }
}