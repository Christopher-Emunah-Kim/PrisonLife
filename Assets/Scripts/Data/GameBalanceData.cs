/// <summary>
/// 경제/채굴/생산/판매/업그레이드 밸런스 수치를 보관하는 ScriptableObject.
/// 소유: ProductionManager, PrisonManager, 각 UpgradeZone, MiningZone
/// 의존: 없음 (순수 데이터)
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MiningLevelData
{
    public float tickInterval;      // 채굴 틱 간격
    public int   backpackMax;       // 해당 단계 백팩 자원 MAX
}

[CreateAssetMenu(fileName = "GameBalanceData", menuName = "PrisonLife/GameBalanceData")]
public class GameBalanceData : ScriptableObject
{
    [Header("백팩 용량")]
    public int backpackMoneyMax;    // 백팩 돈 MAX

    [Header("채굴 업그레이드 단계별 수치")]
    // index 0 = 1단계(곡괭이), 1 = 2단계(드릴), 2 = 3단계(트랙터)
    public List<MiningLevelData> miningLevels = new List<MiningLevelData>
    {
        new MiningLevelData { tickInterval = 0.5f,  backpackMax = 20 },
        new MiningLevelData { tickInterval = 0.33f, backpackMax = 30 },
        new MiningLevelData { tickInterval = 0.17f, backpackMax = 60 },
    };

    [Header("채굴 그리드")]
    public float miningRegenTime = 3f;

    [Header("생산")]
    public int   resourceBufferMax = 30;
    public int   goodsBufferMax = 20;
    public float productionTime = 2f;

    [Header("판매")]
    public int   salesDeskMax = 30;
    public int   goodsPrice = 10;

    [Header("업그레이드 비용")]
    public int   drillUpgradeCost = 50;
    public int   tractorUpgradeCost = 150;
    public int   miningWorkerCost = 100;
    public int   salesWorkerCost = 80;
    public int   prisonExpandCost = 200;

    [Header("감옥 수용")]
    public int   prisonInitialCapacity = 20;
    public int   prisonExpandedCapacity = 40;

    // 범위 초과 방어: 마지막 단계 데이터를 반환
    public MiningLevelData GetMiningLevel(int levelIndex)
    {
        int clamped = Mathf.Clamp(levelIndex, 0, miningLevels.Count - 1);
        return miningLevels[clamped];
    }
}
