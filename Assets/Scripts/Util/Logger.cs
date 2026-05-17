/// <summary>
/// 로그 래퍼 유틸리티. Log/Warn은 에디터·개발빌드 전용, Error는 항상 출력.
/// 소유: 전역 static
/// 의존: 없음
/// </summary>
using System.Diagnostics;
using UnityEngine;

public static class Logger
{
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string category, string message)
    {
        UnityEngine.Debug.Log($"[{category}] {message}");
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Warn(string category, string message)
    {
        UnityEngine.Debug.LogWarning($"[{category}] {message}");
    }

    public static void Error(string category, string message)
    {
        // Error는 릴리즈에서도 출력 (크리티컬)
        UnityEngine.Debug.LogError($"[{category}] {message}");
    }
}
