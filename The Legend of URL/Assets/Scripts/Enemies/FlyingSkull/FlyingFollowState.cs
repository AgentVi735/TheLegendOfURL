using UnityEngine;
using UnityEngine.AI;

public class FlyingFollowState : IEnemyState
{
    private FlyingSkullController _controller;
    
    private Transform playerTransform;
    private Transform enemyTransform;
    private Transform eyesTransform;
    private CharacterController character;
    private Rigidbody rb;
    private float speed;
    private float turnSpeed;
    private float followRange;
    private float forceDetectDistance;
    private Vector3 posToMoveToLocal;
    private Vector3 posToMoveTo;
    private Vector3 lastSeenPos;
    private Vector3 lastSeenPosOnMesh;
    private LayerMask layers;
    private NavMeshPath path;
    private bool canReachPlayer;
    
    public void Initialise(EnemyController controller)
    {
        _controller = (FlyingSkullController) controller;
        character = _controller.characterController;
        enemyTransform = character.transform;
        eyesTransform = _controller.eyesTransform;
        speed = _controller.data.walkSpeed;
        turnSpeed = _controller.data.turnSpeed;
        followRange = _controller.data.followRange;
        forceDetectDistance = _controller.data.forceDetectDistance;
        playerTransform = _controller.player.transform;
        lastSeenPos = playerTransform.position;
        layers = _controller.raycastLayers;
    }

    public void UpdateState()
    {
        bool canSeePlayer = CanSeePlayer();
        float distance;
        switch (canSeePlayer)
        {
            case false:
            {
                distance = Vector3.Distance(enemyTransform.position, canReachPlayer ? lastSeenPosOnMesh : posToMoveTo);
                if (distance < 0.5)
                {
                    _controller.ChangeState(_controller.lookState);
                    return;
                }
                break;
            }
            case true:
            {
                lastSeenPos = playerTransform.position;
                NavMesh.SamplePosition(lastSeenPos, out NavMeshHit lastSeenPosNavMesh, 6, _controller.navMeshFilter);
                float neededDistance = Mathf.Abs(lastSeenPos.y + _controller.player._characterController.height / 2 - lastSeenPosNavMesh.position.y);
                distance = Vector3.Distance(enemyTransform.position, lastSeenPosNavMesh.position);
                if (distance < neededDistance)
                {
                    _controller.ChangeState(_controller.attackState);
                    return;
                }
                break;
            }
        }
        
        if (canSeePlayer)
            lastSeenPos = playerTransform.position;

        path = new NavMeshPath();
        Vector3 pos = enemyTransform.position;
        NavMesh.SamplePosition(pos, out NavMeshHit charPos, 6, _controller.navMeshFilter);
        pos = charPos.position;
        NavMesh.SamplePosition(lastSeenPos, out NavMeshHit navMeshHit, 6, _controller.navMeshFilter);
        lastSeenPosOnMesh = navMeshHit.position;
        NavMesh.CalculatePath(pos, lastSeenPosOnMesh, _controller.navMeshFilter, path);
        canReachPlayer = true;
        
        if (path.corners.Length < 2)
            return;
        posToMoveTo = path.corners[1];
        posToMoveToLocal = posToMoveTo - enemyTransform.position;
        
        float deltaAngle = Vector3.Angle(enemyTransform.forward, posToMoveToLocal);
        Vector3 rotationAxis = Vector3.Cross(enemyTransform.forward, posToMoveToLocal);
        rotationAxis.x = 0;
        rotationAxis.z = 0;
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaAngle, rotationAxis);
        enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
            turnSpeed * Time.deltaTime);

        Vector3 diffPos = enemyTransform.position;
        diffPos.y = lastSeenPosOnMesh.y;
        distance = Vector3.Distance(diffPos, lastSeenPosOnMesh);
        
        Vector3 movePos = Vector3.zero;
        if (distance >= 0.1f)
            movePos += enemyTransform.forward * (speed * Time.deltaTime);
        
        Vector3 directionToCheck = Vector3.zero;
        if (lastSeenPosOnMesh.y > enemyTransform.position.y)
            directionToCheck = enemyTransform.up;
        else if (lastSeenPosOnMesh.y < enemyTransform.position.y)
            directionToCheck = -enemyTransform.up;
        
        if (directionToCheck.y != 0)
        {
            if (!Physics.Raycast(_controller.leftTransform.position, directionToCheck, out RaycastHit _, character.height, layers))
            {
                if (!Physics.Raycast(_controller.rightTransform.position, directionToCheck, out RaycastHit _, character.height, layers))
                {
                    if (!Physics.Raycast(_controller.eyesTransform.position, directionToCheck, out RaycastHit _, character.height, layers))
                    {
                        Vector3 flyMovement = directionToCheck * (_controller.data.gravitySpeed * Time.deltaTime);

                        float amountToFly = Mathf.Abs(lastSeenPosOnMesh.y - enemyTransform.position.y);
                        float amountFlying = Mathf.Abs(flyMovement.y);

                        if (amountFlying > amountToFly)
                        {
                            amountFlying = amountToFly;
                            flyMovement.y = flyMovement.y switch
                            {
                                < 0 => -amountFlying,
                                > 0 => amountFlying,
                                _ => flyMovement.y
                            };
                        }
                        
                        movePos += flyMovement;
                    }
                }
            }
        }
        
        character.Move(movePos);
    }

    private bool CanSeePlayer()
    {
        float distance = Vector3.Distance(playerTransform.position, enemyTransform.position);
        Vector3 playerPos;
        RaycastHit hit;
        if (distance <= forceDetectDistance)
        {
            playerPos = playerTransform.position;
            return Physics.Raycast(eyesTransform.position, playerPos - enemyTransform.position, out hit,
                followRange, layers) && hit.transform != null && hit.transform.CompareTag("Player");
        }
        if (!(distance < followRange)) return false;
        playerPos = playerTransform.position;
        playerPos.y = enemyTransform.position.y;
        Vector3 toTarget = (playerPos - enemyTransform.position).normalized;
        float dot = Vector3.Dot(enemyTransform.forward, toTarget);

        if (!(dot > 0.7071)) return false;
        playerPos.y = playerTransform.position.y;
        return Physics.Raycast(eyesTransform.position, playerPos - enemyTransform.position, out hit,
            followRange, layers) && hit.transform != null && hit.transform.CompareTag("Player");
    }

    public void OnEnter()
    {
        _controller.meshRenderer.material.color = Color.darkBlue;
    }

    public void OnExit()
    {
    }

    public void OnHurt()
    {
    }

    public void OnDrawGizmosSelected()
    {
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
        Gizmos.DrawCube(posToMoveTo, cubeSize);
        
        Gizmos.color = Color.hotPink;
        Gizmos.DrawCube(lastSeenPos, cubeSize);
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

        if (Physics.Raycast(eyesTransform.position, playerTransform.position - enemyTransform.position, out RaycastHit hit,
                followRange, layers) && hit.transform != null && hit.transform.CompareTag("Player"))
            Gizmos.color = Color.blue;
        else
            Gizmos.color = Color.red;
        Gizmos.DrawLine(eyesTransform.position, playerTransform.position);
    }
}