using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFullscreenSwitch : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _text;

    [SerializeField] private FullScreenMode _currentSelectedMode;
    [SerializeField] private FullScreenMode[] _modes;
    private int _idx;

    public void SetCurrent(FullScreenMode mode)
    {
        for (int i = 0; i < _modes.Length; i++)
            if (_modes[i] == mode)
                _idx = i;
        UpdateCurrent(mode);
    }
    
    public void SetCurrent(int idx)
    {
        _idx = idx;
        if (_idx >= _modes.Length)
            _idx = 0;
        UpdateCurrent(_modes[_idx]);
    }

    public void Next()
    {
        _idx++;
        if (_idx >= _modes.Length)
            _idx = 0;
        UpdateCurrent(_modes[_idx]);
    }

    private void UpdateCurrent(FullScreenMode mode)
    {
        _currentSelectedMode = mode;
        Screen.fullScreenMode = _currentSelectedMode;
        _text.text = mode.ToString();
    }
}