using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BasicPatrolState : IEnemyState
{
    private Transform playerTransform;
    private Transform enemyTransform;
    private Transform eyesTransform;
    private CharacterController character;
    private NavMeshAgent agent;
    private float speed;
    private float turnSpeed;
    private float detectDistance;
    private int pathPos;
    private EnemyWaypoint[] path;
    private EnemyWaypoint currentWaypoint;
    private EnemyWaypoint lastWaypoint;
    private LineRenderer line;
    
    public void UpdateState(EnemyController controller)
    {
        if (CanSeeEnemy())
        {
            controller.ChangeState(controller.moveState);
            return;
        }
        
        NavMeshPath navMeshPath = new();
        agent.CalculatePath(currentWaypoint.transform.position, navMeshPath);
        Vector3 destination;
        switch (navMeshPath.corners.Length)
        {
            case 1:
                destination = navMeshPath.corners[0];
                break;
            case > 1:
                destination = navMeshPath.corners[1];
                break;
            default:
                return;
        }
        Vector3 posToMoveTo = destination - enemyTransform.position;
        posToMoveTo.y = enemyTransform.position.y;
        float deltaAngle = Vector3.Angle(enemyTransform.forward, posToMoveTo);
        Vector3 rotationAxis = Vector3.Cross(enemyTransform.forward, posToMoveTo);
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaAngle, rotationAxis);
        enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
            turnSpeed * Time.deltaTime);
        
        character.Move(enemyTransform.forward * (speed * Time.deltaTime));

        Vector3 diffPos = enemyTransform.position;
        diffPos.y = 0;
        if (!(Vector3.Distance(destination, diffPos) < 0.5f)) return;
        EnemyWaypoint oldWaypoint = currentWaypoint;
        currentWaypoint = GetNewWaypoint();
        lastWaypoint = oldWaypoint;
    }

    private bool CanSeeEnemy()
    {
        float distance = Vector3.Distance(playerTransform.position, enemyTransform.position);
        if (!(distance < detectDistance)) return false;
        Vector3 toTarget = (playerTransform.position - enemyTransform.position).normalized;
        float dot = Vector3.Dot(enemyTransform.forward, toTarget);

        if (!(dot > 0.7071)) return false;
        return Physics.Raycast(eyesTransform.position, playerTransform.position - enemyTransform.position, out RaycastHit hit,
            detectDistance) && hit.transform != null && hit.transform.CompareTag("Player");
    }

    private EnemyWaypoint GetNewWaypoint()
    {
        switch (currentWaypoint.availableWaypoints.Length)
        {
            case 1:
                return currentWaypoint.availableWaypoints[0];
            case > 1:
                List<EnemyWaypoint> waypoints = new();
                if (lastWaypoint != null && currentWaypoint.availableWaypoints.Contains(lastWaypoint))
                {
                    waypoints.AddRange(currentWaypoint.availableWaypoints
                        .Where(waypoint => waypoint != lastWaypoint));
                }
                else
                    waypoints = currentWaypoint.availableWaypoints.ToList();
                int idx = Random.Range(0, waypoints.Count);
                return waypoints[idx];
            default:
                return path[0];
        }
    }

    public void OnEnter(EnemyController controller)
    {
        playerTransform = controller.player.transform;
        character = controller.characterController;
        enemyTransform = character.transform;
        eyesTransform = controller.eyesTransform;
        speed = controller.data.walkSpeed;
        turnSpeed = controller.data.turnSpeed;
        detectDistance = controller.data.detectDistance;
        agent = controller.navMeshAgent;
        agent.isStopped = true;
        agent.autoBraking = false;
        path = controller.patrolPath;
        line = enemyTransform.GetComponent<LineRenderer>();
        // line.useWorldSpace = true;

        float closestPosDiff = 0;
        int closestPosIdx = -1;
        for (int i = 0; i < path.Length; i++)
        {
            Vector3 pos = path[i].transform.position;
            float difference = Vector3.Distance(pos, enemyTransform.position);
            if (closestPosIdx != -1 && !(difference < closestPosDiff)) continue;
            closestPosIdx = i;
            closestPosDiff = difference;
        }

        if (closestPosIdx != -1)
            currentWaypoint = path[closestPosIdx];
    }

    public void OnExit(EnemyController controller)
    {
        agent.autoBraking = true;
    }

    public void OnHurt(EnemyController controller)
    {
        
    }
}