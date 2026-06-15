using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixer mainMixer;

    [Header("UI Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    // 노출한 파라미터 이름과 정확히 일치해야 합니다.
    private const string BgmVolumeParam = "BGMVolume";
    private const string SfxVolumeParam = "SFXVolume";

    private void Start()
    {
        // 슬라이더 초기 설정
        // 믹서 볼륨 계산 시 Log10(0)은 무한대가 되므로 최소값을 0.0001로 잡습니다.

        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0.0001f;
            bgmSlider.maxValue = 1f;
            bgmSlider.onValueChanged.AddListener(SetBgmVolume);
            SetBgmVolume(bgmSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0.0001f;
            sfxSlider.maxValue = 1f;
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);
            SetSfxVolume(sfxSlider.value);
        }
    }

    public void SetBgmVolume(float value)
    {
        mainMixer.SetFloat(BgmVolumeParam, Mathf.Log10(value) * 20);
    }

    public void SetSfxVolume(float value)
    {
        mainMixer.SetFloat(SfxVolumeParam, Mathf.Log10(value) * 20);
    }

    private void OnDestroy()
    {
        if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(SetBgmVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(SetSfxVolume);
    }
}