using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    private const string MasterKey = "audio_master";
    private const string BgmKey = "audio_bgm";
    private const string SfxKey = "audio_sfx";

    [Header("References")]
    [SerializeField] private GameObject panelOptions;
    [SerializeField] private Button btnOptionsOpen;
    [SerializeField] private Button btnOptionsClose;

    [Header("Sliders")]
    [SerializeField] private Slider sldMaster;
    [SerializeField] private Slider sldBgm;
    [SerializeField] private Slider sldSfx;

    [Header("Value Text")]
    [SerializeField] private Text txtMasterValue;
    [SerializeField] private Text txtBgmValue;
    [SerializeField] private Text txtSfxValue;

    private void Awake()
    {
        BindButtons();
        ConfigureSliders();
    }

    private void Start()
    {
        LoadValues();
        CloseOptions();
    }

    private void BindButtons()
    {
        if (btnOptionsOpen != null)
        {
            btnOptionsOpen.onClick.AddListener(OpenOptions);
        }

        if (btnOptionsClose != null)
        {
            btnOptionsClose.onClick.AddListener(CloseOptions);
        }
    }

    private void ConfigureSliders()
    {
        SetupSlider(sldMaster, OnMasterChanged);
        SetupSlider(sldBgm, OnBgmChanged);
        SetupSlider(sldSfx, OnSfxChanged);
    }

    private static void SetupSlider(Slider slider, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.onValueChanged.AddListener(callback);
    }

    public void OpenOptions()
    {
        if (panelOptions != null)
        {
            panelOptions.SetActive(true);
        }
    }

    public void CloseOptions()
    {
        if (panelOptions != null)
        {
            panelOptions.SetActive(false);
        }
    }

    private void LoadValues()
    {
        SetSliderWithoutNotify(sldMaster, PlayerPrefs.GetFloat(MasterKey, 1f));
        SetSliderWithoutNotify(sldBgm, PlayerPrefs.GetFloat(BgmKey, 1f));
        SetSliderWithoutNotify(sldSfx, PlayerPrefs.GetFloat(SfxKey, 1f));

        UpdateValueText(txtMasterValue, sldMaster != null ? sldMaster.value : 1f);
        UpdateValueText(txtBgmValue, sldBgm != null ? sldBgm.value : 1f);
        UpdateValueText(txtSfxValue, sldSfx != null ? sldSfx.value : 1f);
    }

    private static void SetSliderWithoutNotify(Slider slider, float value)
    {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(Mathf.Clamp01(value));
        }
    }

    private void OnMasterChanged(float value)
    {
        SaveValue(MasterKey, value);
        UpdateValueText(txtMasterValue, value);
    }

    private void OnBgmChanged(float value)
    {
        SaveValue(BgmKey, value);
        UpdateValueText(txtBgmValue, value);
    }

    private void OnSfxChanged(float value)
    {
        SaveValue(SfxKey, value);
        UpdateValueText(txtSfxValue, value);
    }

    private static void SaveValue(string key, float value)
    {
        PlayerPrefs.SetFloat(key, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }

    private static void UpdateValueText(Text label, float value)
    {
        if (label != null)
        {
            label.text = $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
        }
    }
}
