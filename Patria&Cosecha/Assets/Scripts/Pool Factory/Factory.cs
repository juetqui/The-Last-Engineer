using UnityEngine;

public abstract class Factory<T> where T : MonoBehaviour
{
    public T _prefab = default;

    public virtual T GetObject()
    {
        return Object.Instantiate(_prefab);
    }
}
