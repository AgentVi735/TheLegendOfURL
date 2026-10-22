using Heathen.GameplayTags;
using UnityEngine;

public class IntroScreen : MonoBehaviour
{
    [SerializeField] private GameplayTag _introTag;
    
    public void Show()
    {
        gameObject.SetActive(true);
        SceneController.Instance.PlayerController.ToggleAllInputs(false);
        SceneController.Instance.PlayerController.HudController.gameObject.SetActive(true);
        SceneController.Instance.DialogueController.OpenStory(_introTag);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        SceneController.Instance.PlayerController.ToggleAllInputs(true);
        SaveManager.Instance.Save();
    }
}