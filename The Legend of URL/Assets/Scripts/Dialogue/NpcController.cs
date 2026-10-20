using Heathen.GameplayTags;
using UnityEngine;

public class NpcController : MonoBehaviour
{
    [SerializeField] private GameplayTag _storyTag;
    public Vector3 InteractButtonPosition => _interactButtonPosition;
    [SerializeField] private Vector3 _interactButtonPosition;

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