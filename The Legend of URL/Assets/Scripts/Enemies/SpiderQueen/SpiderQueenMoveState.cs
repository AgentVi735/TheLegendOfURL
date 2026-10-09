using UnityEngine;
using UnityEngine.AI;

public class SpiderQueenMoveState : IEnemyState
{
    private SpiderQueenController _controller;
    
    private Transform playerTransform;
    private Transform enemyTransform;
    private Transform eyesTransform;
    private CharacterController character;
    private float speed;
    private float followRange;
    private float attackRange;
    private Vector3 posToMoveToLocal;
    private Vector3 posToMoveTo;
    private LayerMask layers;
    private NavMeshPath path;

    private float invalidTime;
    private const float maxInvalidTime = 1.6f;
    
    public void Initialise(EnemyController controller)
    {
        _controller = (SpiderQueenController) controller;
        character = _controller.characterController;
        enemyTransform = character.transform;
        eyesTransform = _controller.eyesTransform;
        speed = _controller.data.chaseSpeed;
        followRange = _controller.data.followRange;
        attackRange = _controller.data.attackRadius;
        playerTransform = _controller.player.transform;
        layers = _controller.raycastLayers;
    }

    public void UpdateState()
    {
        float distance = Vector3.Distance(enemyTransform.position, playerTransform.position);
        if (distance < attackRange)
        {
            _controller.ChangeState(_controller.attackChargeState);
            return;
        }

        bool isOnGround = Physics.Raycast(enemyTransform.position, -enemyTransform.up, out RaycastHit _,
            0.2f);

        path = new NavMeshPath();
        Vector3 pos = enemyTransform.position;
        Vector3 charPos = _controller.GetNavMeshPosition(pos);
        pos = charPos;
        Vector3 destinationPos = playerTransform.position;
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
                    invalidTime += Time.deltaTime;
                    if (invalidTime >= maxInvalidTime)
                        _controller.ChangeState(_controller.idleState);
                    return;
                }
            }
        }

        if (path.corners.Length < 2)
            return;
        posToMoveTo = path.corners[1];
        posToMoveToLocal = posToMoveTo - enemyTransform.position;

        enemyTransform.LookAt(playerTransform.position);

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

    public void OnEnter()
    {
        if (_controller.meshRenderer != null)
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

        if (Physics.Raycast(eyesTransform.position, playerTransform.position - enemyTransform.position, out RaycastHit hit,
                followRange, layers) && hit.transform != null && hit.transform.CompareTag("Player"))
            Gizmos.color = Color.blue;
        else
            Gizmos.color = Color.red;
        Gizmos.DrawLine(eyesTransform.position, playerTransform.position);
    }
}