using UnityEngine;
using UnityEngine.AI;

public class BasicMoveState : IEnemyState
{
    private EnemyController _controller;
    
    private Transform playerTransform;
    private Transform enemyTransform;
    private Transform eyesTransform;
    private CharacterController character;
    private float speed;
    private float turnSpeed;
    private float followRange;
    private float forceDetectDistance;
    private float attackRange;
    private Vector3 posToMoveToLocal;
    private Vector3 posToMoveTo;
    private Vector3 lastSeenPos;
    private LayerMask layers;
    private NavMeshPath path;
    private bool canReachPlayer;

    private float invalidTime;
    private const float maxInvalidTime = 1.6f;
    
    public void Initialise(EnemyController controller)
    {
        _controller = controller;
        character = _controller.characterController;
        enemyTransform = character.transform;
        eyesTransform = _controller.eyesTransform;
        speed = _controller.data.chaseSpeed;
        turnSpeed = _controller.data.turnSpeed;
        followRange = _controller.data.followRange;
        forceDetectDistance = _controller.data.forceDetectDistance;
        attackRange = _controller.data.attackRadius;
        playerTransform = _controller.player.transform;
        layers = _controller.raycastLayers;
        lastSeenPos = playerTransform.position;
    }
    
    public void UpdateState()
    {
        bool canSeePlayer = CanSeePlayer();
        switch (canSeePlayer)
        {
            case false:
            {
                float distance = Vector3.Distance(enemyTransform.position, canReachPlayer ? lastSeenPos : posToMoveTo);
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
                float distance = Vector3.Distance(enemyTransform.position, lastSeenPos);
                if (distance < attackRange)
                {
                    _controller.ChangeState(_controller.attackState);
                    return;
                }
                break;
            }
        }
        
        bool isOnGround = Physics.Raycast(enemyTransform.position, -enemyTransform.up, out RaycastHit _,
            0.2f);
        
        path = new NavMeshPath();
        Vector3 pos = enemyTransform.position;
        Vector3 charPos = _controller.GetNavMeshPosition(pos);
        pos = charPos;
        Vector3 destinationPos = lastSeenPos;
        NavMesh.CalculatePath(pos, destinationPos, _controller.navMeshFilter, path);
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            canReachPlayer = false;
            NavMeshHit navMeshHit = _controller.GetNavMeshHit(lastSeenPos);
            if (navMeshHit.hit)
            {
                destinationPos = navMeshHit.position;
                NavMesh.CalculatePath(pos, destinationPos, _controller.navMeshFilter, path);
                if (path.status != NavMeshPathStatus.PathComplete)
                {
                    invalidTime += Time.deltaTime;
                    if (invalidTime >= maxInvalidTime)
                        _controller.ChangeState(_controller.idleState);
                    return;
                }
            }
        }
        else
            canReachPlayer = true;
        
        if (path.corners.Length < 2)
            return;
        posToMoveTo = path.corners[1];
        posToMoveToLocal = posToMoveTo - enemyTransform.position;
        
        if (canSeePlayer)
            lastSeenPos = playerTransform.position;
        
        float deltaAngle = Vector3.Angle(enemyTransform.forward, posToMoveToLocal);
        Vector3 rotationAxis = Vector3.Cross(enemyTransform.forward, posToMoveToLocal);
        rotationAxis.x = 0;
        rotationAxis.z = 0;
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaAngle, rotationAxis);
        enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, enemyTransform.rotation * deltaRotation,
            turnSpeed * Time.deltaTime);
        
        Vector3 velocity = Vector3.zero;
        if (isOnGround)
        {
            if (velocity.y < -2f)
                velocity.y = -2f;
        }
        
        velocity.y += _controller.data.gravitySpeed * Time.deltaTime;

        Vector3 movePos = enemyTransform.forward * (speed * Time.deltaTime);
        movePos += -enemyTransform.up * velocity.y;
        
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