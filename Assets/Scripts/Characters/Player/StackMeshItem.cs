/// <summary>
/// 백팩/쟁반 적층 더미 메시용 풀링 컴포넌트. ObjectPool<StackMeshItem>으로 관리.
/// 소유: InventoryComponent
/// 의존: IPoolable
/// </summary>
using UnityEngine;

public class StackMeshItem : MonoBehaviour, IPoolable
{
    public void Initialize()
    {
        transform.localPosition = Vector3.zero;
    }
}
