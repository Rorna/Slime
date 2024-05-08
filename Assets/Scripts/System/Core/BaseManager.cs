using UnityEngine;

public abstract class BaseManager : MonoBehaviour
{
    public abstract void Init();

    public abstract void Clear();

    public abstract void UpdateManager();
}