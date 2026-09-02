using System;
using UnityEngine;
using UnityEngine.AI;

public class SpiderMoveState : IEnemyState
{
    private Transform playerTransform;
    private Transform enemyTransform;
    private Transform eyesTransform;
    private CharacterController character;
    private NavMeshAgent agent;
    private float speed;
    private float turnSpeed;
    private float followRange;
    private Vector3 posToMoveToLocal;
    private Vector3 posToMoveTo;
    private Vector3 lastSeenPos;
    private Transform obj;
    
    public void UpdateState(EnemyController controller)
    {
        bool canSeePlayer = CanSeePlayer();
        switch (canSeePlayer)
        {
            case false:
            {
                float distance = Vector3.Distance(enemyTransform.position, lastSeenPos);
                if (distance < 0.1)
                {
                    controller.ChangeState(controller.lookState);
                    return;
                }
                break;
            }
            case true:
            {
                lastSeenPos = playerTransform.position;
                break;
            }
        }
        
        NavMeshPath path = new();
        agent.CalculatePath(lastSeenPos, path);
        if (path.corners.Length < 2)
            return;
        posToMoveTo = path.corners[1];
        posToMoveToLocal = posToMoveTo - enemyTransform.position;
        posToMoveTo.y = enemyTransform.position.y;
        posToMoveToLocal.y = enemyTransform.position.y;
        
        if (canSeePlayer)
            lastSeenPos = posToMoveTo;
        if (obj != null)
            obj.position = lastSeenPos;
        
        float deltaAngle = Vector3.Angle(enemyTransform.forward, posToMoveToLocal);
        Vector3 rotationAxis = Vector3.Cross(enemyTransform.forward, posToMoveToLocal);
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaAngle, rotationAxis);
        enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
            turnSpeed * Time.deltaTime);
        
        Vector3 velocity = Vector3.zero;
        if (controller.characterController.isGrounded)
        {
            if (velocity.y < -2f)
                velocity.y = -2f;
        }
        
        velocity.y += controller.data.gravitySpeed * Time.deltaTime;

        character.Move(enemyTransform.forward * (speed * Time.deltaTime));
        character.Move(-enemyTransform.up * velocity.y);
    }

    private bool CanSeePlayer()
    {
        float distance = Vector3.Distance(playerTransform.position, enemyTransform.position);
        if (!(distance < followRange)) return false;
        Vector3 toTarget = (playerTransform.position - enemyTransform.position).normalized;
        float dot = Vector3.Dot(enemyTransform.forward, toTarget);

        if (!(dot > 0.7071)) return false;
        return Physics.Raycast(eyesTransform.position, playerTransform.position - enemyTransform.position, out RaycastHit hit,
            followRange) && hit.transform != null && hit.transform.CompareTag("Player");
    }

    public void OnEnter(EnemyController controller)
    {
        character = controller.characterController;
        enemyTransform = character.transform;
        eyesTransform = controller.eyesTransform;
        speed = controller.data.walkSpeed;
        turnSpeed = controller.data.turnSpeed;
        followRange = controller.data.followRange;
        playerTransform = controller.player.transform;
        agent = controller.navMeshAgent;
        agent.isStopped = true;
        obj = controller.obj;
        lastSeenPos = playerTransform.position;
    }

    public void OnExit(EnemyController controller)
    {
        
    }

    public void OnHurt(EnemyController controller)
    {
        
    }

    public void OnDrawGizmosSelected(EnemyController controller)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(posToMoveTo, new Vector3(0.5f, 0.5f, 0.5f));
        Gizmos.color = Color.purple;
        switch (agent.path.corners.Length)
        {
            case > 1:
                Gizmos.DrawLineStrip(agent.path.corners, false);
                break;
            case 1:
                Gizmos.DrawLine(enemyTransform.position, agent.path.corners[0]);
                break;
        }
    }
}