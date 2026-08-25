using UnityEngine;
using UnityEngine.AI;

public class SpiderMoveState : IEnemyState
{
    private Transform playerTransform;
    private Transform enemyTransform;
    private CharacterController character;
    private NavMeshAgent navMeshAgent;
    private float speed;
    private float turnSpeed;
    
    public void UpdateState(EnemyController controller)
    {
        NavMeshPath path = new();
        navMeshAgent.CalculatePath(playerTransform.position, path);
        if (path.corners.Length < 2)
            return;
        Vector3 posToMoveTo = path.corners[1] - enemyTransform.position;
        posToMoveTo = new Vector3(posToMoveTo.x, 0, posToMoveTo.z);
        float deltaAngle = Vector3.Angle(enemyTransform.forward, posToMoveTo);
        Vector3 rotationAxis = Vector3.Cross(enemyTransform.forward, posToMoveTo);
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaAngle, rotationAxis);
        enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
            turnSpeed * Time.deltaTime);
        
        character.Move(enemyTransform.forward * (speed * Time.deltaTime));
    }

    public void OnEnter(EnemyController controller)
    {
        character = controller.GetCharacterController();
        enemyTransform = character.transform;
        speed = controller.GetData().walkSpeed;
        turnSpeed = controller.GetData().turnSpeed;
        playerTransform = controller.GetPlayer().transform;
        navMeshAgent = controller.GetNavMeshAgent();
        navMeshAgent.isStopped = true;
    }

    public void OnExit(EnemyController controller)
    {
        
    }

    public void OnHurt(EnemyController controller)
    {
        
    }
}