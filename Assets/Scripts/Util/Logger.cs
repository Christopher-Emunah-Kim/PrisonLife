using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Logging 로그 래퍼 유틸리티 클래스
 * Unity 에디터/Development 빌드에서만 로그 출력
 * Error는 항상 출력
 */
public static class Logger 
{
    // 카테고리 ON/OFF 필터
    [SerializeField] public static bool EnableVerbose = true;

    public static void Log(string category, string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[{category}] {message}");
#endif
    }

    public static void Warn(string category, string message)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[{category}] {message}");
#endif
    }

    public static void Error(string category, string message)
    {
        // Error는 릴리즈에서도 출력 (크리티컬)
        Debug.LogError($"[{category}] {message}");
    }
}
