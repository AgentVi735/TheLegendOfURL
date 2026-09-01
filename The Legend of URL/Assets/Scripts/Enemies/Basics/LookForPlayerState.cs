using UnityEngine;
using Random = UnityEngine.Random;

public class LookForPlayerState : IEnemyState
{
    private Transform playerTransform;
    private Transform enemyTransform;
    private Transform eyesTransform;
    private CharacterController character;
    private float turnSpeed;
    private float followRange;
    private Vector3 posToMoveTo;
    private Quaternion rotationToGoTo;
    private float timeSpent;
    private float maxLookTime;
    
    public void UpdateState(EnemyController controller)
    {
        timeSpent += Time.deltaTime;
        float deltaAngle = Vector3.Angle(enemyTransform.forward, posToMoveTo);
        Vector3 rotationAxis = Vector3.Cross(enemyTransform.forward, posToMoveTo);
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaAngle, rotationAxis);
        enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
            turnSpeed * Time.deltaTime);

        if (deltaAngle < 0.8)
            GetNewDirection();

        if (CanSeePlayer())
            controller.ChangeState(controller.moveState);
        else if (timeSpent >= maxLookTime)
            controller.ChangeState(controller.idleState);
    }

    private bool CanSeePlayer()
    {
        Vector3 toTarget = (playerTransform.position - enemyTransform.position).normalized;
        float dot = Vector3.Dot(enemyTransform.forward, toTarget);

        if (!(dot > 0.7071)) return false;
        return Physics.Raycast(eyesTransform.position, playerTransform.position - enemyTransform.position, out RaycastHit hit,
            followRange) && hit.transform != null && hit.transform.CompareTag("Player");
    }

    private void GetNewDirection()
    {
        float rot = Random.Range(-360f, 360f);
        rotationToGoTo = Quaternion.Euler(0, rot, 0);
        Vector3 direction = rotationToGoTo * enemyTransform.forward;
        posToMoveTo = direction;
    }

    private void GetPlayerDirection()
    {
        posToMoveTo = playerTransform.position - enemyTransform.position;
        float deltaAngle = Vector3.Angle(enemyTransform.forward, posToMoveTo);
        Vector3 rotationAxis = Vector3.Cross(enemyTransform.forward, posToMoveTo);
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaAngle, rotationAxis);
        rotationToGoTo = Quaternion.Euler(0, deltaRotation.eulerAngles.y + enemyTransform.rotation.y, 0);
    }

    public void OnEnter(EnemyController controller)
    {
        character = controller.characterController;
        enemyTransform = character.transform;
        eyesTransform = controller.eyesTransform;
        turnSpeed = controller.data.turnSpeed;
        followRange = controller.data.followRange;
        playerTransform = controller.player.transform;
        maxLookTime = controller.data.maxLookTime;
        timeSpent = 0;
        GetPlayerDirection();
    }

    public void OnExit(EnemyController controller)
    {
        
    }

    public void OnHurt(EnemyController controller)
    {
        
    }
}