using System;
using UnityEngine;
using UnityEngine.AI;

public class SpiderQueenMoveToCeilingState : IEnemyState
{
    private SpiderQueenController _controller;

    private Transform movementTransform;
    private Transform playerTransform;
    private Transform enemyTransform;
    private Transform eyesTransform;
    private CharacterController character;
    private float speed;
    private float turnSpeed;
    private float movementTurnSpeed;
    private Vector3 posToMoveToLocal;
    private Vector3 posToMoveToGlobal;

    private SpiderQueenWallPoint currentWallPoint;
    private SpiderQueenWallPoint[] wallPoints;
    
    private NavMeshPath path;
    private int pathPos;
    private NavMeshPath navMeshPath;
    private int pathIdx;
    private float lastDistance;
    
    public void Initialise(EnemyController controller)
    {
        _controller = (SpiderQueenController) controller;
        wallPoints = _controller.wallPoints;
        character = _controller.characterController;
        movementTransform = _controller.MovementTrans;
        enemyTransform = character.transform;
        eyesTransform = _controller.eyesTransform;
        speed = _controller.data.chaseSpeed;
        turnSpeed = _controller.data.turnSpeed;
        movementTurnSpeed = _controller.data.movementTurnSpeed;
        playerTransform = _controller.player.transform;
    }

    public void UpdateState()
    {
        // float distance = Vector3.Distance(enemyTransform.position, playerTransform.position);
        // if (distance < attackRange)
        // {
        //     _controller.ChangeState(_controller.attackChargeState);
        //     return;
        // }
        //
        bool isOnGround = Physics.Raycast(enemyTransform.position, -enemyTransform.up, out RaycastHit _,
            0.2f);
        //
        // path = new NavMeshPath();
        // Vector3 pos = enemyTransform.position;
        // Vector3 charPos = _controller.GetNavMeshPosition(pos);
        // pos = charPos;
        // Vector3 destinationPos = playerTransform.position;
        // NavMesh.CalculatePath(pos, destinationPos, _controller.navMeshFilter, path);
        // if (path.status != NavMeshPathStatus.PathComplete)
        // {
        //     NavMeshHit navMeshHit = _controller.GetNavMeshHit(destinationPos);
        //     if (navMeshHit.hit)
        //     {
        //         destinationPos = navMeshHit.position;
        //         NavMesh.CalculatePath(pos, destinationPos, _controller.navMeshFilter, path);
        //         if (path.status != NavMeshPathStatus.PathComplete)
        //             return;
        //     }
        // }
        //
        // if (path.corners.Length < 2)
        //     return;
        // this.posToMoveToGlobal = path.corners[1];
        // posToMoveToLocal = this.posToMoveToGlobal - enemyTransform.position;
        //
        // Vector3 posToLookAt = posToMoveToLocal;
        // posToLookAt.y = 0;
        // Quaternion deltaRotation = Quaternion.FromToRotation(movementTransform.forward, posToLookAt);
        // movementTransform.rotation = Quaternion.Lerp(movementTransform.rotation, movementTransform.rotation * deltaRotation,
        //     movementTurnSpeed * Time.deltaTime);
        //
        // Vector3 velocity = Vector3.zero;
        // if (isOnGround)
        // {
        //     if (velocity.y < -2f)
        //         velocity.y = -2f;
        // }
        //
        // velocity.y += _controller.data.gravitySpeed * Time.deltaTime;
        //
        // Vector3 movePos = movementTransform.forward * (speed * Time.deltaTime);
        // movePos += -movementTransform.up * velocity.y;
        //
        // character.Move(movePos);
        //
        // Vector3 playerPos = playerTransform.position - enemyTransform.position;
        // playerPos.y = 0;
        // deltaRotation = Quaternion.FromToRotation(enemyTransform.forward, playerPos);
        // enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
        //     turnSpeed * Time.deltaTime);
        //
        // _controller.WalkAnimation();
        
        if (pathIdx == -1)
        {
            navMeshPath = new NavMeshPath();
            Vector3 pos = enemyTransform.position;
            Vector3 charPos = _controller.GetNavMeshPosition(pos);
            pos = charPos;
            Vector3 destinationPos = currentWallPoint.groundPoint.position;
            NavMesh.CalculatePath(pos, destinationPos, _controller.navMeshFilter, navMeshPath);
            switch (navMeshPath.corners.Length)
            {
                case 1:
                    pathIdx = 0;
                    break;
                case > 1:
                    pathIdx = 1;
                    break;
                case 0:
                    pathIdx = -1;
                    return;
                default:
                    return;
            }
        }
        
        posToMoveToGlobal = navMeshPath.corners[pathIdx];
        posToMoveToLocal = posToMoveToGlobal - enemyTransform.position;
        
        Vector3 posToLookAt = posToMoveToLocal;
        posToLookAt.y = 0;
        Quaternion deltaRotation = Quaternion.FromToRotation(enemyTransform.forward, posToLookAt);
        enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
            movementTurnSpeed * Time.deltaTime);

        Vector3 velocity = Vector3.zero;
        if (isOnGround)
        {
            if (velocity.y < -2f)
                velocity.y = -2f;
        }
        
        velocity.y += _controller.data.gravitySpeed * Time.deltaTime;
        Vector3 movePos = -enemyTransform.up * velocity.y;

        Vector3 diffPos = enemyTransform.position;
        diffPos.y = posToMoveToGlobal.y;
        float distance = Vector3.Distance(posToMoveToGlobal, diffPos);
        
        lastDistance = distance;

        movePos = movementTransform.forward * (speed * Time.deltaTime);
        movePos += -movementTransform.up * velocity.y;
        
        character.Move(movePos);
        
        // Vector3 playerPos = playerTransform.position - enemyTransform.position;
        // playerPos.y = 0;
        // deltaRotation = Quaternion.FromToRotation(enemyTransform.forward, playerPos);
        // enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
        //     turnSpeed * Time.deltaTime);
        
        _controller.WalkAnimation();
        
        diffPos = enemyTransform.position;
        diffPos.y = posToMoveToGlobal.y;
        distance = Vector3.Distance(posToMoveToGlobal, diffPos);
        
        if (Math.Abs(distance - lastDistance) < 0.001f)
        {
            pathIdx = -1;
            return;
        }

        if (Physics.Raycast(eyesTransform.position, enemyTransform.forward, out RaycastHit hit, 1))
        {
            if (hit.transform.CompareTag("Enemy"))
            {
                pathIdx = -1;
                return;
            }
        }
        
        if (!(distance < 0.5f)) return;
        pathIdx++;
    }

    public void OnEnter()
    {
        pathIdx = -1;
        float distance = Vector3.Distance(enemyTransform.position, wallPoints[0].groundPoint.position);
        int idx = 0;

        for (int i = 0; i < wallPoints.Length; i++)
        {
            SpiderQueenWallPoint point = wallPoints[i];
            float currentDistance = Vector3.Distance(enemyTransform.position, point.groundPoint.position);
            if (currentDistance >= distance) continue;
            idx = i;
            distance = currentDistance;
        }

        currentWallPoint = wallPoints[idx];
        
        _controller.AudioPlayer.TryPlayPersistentClip(0, true);
    }

    public void OnExit()
    {
        _controller.StopWalkAnimation();
    }

    public void OnHurt()
    {
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orangeRed;
        if (navMeshPath?.corners?.Length > 0)
        {
            foreach (var corner in navMeshPath.corners)
                Gizmos.DrawCube(corner, new Vector3(0.4f, 0.4f, 0.4f));
        }
        Gizmos.color = Color.green;
        Gizmos.DrawCube(currentWallPoint.groundPoint.position, new Vector3(0.3f, 0.3f, 0.3f));
        if (navMeshPath is { corners: not null })
        {
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
        
        Gizmos.DrawLine(eyesTransform.position, playerTransform.position);
    }
}