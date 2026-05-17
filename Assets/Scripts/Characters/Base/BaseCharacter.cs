/// <summary>
/// 플레이어/인부 공통 이동·회전 기반 클래스.
/// 소유: PlayerController, MiningWorker, SalesWorker (상속)
/// 의존: UnityEngine (없음)
/// </summary>
using UnityEngine;

public abstract class BaseCharacter : MonoBehaviour
{
    [SerializeField] protected float _moveSpeed     = 5f;
    [SerializeField] protected float _rotationSpeed = 10f;

    // 컷씬 중 이동 입력을 차단하는 플래그
    protected bool _inputBlocked;

    /// <summary>
    /// 월드 공간 방향 벡터로 이동 + Y축 회전. _inputBlocked=true 시 무시.
    /// </summary>
    protected void Move(Vector3 direction)
    {
        if (_inputBlocked)
        {
            return;
        }

        if (direction.sqrMagnitude < 0.001f) //데드존방어
        {
            return;
        }

        transform.position += direction * (_moveSpeed * Time.deltaTime);

        // 이동 방향으로 부드러운 Y축 회전
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    public void SetInputBlocked(bool blocked)
    {
        _inputBlocked = blocked;
    }
}
