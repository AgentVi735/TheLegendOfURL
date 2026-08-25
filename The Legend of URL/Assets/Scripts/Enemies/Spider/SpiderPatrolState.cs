using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SpiderPatrolState : IEnemyState
{
    private Transform enemyTransform;
    private CharacterController character;
    private NavMeshAgent agent;
    private float speed;
    private float turnSpeed;
    private int pathPos;
    private EnemyWaypoint[] path;
    private EnemyWaypoint currentWaypoint;
    private EnemyWaypoint lastWaypoint;
    
    public void UpdateState(EnemyController controller)
    {
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
        posToMoveTo = new Vector3(posToMoveTo.x, 0, posToMoveTo.z);
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
        character = controller.characterController;
        enemyTransform = character.transform;
        speed = controller.walkSpeed;
        turnSpeed = controller.turnSpeed;
        agent = controller.navMeshAgent;
        agent.isStopped = true;
        agent.autoBraking = false;
        path = controller.patrolPath;

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