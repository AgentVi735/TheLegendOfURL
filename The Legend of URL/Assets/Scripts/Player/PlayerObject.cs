using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    [SerializeField] private PlayerMovement _movement;

    public void FinishPreJump() => _movement.FinishPreJump();
    
    public void FinishLanding() => _movement.FinishLanding();
}