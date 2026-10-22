using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Color _backgroundColor;
    [SerializeField] private float _imageFadeTime;
    [SerializeField] private TMP_Text _deathText;
    [SerializeField] private Color _textColor;
    [SerializeField] private float _textDelay;
    private WaitForSeconds _waitTextDelay;
    [SerializeField] private float _textFadeTime;
    [SerializeField] private UIButton _mainMenuButton;
    [SerializeField] private CanvasGroup _buttonCanvasGroup;
    [SerializeField] private float _buttonDelay;
    private WaitForSeconds _waitButtonDelay;
    [SerializeField] private float _buttonFadeTime;

    private void Awake()
    {
        _waitTextDelay = new WaitForSeconds(_textDelay);
        _waitButtonDelay = new WaitForSeconds(_buttonDelay);
    }

    public void Show()
    {
        SceneController.Instance.PlayerController.ToggleAllInputs(false);
        SceneController.Instance.PlayerController.HudController.gameObject.SetActive(false);
        SceneController.Instance.DisableEnemies();
        SceneController.Instance.ShouldResetSave(true);
        StartCoroutine(ShowDeathScreen());
    }

    private void Hide()
    {
        _deathText.gameObject.SetActive(false);
        _buttonCanvasGroup.gameObject.SetActive(false);
        _buttonCanvasGroup.alpha = 0;
        _buttonCanvasGroup.interactable = false;
        Color startColor = _backgroundColor;
        startColor.a = 0;
        _backgroundImage.color = startColor;
        _backgroundImage.gameObject.SetActive(false);
    }
    
    private IEnumerator ShowDeathScreen()
    {
        _deathText.gameObject.SetActive(false);
        _buttonCanvasGroup.gameObject.SetActive(false);
        _buttonCanvasGroup.alpha = 0;
        _buttonCanvasGroup.interactable = false;
        Color startColor = _backgroundColor;
        startColor.a = 0;
        _backgroundImage.color = startColor;
        _backgroundImage.gameObject.SetActive(true);

        for (float i = 0; i < _imageFadeTime; i += Time.deltaTime)
        {
            _backgroundImage.color = Color.Lerp(startColor, _backgroundColor, i / _imageFadeTime);
            
            yield return null;
        }

        _backgroundImage.color = _backgroundColor;

        yield return _waitTextDelay;
        
        startColor = _textColor;
        startColor.a = 0;
        _deathText.color = startColor;
        _deathText.gameObject.SetActive(true);
        
        for (float i = 0; i < _textFadeTime; i += Time.deltaTime)
        {
            _deathText.color = Color.Lerp(startColor, _textColor, i / _textFadeTime);
            
            yield return null;
        }

        _deathText.color = _textColor;
        
        yield return _waitButtonDelay;
        
        _buttonCanvasGroup.alpha = 0;
        _buttonCanvasGroup.interactable = false;
        _buttonCanvasGroup.gameObject.SetActive(true);
        
        for (float i = 0; i < _buttonFadeTime; i += Time.deltaTime)
        {
            _buttonCanvasGroup.alpha = Mathf.Lerp(0, 1, i / _buttonFadeTime);
            
            yield return null;
        }

        _buttonCanvasGroup.alpha = 1;
        _buttonCanvasGroup.interactable = true;
        PlayerController.ToggleCursor(true);
    }
    
    public void OnMainMenu()
    {
        Hide();
        SceneController.Instance.LoadMainMenuScene();
    }
}