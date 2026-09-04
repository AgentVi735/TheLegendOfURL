using UnityEngine;
using UnityEngine.AI;

public class KnockbackState : IEnemyState
{
    private Transform enemyTransform;
    private CharacterController character;
    private NavMeshAgent agent;
    private Vector3 posToMoveToLocal;
    private Vector3 posToMoveTo;
    
    public void UpdateState(EnemyController controller)
    {
        if (controller.hitVelocity.x is < 0.01f and > -0.01f &&
            controller.hitVelocity.y is < 0.01f and > -0.01f &&
            controller.hitVelocity.z is < 0.01f and > -0.01f)
        {
            controller.ChangeState(controller.stunState);
            return;
        }
        
        Vector3 velocity = Vector3.zero;
        if (controller.characterController.isGrounded)
        {
            if (velocity.y < -2f)
                velocity.y = -2f;
        }
        
        velocity.y += controller.data.gravitySpeed * Time.deltaTime;

        Vector3 movePos = Vector3.zero;
        movePos += -enemyTransform.up * velocity.y;
        Vector3 extraVelocity = controller.hitVelocity * (controller.data.knockbackSpeed * Time.deltaTime);
        movePos += extraVelocity;
        controller.hitVelocity -= extraVelocity;
        
        character.Move(movePos);
    }

    public void OnEnter(EnemyController controller)
    {
        character = controller.characterController;
        enemyTransform = character.transform;
        agent = controller.navMeshAgent;
        agent.isStopped = true;
    }

    public void OnExit(EnemyController controller)
    {
    }

    public void OnHurt(EnemyController controller)
    {
    }

    public void OnDrawGizmosSelected(EnemyController controller)
    {
    }
}