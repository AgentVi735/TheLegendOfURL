using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Material _defaultMat;
    [SerializeField] private Material _hoverMat;
    [SerializeField] private float _matLerpTime;

    private Coroutine lerpCoroutine;

    public void UpdateText(string text)
    {
        _text.text = text;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (lerpCoroutine != null)
            StopCoroutine(lerpCoroutine);
        StartCoroutine(LerpMaterial(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (lerpCoroutine != null)
            StopCoroutine(lerpCoroutine);
        StartCoroutine(LerpMaterial(false));
    }

    private IEnumerator LerpMaterial(bool toHover)
    {
        Material startMat = toHover ? _defaultMat : _hoverMat;
        Material endMat = toHover ? _hoverMat : _defaultMat;

        _text.fontMaterial = startMat;
        
        for (float i = 0; i < _matLerpTime; i += Time.deltaTime)
        {
            _text.fontMaterial.Lerp(startMat, endMat, i / _matLerpTime);
            yield return null;
        }
        
        _text.fontMaterial = endMat;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (lerpCoroutine != null)
            StopCoroutine(lerpCoroutine);
        _text.fontMaterial = _defaultMat;
    }
}