using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeTime;
    private WaitForSeconds waitFadeTime;
    public bool IsOn { get; private set; }

    private Coroutine fadeCoroutine;

    [SerializeField] private bool startOn = true;
    [SerializeField] private Color startColor = Color.black;

    private void Awake()
    {
        fadeImage.color = startColor;
        if (startOn)
            Show();
        waitFadeTime = new WaitForSeconds(fadeTime);
    }

    public void StartFade(bool fadeIn) => StartFade(fadeIn, null, Color.black);
    public void StartFade(bool fadeIn, Action callback) => StartFade(fadeIn, callback, Color.black);
    public void StartFade(bool fadeIn, Action callback, Color fadeColor)
    {
        if (!gameObject.activeSelf || !IsOn)
            Show(fadeIn ? Color.clear : fadeColor);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(Fade(fadeIn, callback, fadeColor));
    }
    
    public WaitForSeconds GetFadeTime() => waitFadeTime;

    public void Show()
    {
        fadeImage.color = startColor;
        gameObject.SetActive(true);
        fadeImage.gameObject.SetActive(true);
        IsOn = true;
    }
    
    public void Show(Color color)
    {
        fadeImage.color = color;
        gameObject.SetActive(true);
        fadeImage.gameObject.SetActive(true);
        IsOn = true;
    }
    
    public void Hide()
    {
        fadeImage.gameObject.SetActive(false);
        IsOn = false;
    }

    private IEnumerator Fade(bool fadeIn, Action callback, Color fadeColor)
    {
        IsOn = !fadeIn;
        Color fadeStartColor = fadeIn ? fadeColor : Color.clear;
        Color fadeEndColor = fadeIn ? Color.clear : fadeColor;
        fadeImage.color = fadeStartColor;
        fadeImage.gameObject.SetActive(true);

        for (float i = 0; i <= fadeTime + Time.deltaTime; i += Time.deltaTime)
        {
            if (i > fadeTime) i = fadeTime;

            float fillAmount = i / fadeTime;

            fadeImage.color = Color.Lerp(fadeStartColor, fadeEndColor, fillAmount);

            yield return null;
        }

        fadeImage.color = fadeEndColor;
        if (fadeIn)
            fadeImage.gameObject.SetActive(false);

        callback?.Invoke();
    }
}