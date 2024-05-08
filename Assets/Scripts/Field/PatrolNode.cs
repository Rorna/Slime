using UnityEngine;

public class PatrolNode : MonoBehaviour
{
    public PatrolNode m_nextNode;

    public Vector3 GetNextNodePosition()
    {
        return m_nextNode.GetPosition();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.25f);
        if (m_nextNode != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(GetPosition(), GetNextNodePosition());
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}