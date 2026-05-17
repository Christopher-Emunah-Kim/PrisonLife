/// <summary>
/// null-safe AudioSource.PlayOneShot 래퍼. 에셋 없으면 무시.
/// 소유: HUDController (음소거 연동)
/// 의존: Singleton<T>
/// </summary>
/// 수정 로그:
/// 2026-05-17 Singleton<T> 베이스 클래스 적용
using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{
    [SerializeField] private AudioSource _audioSource;

    [Header("SFX 클립 (없으면 무시)")]
    [SerializeField] private AudioClip _miningClip;
    [SerializeField] private AudioClip _resourceDropClip;
    [SerializeField] private AudioClip _goodsPickupClip;
    [SerializeField] private AudioClip _goodsDropOffClip;
    [SerializeField] private AudioClip _productionClip;
    [SerializeField] private AudioClip _salesClip;
    [SerializeField] private AudioClip _upgradePayClip;
    [SerializeField] private AudioClip _upgradeCompleteClip;
    [SerializeField] private AudioClip _moneyReceiveClip;

    [Header("클립별 볼륨 (0~1)")]
    [SerializeField] [Range(0f, 1f)] private float _miningVolume          = 1f;
    [SerializeField] [Range(0f, 1f)] private float _resourceDropVolume    = 1f;
    [SerializeField] [Range(0f, 1f)] private float _goodsPickupVolume     = 1f;
    [SerializeField] [Range(0f, 1f)] private float _goodsDropOffVolume    = 1f;
    [SerializeField] [Range(0f, 1f)] private float _productionVolume      = 1f;
    [SerializeField] [Range(0f, 1f)] private float _salesVolume           = 1f;
    [SerializeField] [Range(0f, 1f)] private float _upgradePayVolume      = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float _upgradeCompleteVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float _moneyReceiveVolume    = 1f;

    public void PlayMining()           => Play(_miningClip,          _miningVolume);
    public void PlayResourceDrop()     => Play(_resourceDropClip,    _resourceDropVolume);
    public void PlayGoodsPickup()      => Play(_goodsPickupClip,     _goodsPickupVolume);
    public void PlayGoodsDropOff()     => Play(_goodsDropOffClip,    _goodsDropOffVolume);
    public void PlayProduction()       => Play(_productionClip,      _productionVolume);
    public void PlaySales()            => Play(_salesClip,           _salesVolume);
    public void PlayUpgradePay()       => Play(_upgradePayClip,      _upgradePayVolume);
    public void PlayUpgradeComplete()  => Play(_upgradeCompleteClip, _upgradeCompleteVolume);
    public void PlayMoneyReceive()     => Play(_moneyReceiveClip,    _moneyReceiveVolume);

    public void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _audioSource == null)
        {
            return;
        }
        _audioSource.PlayOneShot(clip, volume);
    }

    // HUDController 음소거 버튼에서 호출
    public void SetMute(bool mute)
    {
        AudioListener.pause = mute;
    }
}
