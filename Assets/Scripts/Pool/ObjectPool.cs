/// <summary>
/// IPoolable 컴포넌트를 Get/Return으로 재사용하는 제네릭 오브젝트 풀 (순수 C# 클래스).
/// 소유: PrisonerSpawner, ResourceDropZone, GoodsPickupZone, MoneyZone (생성자로 직접 생성)
/// 의존: IPoolable
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
    void Initialize();
}

public class ObjectPool<T> where T : MonoBehaviour, IPoolable
{
    private readonly Stack<T>   _pool;
    private readonly T          _prefab;
    private readonly Transform  _parent;

    /// <param name="prefab">풀링할 프리팹. null이면 풀 생성 중단.</param>
    /// <param name="poolSize">초기 생성 수량.</param>
    /// <param name="parent">Instantiate 시 부모 Transform.</param>
    public ObjectPool(T prefab, int poolSize, Transform parent)
    {
        if (prefab == null)
        {
            Logger.Error("ObjectPool", $"Prefab이 null입니다. 풀 생성 중단 ({typeof(T).Name})");
            _pool = new Stack<T>();
            return;
        }

        _prefab = prefab;
        _parent = parent;
        _pool   = new Stack<T>(poolSize);

        for (int i = 0; i < poolSize; i++)
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
        return Object.Instantiate(_prefab, _parent);
    }
}
