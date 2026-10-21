using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISliderHover : Slider
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Material _defaultMat;
    [SerializeField] private Material _hoverMat;
    [SerializeField] private float _matLerpTime;

    private Coroutine lerpCoroutine;

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

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        if (lerpCoroutine != null)
            StopCoroutine(lerpCoroutine);
        StartCoroutine(LerpMaterial(true));
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);  
        if (lerpCoroutine != null)
            StopCoroutine(lerpCoroutine);
        StartCoroutine(LerpMaterial(false));
    }
}