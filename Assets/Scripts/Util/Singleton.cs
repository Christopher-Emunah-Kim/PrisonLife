/// <summary>
/// MonoBehaviour 싱글턴 공통 베이스. Instance 보장 + 중복 Destroy 처리.
/// 소유: 싱글턴 매니저 클래스들 (상속)
/// 의존: 없음
/// </summary>
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this as T;
        OnAwake();
    }

    // 하위 클래스 초기화 진입점 — Awake override 대신 이 메서드를 사용
    protected virtual void OnAwake() { }
}
