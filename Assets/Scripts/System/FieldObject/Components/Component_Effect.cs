using UnityEngine;

public class Component_Effect : FieldObject_Components
{
    public Component_Effect(FieldObject obj) : base(obj)
    {
    }

    public void PlayEffect(FieldObjectStateEnum state, Vector2 pos)
    {
        EffectManager.Instance.PlayEffect(m_obj, state, pos);
    }

    public override void Close()
    {
    }

    public override void InitComponent()
    {
        if (ExternalDataManager.Instance.SetEffectExtData(m_obj) == false)
        {
            Debug.LogError($"Effect Ext Data Fail");
            return;
        }
    }
}