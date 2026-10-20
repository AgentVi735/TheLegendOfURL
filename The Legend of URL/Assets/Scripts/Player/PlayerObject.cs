using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerAttackManager _attackManager;

    public void FinishPreJump() => _movement.FinishPreJump();
    
    public void FinishLanding() => _movement.FinishLanding();

    public void FinishAttack() => _attackManager.AttackFinish();
}