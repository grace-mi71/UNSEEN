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

    [Header("Audio")]
    public AudioClip bgmClip;
    public AudioClip sfxClip;
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    // ������ �Ķ���� �̸��� ��Ȯ�� ��ġ�ؾ� �մϴ�.
    private const string BgmVolumeParam = "BGMVolume";
    private const string SfxVolumeParam = "SFXVolume";

    private void Awake()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.clip = bgmClip;
        bgmSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("BGM")[0];
        bgmSource.Play();

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("SFX")[0];
    }

    public void PlayButtonSFX()
    {
        if (sfxClip != null)
            sfxSource.PlayOneShot(sfxClip);
    }

    private void Start()
    {
        // �����̴� �ʱ� ����
        // �ͼ� ���� ��� �� Log10(0)�� ���Ѵ밡 �ǹǷ� �ּҰ��� 0.0001�� ����ϴ�.

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