/// <summary>
/// 자원/완제품 이동 연출용 재사용 오브젝트. ObjectPool<ResourceFlyObject>로 관리.
/// 소유: ResourceDropZone, GoodsPickupZone (각자 ObjectPool로 보유)
/// 의존: IPoolable, DOTween
/// </summary>
using System;
using DG.Tweening;
using UnityEngine;

public class ResourceFlyObject : MonoBehaviour, IPoolable
{
    [SerializeField] private float _arcHeight = 2f;

    private Tween  _tween;
    private Action _onReturn;

    public void Initialize()
    {
        _tween?.Kill();
        _tween = null;
    }

    public void Fly(Vector3 from, Vector3 to, float duration, Action onReturn)
    {
        transform.position = from;
        _onReturn = onReturn;

        // from → 호 꼭대기 → to 3점 포물선
        Vector3 mid = (from + to) * 0.5f + Vector3.up * _arcHeight;

        _tween = transform
            .DOPath(new Vector3[] { from, mid, to }, duration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnComplete(ReturnToPool);
    }

    private void ReturnToPool()
    {
        _tween = null;
        _onReturn?.Invoke();
    }
}
