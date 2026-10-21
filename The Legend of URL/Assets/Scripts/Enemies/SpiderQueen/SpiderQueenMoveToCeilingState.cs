using System;
using UnityEngine;
using UnityEngine.AI;

public class SpiderQueenMoveToCeilingState : IEnemyState
{
    private static readonly int landAnim = Animator.StringToHash("land");
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

    private int state;

    private float webLength;
    private const float webUpTime = 0.6f;
    private float currentWebUpTime;
    
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
        switch (state)
        {
            case 0:
                State0Loop();
                break;
            case 1:
                State1Loop();
                break;
            case 2:
                State2Loop();
                break;
            case 3:
                State3Loop();
                break;
            case 4:
                State4Loop();
                break;
        }
    }

    private void State0Loop()
    {
        float distance = Vector3.Distance(enemyTransform.position, currentWallPoint.groundPoint.position);
        if (distance < 2)
        {
            SwitchToState1();
            return;
        }
        
        bool isOnGround = Physics.Raycast(enemyTransform.position, -enemyTransform.up, out RaycastHit _,
            0.2f);
        
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
        Quaternion deltaRotation = Quaternion.FromToRotation(movementTransform.forward, posToLookAt);
        movementTransform.rotation = Quaternion.Lerp(movementTransform.rotation, movementTransform.rotation * deltaRotation,
            movementTurnSpeed * Time.deltaTime);

        Vector3 velocity = Vector3.zero;
        if (isOnGround)
        {
            if (velocity.y < -2f)
                velocity.y = -2f;
        }
        
        velocity.y += _controller.data.gravitySpeed * Time.deltaTime;

        Vector3 diffPos = enemyTransform.position;
        diffPos.y = posToMoveToGlobal.y;
        distance = Vector3.Distance(posToMoveToGlobal, diffPos);
        
        lastDistance = distance;

        Vector3 movePos = movementTransform.forward * (speed * Time.deltaTime);
        movePos += -movementTransform.up * velocity.y;
        
        character.Move(movePos);
        
        Vector3 playerPos = playerTransform.position - enemyTransform.position;
        playerPos.y = 0;
        deltaRotation = Quaternion.FromToRotation(enemyTransform.forward, playerPos);
        enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
            turnSpeed * Time.deltaTime);
        
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

    private void SwitchToState1()
    {
        state = 1;
        webLength = 0;
        _controller.StopWalkAnimation();
        _controller.AudioPlayer.StopPersistentAudio();
        
        Vector3 pos = _controller.LineRenderer.GetPosition(0);
        _controller.LineRenderer.SetPosition(1, pos);
        _controller.LineRenderer.enabled = true;
    }

    private void State1Loop()
    {
        Vector3 playerPos = playerTransform.position - enemyTransform.position;
        playerPos.y = 0;
        Quaternion deltaRotation = Quaternion.FromToRotation(enemyTransform.forward, playerPos);
        enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
            turnSpeed * Time.deltaTime);
        
        webLength += Time.deltaTime * 20;
        Vector3 pos = _controller.LineRenderer.GetPosition(0);
        pos.y += webLength;
        _controller.LineRenderer.SetPosition(1, pos);

        if (webLength > 7.5)
            SwitchToState2();
    }

    private void SwitchToState2()
    {
        state = 2;
    }

    private void State2Loop()
    {
        currentWebUpTime += Time.deltaTime;
        
        Vector3 newPos = enemyTransform.position;

        float ratio = currentWebUpTime / webUpTime;
        newPos.y = Mathf.Lerp(currentWallPoint.groundPoint.position.y, currentWallPoint.roofPoint.position.y,ratio);

        enemyTransform.position = newPos;
        
        Vector3 playerPos = playerTransform.position - enemyTransform.position;
        playerPos.y = 0;
        Quaternion deltaRotation = Quaternion.FromToRotation(enemyTransform.forward, playerPos);
        Vector3 deltaRotEuler = deltaRotation.eulerAngles;
        deltaRotEuler.z = 0;
        deltaRotEuler.x = 0;
        deltaRotation = Quaternion.Euler(deltaRotEuler);
        enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation,
            enemyTransform.rotation * deltaRotation,
            turnSpeed * Time.deltaTime);
        
        Vector3 newRot = _controller.ModelTransform.rotation.eulerAngles;
        newRot.z = Mathf.Lerp(0, 180, ratio * 4);
        _controller.ModelTransform.rotation = Quaternion.Euler(newRot);
        
        if (currentWebUpTime >= webUpTime)
            SwitchToState3();
    }

    private void SwitchToState3()
    {
        state = 3;
        _controller.LineRenderer.enabled = false;
        _controller.AudioPlayer.TryPlayPersistentClip(0, true);
        Vector3 newPos = enemyTransform.position;
        newPos.y = currentWallPoint.roofPoint.position.y;
        enemyTransform.position = newPos;
    }

    private void State3Loop()
    {
        Vector3 pos = enemyTransform.position;
        pos.y = 0;
        
        Vector3 destinationPos = playerTransform.position;
        destinationPos.y = 0;
        
        float distance = Vector3.Distance(pos, destinationPos);
        if (distance < 2)
        {
            SwitchToState4();
            return;
        }

        path = new NavMeshPath();
        Vector3 charPos = _controller.GetNavMeshPosition(pos);
        pos = charPos;
        NavMesh.CalculatePath(pos, destinationPos, _controller.navMeshFilter, path);
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            NavMeshHit navMeshHit = _controller.GetNavMeshHit(destinationPos);
            if (navMeshHit.hit)
            {
                destinationPos = navMeshHit.position;
                NavMesh.CalculatePath(pos, destinationPos, _controller.navMeshFilter, path);
                if (path.status != NavMeshPathStatus.PathComplete)
                {
                    Debug.LogError("Unable to calculate path");
                    return;
                }
            }
        }

        if (path.corners.Length < 2)
            return;
        posToMoveToGlobal = path.corners[1];
        posToMoveToLocal = posToMoveToGlobal - enemyTransform.position;
        
        Vector3 posToLookAt = posToMoveToLocal;
        posToLookAt.y = 0;
        Quaternion deltaRotation = Quaternion.FromToRotation(movementTransform.forward, posToLookAt);
        movementTransform.rotation = Quaternion.Lerp(movementTransform.rotation, movementTransform.rotation * deltaRotation,
            movementTurnSpeed * Time.deltaTime);
        
        Vector3 movePos = movementTransform.forward * (speed * Time.deltaTime);

        character.Move(movePos);
        
        Vector3 newPos = enemyTransform.position;
        newPos.y = currentWallPoint.roofPoint.position.y;
        enemyTransform.position = newPos;
        
        Vector3 playerPos = playerTransform.position - enemyTransform.position;
        playerPos.y = 0;
        deltaRotation = Quaternion.FromToRotation(enemyTransform.forward, playerPos);
        enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
            turnSpeed * Time.deltaTime);
        
        _controller.WalkAnimation();
    }

    private void SwitchToState4()
    {
        _controller.AudioPlayer.StopPersistentAudio();
        _controller.StopWalkAnimation();
        state = 4;
        Vector3 newRot = _controller.ModelTransform.rotation.eulerAngles;
        newRot.z = 0;
        _controller.ModelTransform.rotation = Quaternion.Euler(newRot);
        enemyTransform.position += new Vector3(0, -2, 0);
    }
    
    private void State4Loop()
    {
        bool isOnGround = Physics.Raycast(enemyTransform.position, -enemyTransform.up, out RaycastHit _,
            0.2f);

        if (isOnGround)
        {
            State4Finish();
            return;
        }
        
        Vector3 velocity = Vector3.zero;
        velocity.y += _controller.data.gravitySpeed * Time.deltaTime;

        Vector3 movePos = movementTransform.forward * (speed * Time.deltaTime);
        movePos += -movementTransform.up * velocity.y;
        
        character.Move(movePos);
    }

    private void State4Finish()
    {
        state = -1;
        _controller.Animator.SetTrigger(landAnim);
        _controller.AudioPlayer.PlayClip(3);
        Vector3 playerPos = playerTransform.position;
        playerPos.y = 0;
        Vector3 enemyPos = enemyTransform.position;
        enemyPos.y = 0;
        float distance = Vector3.Distance(playerPos, enemyPos);
        if (distance < 1.8)
        {
            _controller.player.OnHit(_controller.data.shockwaveDamage);
        }
    }
    
    public void OnEnter()
    {
        state = 0;
        pathIdx = -1;
        currentWebUpTime = 0;
        
        Vector3 pos = _controller.LineRenderer.GetPosition(0);
        _controller.LineRenderer.SetPosition(1, pos);
        _controller.LineRenderer.enabled = false;
        
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
        switch (state)
        {
            case 0:
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
                break;
            }
            case 3:
                Vector3 cubeSize = new(0.5f, 0.5f, 0.5f);
                Gizmos.color = Color.orangeRed;
                if (path?.corners?.Length > 0)
                {
                    foreach (var corner in path.corners)
                        Gizmos.DrawCube(corner, new Vector3(0.6f, 0.6f, 0.6f));
                }

                Color color = Color.green;
                color.a = 0.5f;
                Gizmos.color = color;
                Gizmos.DrawCube(posToMoveToGlobal, cubeSize);
        
                Gizmos.color = Color.hotPink;
                Gizmos.DrawCube(playerTransform.position, cubeSize);
                Gizmos.color = Color.purple;
                switch (path?.corners?.Length)
                {
                    case > 1:
                        Gizmos.DrawLineStrip(path.corners, false);
                        break;
                    case 1:
                        Gizmos.DrawLine(enemyTransform.position, path.corners[0]);
                        break;
                }

                Gizmos.DrawLine(eyesTransform.position, playerTransform.position);
                break;
        }
    }
}