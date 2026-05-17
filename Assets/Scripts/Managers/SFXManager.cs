/// <summary>
/// null-safe AudioSource.PlayOneShot 래퍼. 에셋 없으면 무시.
/// 소유: HUDController (음소거 연동)
/// 의존: 없음
/// </summary>
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [SerializeField] private AudioSource _audioSource;

    [Header("SFX 클립 (없으면 무시)")]
    [SerializeField] private AudioClip _miningClip;
    [SerializeField] private AudioClip _resourceDropClip;    // 자원 → 버퍼 투입
    [SerializeField] private AudioClip _goodsPickupClip;     // 완제품 버퍼 → 쟁반 픽업
    [SerializeField] private AudioClip _goodsDropOffClip;    // 쟁반 → 판매 책상 내려놓기
    [SerializeField] private AudioClip _productionClip;
    [SerializeField] private AudioClip _salesClip;
    [SerializeField] private AudioClip _upgradePayClip;      // 업그레이드 돈 투입 틱
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

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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
