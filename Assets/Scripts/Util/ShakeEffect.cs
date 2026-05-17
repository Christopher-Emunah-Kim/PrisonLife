/// <summary>
/// 부착된 GameObject를 랜덤 오프셋으로 진동시키는 범용 연출 컴포넌트.
/// Play(1회성) / Loop(무한) / Stop API 제공.
/// 소유: 진동 연출이 필요한 GameObject에 직접 부착
/// 의존: 없음
/// </summary>
using System.Collections;
using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
    [SerializeField] private float _defaultDuration  = 0.5f;
    [SerializeField] private float _defaultMagnitude = 0.05f;

    private Coroutine _shakeCoroutine;
    private Vector3   _originalLocalPosition;

    private void Awake()
    {
        _originalLocalPosition = transform.localPosition;
    }

    // 1회성 진동 (감옥 만원 연출 등)
    public void Play()
    {
        Play(_defaultDuration, _defaultMagnitude);
    }

    public void Play(float duration, float magnitude)
    {
        StopShake();
        _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude, loop: false));
    }

    // 무한 루프 진동 (생산 라인 가동 중 연출)
    public void Loop()
    {
        Loop(_defaultMagnitude);
    }

    public void Loop(float magnitude)
    {
        StopShake();
        _shakeCoroutine = StartCoroutine(ShakeRoutine(0f, magnitude, loop: true));
    }

    // 진동 중단 + 원위치 복원
    public void Stop()
    {
        StopShake();
        transform.localPosition = _originalLocalPosition;
    }

    private void StopShake()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null;
        }
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude, bool loop)
    {
        float elapsed = 0f;

        while (loop || elapsed < duration)
        {
            // XZ 평면 진동 (Y축 부유 방지)
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetZ = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = _originalLocalPosition + new Vector3(offsetX, 0f, offsetZ);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalLocalPosition;
        _shakeCoroutine = null;
    }
}
