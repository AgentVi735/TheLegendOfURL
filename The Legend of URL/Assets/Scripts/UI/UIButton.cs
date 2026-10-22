using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : Button, ICancelHandler
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Material _defaultMat;
    [SerializeField] private Material _hoverMat;
    [SerializeField] private float _matLerpTime;
    
    [Serializable] public class ButtonBackEvent : UnityEvent {}
    [SerializeField] private ButtonBackEvent _onBack = new ButtonBackEvent();

    private Coroutine lerpCoroutine;

    public void UpdateText(string text) => _text.text = text;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        ChangeMaterial(true);
        base.OnPointerEnter(eventData);
    }
    
    public override void OnPointerExit(PointerEventData eventData)
    {
        ChangeMaterial(false);
        base.OnPointerExit(eventData);
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

    public override void OnPointerClick(PointerEventData eventData)
    {
        Press();
        base.OnPointerClick(eventData);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        Press();
        base.OnSubmit(eventData);
    }

    private void Press()
    {
        if (lerpCoroutine != null)
            StopCoroutine(lerpCoroutine);
        _text.fontMaterial = _defaultMat;
        SceneController.Instance.AudioManager.PlayUIClickSfx();
    }
    
    public override void OnSelect(BaseEventData eventData)
    {
        ChangeMaterial(true);
        base.OnSelect(eventData);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        ChangeMaterial(false);
        base.OnDeselect(eventData);
    }

    private void ChangeMaterial(bool toggle)
    {
        if (lerpCoroutine != null)
            StopCoroutine(lerpCoroutine);
        StartCoroutine(LerpMaterial(toggle));
    }

    public void OnCancel(BaseEventData eventData)
    {
        _onBack?.Invoke();
    }
}