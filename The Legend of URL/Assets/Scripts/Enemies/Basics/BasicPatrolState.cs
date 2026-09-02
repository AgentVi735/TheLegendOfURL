using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

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
    private float forceDetectDistance;
    private int pathPos;
    private EnemyWaypoint[] path;
    private EnemyWaypoint currentWaypoint;
    private EnemyWaypoint lastWaypoint;
    private NavMeshPath navMeshPath;
    private int pathIdx;
    private Transform obj;
    private float lastDistance;
    
    public void UpdateState(EnemyController controller)
    {
        if (CanSeePlayer())
        {
            controller.ChangeState(controller.moveState);
            return;
        }

        if (pathIdx == -1)
        {
            navMeshPath = new NavMeshPath();
            agent.CalculatePath(currentWaypoint.transform.position, navMeshPath);
            switch (navMeshPath.corners.Length)
            {
                case 1:
                    pathIdx = 0;
                    break;
                case > 1:
                    pathIdx = 1;
                    break;
                default:
                    return;
            }
        }
        
        Vector3 destination = navMeshPath.corners[pathIdx];
        Vector3 posToMoveTo = destination - enemyTransform.position;
        posToMoveTo.y = enemyTransform.position.y;
        if (obj != null)
            obj.position = posToMoveTo + enemyTransform.position;
        float deltaAngle = Vector3.Angle(enemyTransform.forward, posToMoveTo);
        Vector3 rotationAxis = Vector3.Cross(enemyTransform.forward, posToMoveTo);
        rotationAxis.x = 0;
        rotationAxis.z = 0;
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

        Vector3 diffPos = enemyTransform.position;
        diffPos.y = destination.y;
        float distance = Vector3.Distance(destination, diffPos);
        
        lastDistance = distance;
        
        character.Move(enemyTransform.forward * (speed * Time.deltaTime));
        character.Move(-enemyTransform.up * velocity.y);
        
        diffPos = enemyTransform.position;
        diffPos.y = destination.y;
        distance = Vector3.Distance(destination, diffPos);
        
        if (Math.Abs(distance - lastDistance) < 0.001f)
        {
            pathIdx = -1;
            return;
        }

        if (Physics.Raycast(eyesTransform.position, enemyTransform.forward, out RaycastHit hit, 1))
        {
            if (hit.transform.CompareTag("Enemy"))
            {
                if (obj != null)
                    Debug.Log($"Hit enemy {hit.transform.name}");
                pathIdx = -1;
                return;
            }
        }
        
        if (!(distance < 0.5f)) return;
        pathIdx++;
        if (navMeshPath.corners.Length > pathIdx) return;
        EnemyWaypoint oldWaypoint = currentWaypoint;
        currentWaypoint = GetNewWaypoint();
        lastWaypoint = oldWaypoint;
        pathIdx = -1;
    }

    private bool CanSeePlayer()
    {
        float distance = Vector3.Distance(playerTransform.position, enemyTransform.position);
        if (distance <= forceDetectDistance) return true;
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
        forceDetectDistance = controller.data.forceDetectDistance;
        agent = controller.navMeshAgent;
        agent.isStopped = true;
        agent.autoBraking = false;
        path = controller.patrolPath;
        pathIdx = -1;
        obj = controller.obj;

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

    public void OnDrawGizmosSelected(EnemyController controller)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(currentWaypoint.transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        Gizmos.color = Color.purple;
        switch (navMeshPath.corners.Length)
        {
            case > 1:
                Gizmos.DrawLineStrip(navMeshPath.corners, false);
                break;
            case 1:
                Gizmos.DrawLine(enemyTransform.position, navMeshPath.corners[0]);
                break;
        }
    }
}