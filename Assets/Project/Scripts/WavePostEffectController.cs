using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WaveEffectController : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _stopButton;

    [SerializeField] private ParameterSlider _waveSlider;
    [SerializeField] private ParameterSlider _speedSlider;
    [SerializeField] private ParameterSlider _intensitySlider;
    [SerializeField] private ParameterSlider _offsetXSlider;
    [SerializeField] private ParameterSlider _offsetYSlider;

    [SerializeField] private Button _openCloseButton;

    [SerializeField] private GameObject _configView;
    [SerializeField] private TMP_Text _configButtonText;
    [SerializeField] private VolumeProfile _localProfile;

    private WaveVolumeComponent Volume
    {
        get
        {
            _localProfile.TryGet(out WaveVolumeComponent component);
            return component;
        }
    }

    private void Awake()
    {
        Stop();

        _playButton.onClick.AddListener(HandlePlayButtonClicked);
        _stopButton.onClick.AddListener(HandleStopButtonClicked);
        _openCloseButton.onClick.AddListener(HandleOpenCloseClicked);

        _waveSlider.Value = Volume.Wave.value;
        _speedSlider.Value = Volume.Speed.value;
        _intensitySlider.Value = Volume.Intensity.value;
        _offsetXSlider.Value = Volume.Offset.value.x;
        _offsetYSlider.Value = Volume.Offset.value.y;

        _waveSlider.OnValueChanged += HandleWaveValueChanged;
        _speedSlider.OnValueChanged += HandleSpeedValueChanged;
        _intensitySlider.OnValueChanged += HandleIntensityValueChanged;
        _offsetXSlider.OnValueChanged += HandleOffsetXValueChanged;
        _offsetYSlider.OnValueChanged += HandleOffsetYValueChanged;
    }

    private void OnApplicationQuit()
    {
        Stop();
    }

    private void Play()
    {
        Volume.NeedsEffect.value = 1;
    }

    private void Stop()
    {
        Volume.NeedsEffect.value = 0;
    }

    private void SetWave(float value)
    {
        Volume.Wave.value = value;
    }

    private void SetSpeed(float value)
    {
        Volume.Speed.value = value;
    }

    private void SetIntensity(float value)
    {
        Volume.Intensity.value = value;
    }

    private void SetOffset(Vector2 offset)
    {
        Volume.Offset.value = offset;
    }

    private void HandlePlayButtonClicked()
    {
        Play();
    }

    private void HandleStopButtonClicked()
    {
        Stop();
    }

    private void HandleOpenCloseClicked()
    {
        if (_configView.activeSelf)
        {
            _configView.SetActive(false);
            _configButtonText.text = "|>";
        }
        else
        {
            _configView.SetActive(true);
            _configButtonText.text = "▼";
        }
    }

    private void HandleWaveValueChanged(float value)
    {
        SetWave(value);
    }

    private void HandleSpeedValueChanged(float value)
    {
        SetSpeed(value);
    }

    private void HandleIntensityValueChanged(float value)
    {
        SetIntensity(value);
    }

    private void HandleOffsetXValueChanged(float value)
    {
        SetOffset(new Vector2(_offsetXSlider.Value, _offsetYSlider.Value));
    }

    private void HandleOffsetYValueChanged(float value)
    {
        SetOffset(new Vector2(_offsetXSlider.Value, _offsetYSlider.Value));
    }
}