using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveEffectController : MonoBehaviour
{
    [SerializeField] private Material _material;
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

    private static readonly int s_needsEffectId = Shader.PropertyToID("_NeedsEffect");
    private static readonly int s_intensityId = Shader.PropertyToID("_Intensity");
    private static readonly int s_offsetId = Shader.PropertyToID("_Offset");
    private static readonly int s_waveId = Shader.PropertyToID("_Wave");
    private static readonly int s_speedId = Shader.PropertyToID("_Speed");

    private void Awake()
    {
        Stop();

        _playButton.onClick.AddListener(HandlePlayButtonClicked);
        _stopButton.onClick.AddListener(HandleStopButtonClicked);

        _openCloseButton.onClick.AddListener(HandleOpenCloseClicked);

        _waveSlider.Value = _material.GetFloat(s_waveId);
        _speedSlider.Value = _material.GetFloat(s_speedId);
        _intensitySlider.Value = _material.GetFloat(s_intensityId);
        _offsetXSlider.Value = _material.GetVector(s_offsetId).x;
        _offsetYSlider.Value = _material.GetVector(s_offsetId).y;

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
        _material.SetInt(s_needsEffectId, 1);
    }

    private void Stop()
    {
        _material.SetInt(s_needsEffectId, 0);
    }

    private void SetWave(float value)
    {
        _material.SetFloat(s_waveId, value);
    }

    private void SetSpeed(float value)
    {
        _material.SetFloat(s_speedId, value);
    }

    private void SetIntensity(float value)
    {
        _material.SetFloat(s_intensityId, value);
    }

    private void SetOffset(Vector2 offset)
    {
        _material.SetVector(s_offsetId, offset);
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