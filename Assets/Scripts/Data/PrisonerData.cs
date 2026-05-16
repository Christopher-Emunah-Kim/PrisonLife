/// <summary>
/// 죄수 스폰 간격·큐 크기·구매 수량 범위를 보관하는 ScriptableObject.
/// 소유: PrisonerSpawner, SalesZone
/// 의존: 없음 (순수 데이터)
/// </summary>
using UnityEngine;

[CreateAssetMenu(fileName = "PrisonerData", menuName = "PrisonLife/PrisonerData")]
public class PrisonerData : ScriptableObject
{
    [Header("스폰")]
    public float spawnInterval = 3f;    // 죄수 스폰 간격

    [Header("큐")]
    public int   maxQueueSize = 5;      // 판매 Zone 앞 대기 큐 최대 인원

    [Header("구매")]
    public int   purchaseMin = 2;       // 구매 수량 최솟값
    public int   purchaseMax = 4;       // 구매 수량 최댓값

    [Header("감옥 그리드")]
    public int   prisonColumns  = 5;    // 한 행에 배치할 메시 수
    public float prisonSpacingX = 1f;   // 열 간격
    public float prisonSpacingZ = 1f;   // 행 간격
}
