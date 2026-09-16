using UnityEngine;

public class KnockbackState : IEnemyState
{
    private EnemyController _controller;
    
    private Transform enemyTransform;
    private CharacterController character;
    private Vector3 posToMoveToLocal;
    private Vector3 posToMoveTo;
    
    public void Initialise(EnemyController controller)
    {
        _controller = controller;
        character = _controller.characterController;
        enemyTransform = character.transform;
    }

    public void UpdateState()
    {
        if (_controller.hitVelocity.x is < 0.01f and > -0.01f &&
            _controller.hitVelocity.y is < 0.01f and > -0.01f &&
            _controller.hitVelocity.z is < 0.01f and > -0.01f)
        {
            _controller.Stun(_controller.data.knockbackStunTime);
            return;
        }
        
        Vector3 velocity = Vector3.zero;
        if (_controller.characterController.isGrounded)
        {
            if (velocity.y < -2f)
                velocity.y = -2f;
        }
        
        velocity.y += _controller.data.gravitySpeed * Time.deltaTime;

        Vector3 movePos = Vector3.zero;
        movePos += -enemyTransform.up * velocity.y;
        Vector3 extraVelocity = _controller.hitVelocity * (_controller.data.knockbackSpeed * Time.deltaTime);
        movePos += extraVelocity;
        _controller.hitVelocity -= extraVelocity;
        
        character.Move(movePos);
    }

    public void OnEnter()
    {
        _controller.meshRenderer.material.color = Color.blueViolet;
    }

    public void OnExit()
    {
    }

    public void OnHurt()
    {
    }

    public void OnDrawGizmosSelected()
    {
    }
}