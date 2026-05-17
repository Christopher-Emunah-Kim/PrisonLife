/// <summary>
/// 감옥 수용 Zone. 죄수 트리거 감지 → PrisonManager.TryAdmit() → 수용 메시 그리드에 순서대로 ON.
/// 소유: 씬 PrisonZone GameObject
/// 의존: PrisonManager, PrisonerSpawner, Prisoner, PrisonerData, GameBalanceData
/// </summary>
/// 수정 로그:
/// 2026-05-16 메시 배열 제거 → 프리팹+소켓 그리드 동적 생성
using System.Collections.Generic;
using UnityEngine;

public class PrisonZone : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private PrisonerData    _prisonerData;
    [SerializeField] private GameBalanceData _balanceData;

    [Header("메시")]
    [SerializeField] private GameObject _prisonerMeshPrefab;   // 감옥 내 수용 메시 프리팹
    [SerializeField] private Transform  _prisonSocket;         // 그리드 기준점 (좌하단)

    [Header("참조")]
    [SerializeField] private PrisonerSpawner _spawner;
    [SerializeField] private LayerMask       _prisonerLayer;

    // 동적 생성된 메시 목록
    private readonly List<GameObject> _spawnedMeshes = new List<GameObject>();
    private int _admittedCount;

    private void Awake()
    {
        SpawnAllMeshes();
    }

    private void OnEnable()
    {
        PrisonManager.OnPrisonFull     += HandlePrisonFull;
        PrisonManager.OnPrisonExpanded += HandlePrisonExpanded;
    }

    private void OnDisable()
    {
        PrisonManager.OnPrisonFull     -= HandlePrisonFull;
        PrisonManager.OnPrisonExpanded -= HandlePrisonExpanded;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_prisonerLayer.value & (1 << other.gameObject.layer)) == 0)
        {
            Logger.Warn("PrisonZone", $"레이어 불일치 — 무시: {other.gameObject.name}");
            return;
        }

        Prisoner prisoner = other.GetComponentInParent<Prisoner>();
        if (prisoner == null)
        {
            Logger.Warn("PrisonZone", $"Prisoner 컴포넌트 없음: {other.gameObject.name}");
            return;
        }

        AdmitPrisoner(prisoner);
    }

    private void AdmitPrisoner(Prisoner prisoner)
    {
        if (PrisonManager.Instance == null)
        {
            Logger.Warn("PrisonZone", "PrisonManager 인스턴스 없음");
            return;
        }

        if (!PrisonManager.Instance.TryAdmit())
        {
            Logger.Warn("PrisonZone", "감옥 만원 — 수용 거부");
            return;
        }

        // 수용 순서대로 메시 ON
        if (_admittedCount < _spawnedMeshes.Count)
        {
            _spawnedMeshes[_admittedCount].SetActive(true);
        }

        _admittedCount++;

        prisoner.HideBubble();
        _spawner?.ReturnToPool(prisoner);
    }

    // 최대 수용 인원(확장 포함) 전체 메시를 미리 생성 후 비활성화
    private void SpawnAllMeshes()
    {
        if (_prisonerMeshPrefab == null || _prisonSocket == null || _prisonerData == null || _balanceData == null)
        {
            Logger.Warn("PrisonZone", "SpawnAllMeshes: 필수 참조 미설정");
            return;
        }

        int total   = _balanceData.prisonExpandedCapacity;
        int columns = _prisonerData.prisonColumns;
        float spacingX = _prisonerData.prisonSpacingX;
        float spacingZ = _prisonerData.prisonSpacingZ;

        for (int i = 0; i < total; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector3 localPos = new Vector3(col * spacingX, 0f, row * spacingZ);
            GameObject mesh  = Object.Instantiate(_prisonerMeshPrefab, _prisonSocket);
            mesh.transform.localPosition = localPos;
            mesh.SetActive(false);
            _spawnedMeshes.Add(mesh);
        }
    }

    private void HandlePrisonFull()
    {
        _spawner?.SetNoCellAll(true);
    }

    private void HandlePrisonExpanded()
    {
        _spawner?.SetNoCellAll(false);
    }
}
