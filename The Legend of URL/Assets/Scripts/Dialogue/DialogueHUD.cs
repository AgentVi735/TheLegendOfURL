using System.Collections;
using Heathen.Ogham;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueController _controller;
    [SerializeField] private GameObject _hudObject;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text _speakerNameText;
    [SerializeField] private TMP_Text _contentText;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Image _continueIndicatorImage;

    [Header("Options")]
    [SerializeField] private float _textSpeed;
    [SerializeField] private float _fastTextSpeed;
    private WaitForSeconds _waitTextSpeed;

    private bool _hasFinished;
    private bool _isFast;
    
    private OghamSession _story;
    private StoryNode _currentNode;

    public void Initialise()
    {
        _story = Storyteller.GetStory();
        _controller.OnOpenStory.AddListener(OpenHUD);
        _controller.OnOpenEntry.AddListener(ShowNewEntry);
        _controller.OnCloseStory.AddListener(CloseHUD);
    }

    private void OnDestroy()
    {
        _controller?.OnOpenStory?.RemoveListener(OpenHUD);
        _controller?.OnOpenEntry?.RemoveListener(ShowNewEntry);
        _controller?.OnCloseStory?.RemoveListener(CloseHUD);
    }

    private void OpenHUD()
    {
        _speakerNameText.text = "";
        _contentText.text = "";
        _hasFinished = false;
        _isFast = false;
        ToggleContinueImage(false);
        _hudObject.SetActive(true);
    }

    private void CloseHUD()
    {
        _hudObject.SetActive(false);
    }
    
    private void ShowNewEntry()
    {
        _currentNode = _story.CurrentNode;
        UpdateSpeakerName(_currentNode.GetText(0));
        UpdateContentText(_currentNode.GetText(1));
    }

    private void UpdateSpeakerName(string speakerName)
    {
        if (string.IsNullOrEmpty(speakerName))
            return;
        
        _speakerNameText.text = speakerName;
        _speakerNameText.gameObject.SetActive(true);
    }

    private void UpdateContentText(string contentText)
    {
        if (string.IsNullOrEmpty(contentText))
        {
            _contentText.gameObject.SetActive(false);
            ToggleContinueImage(true);
            return;
        }
        _contentText.text = "";
        _contentText.gameObject.SetActive(true);
        StartCoroutine(ContentTextAnimation(contentText));
    }

    private IEnumerator ContentTextAnimation(string givenContent)
    {
        _hasFinished = false;
        _isFast = false;
        _waitTextSpeed = new WaitForSeconds(_textSpeed);
        int textLength = givenContent.Length;
        for (int i = 1; i < textLength + 1; i++)
        {
            string text = givenContent[..i];
            if (i < textLength)
            {
                if (givenContent[i - 1].ToString() == "<")
                {
                    int charsTillEnd = 0;
                    while (true)
                    {
                        charsTillEnd++;
                        if (givenContent[i - 1 + charsTillEnd].ToString() == ">")
                            break;
                    }

                    i += charsTillEnd;
                    text = givenContent[..i];
                }
            }

            _contentText.text = text;
            yield return _waitTextSpeed;
        }

        _hasFinished = true;
        ToggleContinueImage(true);
    }

    private void FastTyping()
    {
        _isFast = true;
        _waitTextSpeed = new WaitForSeconds(_fastTextSpeed);
    }

    private void ToggleContinueImage(bool toggle)
    {
        _continueIndicatorImage.gameObject.SetActive(toggle);
    }

    public void OnContinueButton()
    {
        if (_hasFinished)
        {
            ToggleContinueImage(false);
            _controller.Continue();
            _hasFinished = false;
        }
        else if (!_isFast)
            FastTyping();
    }
}