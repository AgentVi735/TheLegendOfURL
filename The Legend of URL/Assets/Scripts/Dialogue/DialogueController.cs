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

    public void Initialise()
    {
        OghamStoryCatalog.Build();
        _story = Storyteller.GetStory();
        _hud.Initialise();
        SaveManager.Instance.LoadStory();
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

        SaveManager.Instance.CanSave = false;
        OnOpenStory?.Invoke();
        OnOpenEntry?.Invoke();
    }

    public void Continue()
    {
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
        SaveManager.Instance.CanSave = true;
        SaveManager.Instance.SaveStory();
        _story.Close();
        OnCloseStory?.Invoke();
        
        int shouldUnlockCombat = _story.NarrativeState.GetInt("Unlocks.Combat");
        Debug.Log(shouldUnlockCombat);
        if (shouldUnlockCombat > 0)
        {
            SceneController.Instance.PlayerController.UnlockCombat();
            _story.NarrativeState.SetInt("Unlocks.Combat", 0);
        }
        
        int isIntroFinished = _story.NarrativeState.GetInt("Dialogue.Intro.Finished");
        if (isIntroFinished <= 0) return;
        SceneController.Instance.IntroScreen.Hide();
        _story.NarrativeState.SetInt("Dialogue.Intro.Finished", 0);
    }

    public void OnDeviceChange(ControlScheme controlScheme) => _hud.OnDeviceChanged(controlScheme);
}