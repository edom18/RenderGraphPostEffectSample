using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParameterSlider : MonoBehaviour
{
    public event System.Action<float> OnValueChanged; 
    
    [SerializeField] private TMP_Text _view;
    [SerializeField] private Slider _slider;

    public float Value
    {
        get => _slider.value;
        set
        {
            _slider.value = value;
            UpdateView();
        }
    }

    private void Awake()
    {
        _slider.onValueChanged.AddListener(HandleValueChanged);
    }

    private void HandleValueChanged(float value)
    {
        UpdateView();
        OnValueChanged?.Invoke(value);
    }

    private void UpdateView()
    {
        _view.text = _slider.value.ToString("F2");
    }
}
