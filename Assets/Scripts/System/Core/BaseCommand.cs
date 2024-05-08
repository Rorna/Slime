public abstract class Command<T>
{
    public T m_obj;
    private bool m_exit = false;
    private bool m_isEnd = false;

    public abstract int GetCmdType();

    protected abstract void OnExit();

    public abstract void OnUpdate();

    protected void InitCommand(T obj, CommandManager<T> cmdManager)
    {
        m_obj = obj;
        cmdManager.AddCommand(this);
    }

    public bool IsExit()
    {
        return m_exit;
    }

    public void Exit()
    {
        m_exit = true;
        if (m_isEnd == false)
        {
            m_isEnd = true;
            OnExit();
        }
    }

    public void Update()
    {
        OnUpdate();
    }
}