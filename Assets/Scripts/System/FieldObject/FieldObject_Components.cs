public abstract class FieldObject_Components
{
    public FieldObject m_obj;

    public FieldObject_Components(FieldObject obj)
    {
        m_obj = obj;
    }

    public abstract void InitComponent();

    public abstract void Close();
}