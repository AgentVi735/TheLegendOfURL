using Heathen.GameplayTags;
using Heathen.Ogham;
using UnityEngine;
using UnityEngine.Events;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private DialogueHUD _hud;

    private OghamSession _story;

    public UnityEvent OnOpenStory;
    public UnityEvent OnOpenEntry;
    public UnityEvent OnCloseStory;

    private void Awake()
    {
        Initialise();
    }

    private void Initialise()
    {
        _story = Storyteller.GetStory();
        _hud.Initialise();
    }

    public void OpenStory(GameplayTag storyTag)
    {
        if (!storyTag.IsValid)
        {
            Debug.LogError($"GameplayTag {storyTag} is invalid.");
            return;
        }
        bool hasNode = Storyteller.Enter(storyTag);
        if (!hasNode)
        {
            Debug.LogError($"Couldn't load node with tag: {storyTag}");
            return;
        }
        
        OnOpenStory?.Invoke();
        OnOpenEntry?.Invoke();
    }

    public void OnContinueButton()
    {
        _hud.ToggleContinueButton(false);

        if (_story.CurrentOptions.Count > 0)
            OpenNextNode();
        else
            CloseStory();
    }

    private void OpenNextNode()
    {
        switch (_story.CurrentOptions.Count)
        {
            case > 1:
                Debug.LogWarning($"Node {_story.CurrentNode.Tag} has multiple options. Only the first option will be used. Please ensure that there's only 1 option total.");
                break;
            case 0:
                Debug.LogError($"Node {_story.CurrentNode.Tag} has no options even though it should continue the dialogue. Please ensure the flow of the story is correct.");
                return;
        }

        _story.Choose(_story.CurrentOptions[0].Tag);
        OnOpenEntry?.Invoke();
    }

    private void CloseStory()
    {
        OnCloseStory?.Invoke();
    }
}