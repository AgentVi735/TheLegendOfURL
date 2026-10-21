using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

public class PlayerHUDController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerController controller;
    
    [Header("UI References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider bossHealthBar;
    [SerializeField] private Image interactButtonImage;
    [SerializeField] private RectTransform interactButtonTransform;

    [Header("Interact Button Sprites")]
    [SerializeField] private Sprite keyboardInteractSprite;
    [SerializeField] private Sprite switchInteractSprite;
    [SerializeField] private Sprite psInteractSprite;
    [SerializeField] private Sprite xboxInteractSprite;

    public void Initialise(int maxHealth)
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
        healthBar.gameObject.SetActive(true);
    }
    
    public void UpdateHealthBar(int health)
    {
        healthBar.value = health >= 0 ? health : 0;
        Debug.Log($"Updated player health bar to {healthBar.value}");
    }

    public void SetInteractButtonPosition(Vector3 pos)
    {
        interactButtonTransform.position = pos;
    }

    public void ToggleInteractButton(bool toggle) => interactButtonTransform.gameObject.SetActive(toggle);

    public void SetInteractButtonSprite(ControlScheme controlScheme)
    {
        if (controlScheme == ControlScheme.Gamepad)
        {
            interactButtonImage.sprite = Gamepad.current switch
            {
                DualShockGamepad _ => psInteractSprite,
                XInputController _ => xboxInteractSprite,
                SwitchProControllerHID _ => switchInteractSprite,
                _ => interactButtonImage.sprite
            };
        }
        else
            interactButtonImage.sprite = keyboardInteractSprite;
    }

    public void ToggleBossHealthBar(bool toggle)
    {
        bossHealthBar.gameObject.SetActive(toggle);
        Debug.Log($"Set boss health bar to {bossHealthBar.gameObject.activeSelf}");
    }

    public void SetBossMaxHealth(short max)
    {
        bossHealthBar.maxValue = max;
        Debug.Log($"Set boss health bar max to {bossHealthBar.maxValue}");
    }
    
    public void UpdateBossHealth(short health)
    {
        bossHealthBar.value = health >= 0 ? health : 0;
        Debug.Log($"Updated boss health bar to {bossHealthBar.value}");
    }
}