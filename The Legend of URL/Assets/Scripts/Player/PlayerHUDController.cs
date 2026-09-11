using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerController controller;
    
    [Header("UI References")]
    [SerializeField] private Slider healthBar;

    public void Initialise(int maxHealth)
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
        healthBar.gameObject.SetActive(true);
    }
    
    public void UpdateHealthBar(int health)
    {
        healthBar.value = health >= 0 ? health : 0;
    }
}