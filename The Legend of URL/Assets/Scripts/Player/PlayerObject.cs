using UnityEngine;
// ReSharper disable UnusedMember.Global
// Once again ReSharper doesn't detect these methods being used.
// They're using in animation events in the Player model.

public class PlayerObject : MonoBehaviour
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerAttackManager _attackManager;

    public void FinishPreJump() => _movement.FinishPreJump();
    
    public void FinishLanding() => _movement.FinishLanding();

    public void FinishAttack() => _attackManager.AttackFinish();
}