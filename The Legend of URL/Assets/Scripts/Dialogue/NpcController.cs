using Heathen.GameplayTags;
using UnityEngine;

public class NpcController : MonoBehaviour
{
    [SerializeField] private GameplayTag _storyTag;

    private DialogueController _dialogueController;

    private void Awake()
    {
        _dialogueController = SceneController.Instance.DialogueController;
    }

    public void StartStory()
    {
        SaveManager.Instance.Save();
        _dialogueController.OpenStory(_storyTag);
    }
}