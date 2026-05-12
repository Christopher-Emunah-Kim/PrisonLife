/// <summary>
/// IPoolable 컴포넌트를 Get/Return으로 재사용하는 제네릭 오브젝트 풀.
/// 소유: PrisonerSpawner, ResourceDropZone, MoneyZone (각 풀 인스턴스 소유)
/// 의존: IPoolable (풀 대상 인터페이스)
/// </summary>
using System.Collections.Generic;
using UnityEngine;

// T는 Initialize()로 상태를 완전히 리셋할 수 있는 MonoBehaviour 컴포넌트여야 한다.
public interface IPoolable
{
    void Initialize();
}

public class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour, IPoolable
{
    [SerializeField] private T _prefab;
    [SerializeField] private int _poolSize = 30;

    private readonly Stack<T> _pool = new Stack<T>();

    private void Awake()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            T obj = CreateNew();
            obj.gameObject.SetActive(false);
            _pool.Push(obj);
        }
    }

    public T Get()
    {
        T obj = _pool.Count > 0 ? _pool.Pop() : CreateNew();
        obj.gameObject.SetActive(true);
        obj.Initialize();
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Push(obj);
    }

    private T CreateNew()
    {
        T obj = Instantiate(_prefab, transform);
        return obj;
    }
}
