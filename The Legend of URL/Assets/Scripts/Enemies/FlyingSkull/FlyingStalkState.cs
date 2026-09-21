using UnityEngine;
using UnityEngine.AI;

public class FlyingStalkState : IEnemyState
{
    private FlyingSkullController _controller;

    private Transform enemyTrans;
    private Transform eyesTrans;
    private Transform playerTrans;
    private Transform targetTrans;
    private CharacterController character;
    private float speed;
    private float turnSpeed;

    private Vector3 destinationGlobal;
    private NavMeshPath navMeshPath;
    private int pathIdx;
    private Vector3 posToMoveToGlobal;
    private Vector3 posToMoveToLocal;

    private int destinationAmount;
    private int maxDestinations;

    private bool isParentedToPlayer;
    private Vector3 currentPlayerPos;
    private Vector3 previousPlayerPos;
    
    public void Initialise(EnemyController controller)
    {
        _controller = (FlyingSkullController) controller;
        enemyTrans = _controller.transform;
        eyesTrans = _controller.eyesTransform;
        playerTrans = _controller.player.transform;
        targetTrans = _controller.targetTrans;
        character = _controller.characterController;
        speed = _controller.data.chaseSpeed;
        turnSpeed = _controller.data.turnSpeed;
    }

    public void UpdateState()
    {
        previousPlayerPos = currentPlayerPos;
        currentPlayerPos = playerTrans.position;

        float distance;
        if (!CanSeePlayer())
        {
            distance = Vector3.Distance(enemyTrans.position, destinationGlobal);
            if (distance < 0.5)
            {
                _controller.ChangeState(_controller.lookState);
                return;
            }

            isParentedToPlayer = false;
        }
        else if (!isParentedToPlayer)
            isParentedToPlayer = true;

        Vector3 targetPos = playerTrans.position;
        targetPos.y = targetTrans.position.y;
        targetTrans.position = targetPos;

        destinationGlobal = targetTrans.position + targetTrans.forward * _controller.data.stalkRange;

        float dist = Vector3.Distance(destinationGlobal, enemyTrans.position);
        if (dist < 0.5f)
        {
            GetNewDestination(true);
            return;
        }

        navMeshPath = new NavMeshPath();
        Vector3 charPos = _controller.GetNavMeshPosition(enemyTrans.position, _controller.data.stalkRange + 1);
        Vector3 destinationPos = destinationGlobal;
        destinationPos.y = charPos.y;
        NavMesh.CalculatePath(charPos, destinationPos, _controller.navMeshFilter, navMeshPath);
        if (pathIdx == -1)
        {
            switch (navMeshPath.corners.Length)
            {
                case 1:
                    pathIdx = 0;
                    break;
                case > 1:
                    pathIdx = 1;
                    break;
                case 0:
                    pathIdx = 0;
                    break;
                default:
                    return;
            }
        }

        if (navMeshPath.corners == null || navMeshPath.corners.Length == 0 || navMeshPath.corners.Length <= pathIdx)
            posToMoveToGlobal = destinationGlobal;
        else
            posToMoveToGlobal = navMeshPath.corners[pathIdx];

        posToMoveToLocal = posToMoveToGlobal - enemyTrans.position;
        float deltaAngle = Vector3.Angle(enemyTrans.forward, posToMoveToLocal);
        Vector3 rotationAxis = Vector3.Cross(enemyTrans.forward, posToMoveToLocal);
        rotationAxis.x = 0;
        rotationAxis.z = 0;
        Quaternion deltaRotation = Quaternion.AngleAxis(deltaAngle, rotationAxis);
        enemyTrans.rotation = Quaternion.Lerp(enemyTrans.rotation, enemyTrans.rotation * deltaRotation,
            turnSpeed * Time.deltaTime);

        Vector3 diffPos = enemyTrans.position;
        diffPos.y = posToMoveToGlobal.y;
        distance = Vector3.Distance(diffPos, posToMoveToGlobal);

        Vector3 movePos = Vector3.zero;
        if (distance >= 0.1f)
            movePos += enemyTrans.forward * (speed * Time.deltaTime);

        Vector3 directionToCheck = Vector3.zero;
        if (destinationGlobal.y > enemyTrans.position.y)
            directionToCheck = enemyTrans.up;
        else if (destinationGlobal.y < enemyTrans.position.y)
            directionToCheck = -enemyTrans.up;

        if (directionToCheck.y != 0)
        {
            if (!Physics.Raycast(_controller.leftTransform.position, directionToCheck, out RaycastHit _,
                    character.height, _controller.raycastLayers))
            {
                if (!Physics.Raycast(_controller.rightTransform.position, directionToCheck, out RaycastHit _,
                        character.height, _controller.raycastLayers))
                {
                    if (!Physics.Raycast(_controller.eyesTransform.position, directionToCheck, out RaycastHit _,
                            character.height, _controller.raycastLayers))
                    {
                        Vector3 flyMovement = directionToCheck * (_controller.data.gravitySpeed * Time.deltaTime);

                        float amountToFly = Mathf.Abs(destinationGlobal.y - enemyTrans.position.y);
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

        if (isParentedToPlayer)
            movePos += currentPlayerPos - previousPlayerPos;

        character.Move(movePos);

        posToMoveToGlobal.y = enemyTrans.position.y;
        distance = Vector3.Distance(posToMoveToGlobal, enemyTrans.position);

        if (Physics.Raycast(eyesTrans.position, enemyTrans.forward, out RaycastHit enemyHit, 1))
        {
            if (enemyHit.transform.CompareTag("Enemy"))
            {
                pathIdx = -1;
                return;
            }
        }

        if (distance >= 0.5f) return;
        pathIdx++;
    }

    private void GetNewDestination(bool succeeded)
    {
        float rot = Random.Range(160f, 360f);
        targetTrans.Rotate(0, rot, 0);
        destinationGlobal = targetTrans.position + targetTrans.forward * _controller.data.stalkRange;
        if (succeeded)
        {
            destinationAmount++;
            if (destinationAmount > maxDestinations)
                _controller.ChangeState(_controller.moveState);
        }
        pathIdx = -1;
    }
    
    private bool CanSeePlayer()
    {
        float distance = Vector3.Distance(playerTrans.position, enemyTrans.position);
        Vector3 playerPos = playerTrans.position;
        RaycastHit hit;
        if (distance <= _controller.data.forceDetectDistance)
        {
            return Physics.Raycast(enemyTrans.position, playerPos - enemyTrans.position, out hit,
                       _controller.data.followRange, _controller.raycastLayers) && hit.transform != null &&
                   hit.transform.CompareTag("Player");
        }
        if (!(distance < _controller.data.followRange)) return false;
        return Physics.Raycast(enemyTrans.position, playerPos - enemyTrans.position, out hit,
                   _controller.data.followRange, _controller.raycastLayers) && hit.transform != null &&
               hit.transform.CompareTag("Player");
    }

    public void OnEnter()
    {
        _controller.meshRenderer.material.color = Color.forestGreen;
        Vector3 targetPos = playerTrans.position;
        Vector3 charPos = _controller.GetNavMeshPosition(targetPos, enemyTrans.position);
        targetPos.y = charPos.y;
        targetTrans.position = targetPos;
        destinationAmount = 0;
        maxDestinations = Random.Range(_controller.data.stalkDestinationsAmount.x,
            _controller.data.stalkDestinationsAmount.y);
        GetNewDestination(true);
        isParentedToPlayer = true;
        previousPlayerPos = playerTrans.position;
        currentPlayerPos = previousPlayerPos;
    }

    public void OnExit()
    {
        targetTrans.gameObject.SetActive(false);
        if (!isParentedToPlayer) return;
        isParentedToPlayer = false;
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
        Gizmos.DrawCube(destinationGlobal, new Vector3(0.3f, 0.3f, 0.3f));
        if (navMeshPath is { corners: not null })
        {
            Gizmos.color = Color.purple;
            switch (navMeshPath.corners.Length)
            {
                case > 1:
                    Gizmos.DrawLineStrip(navMeshPath.corners, false);
                    break;
                case 1:
                    Gizmos.DrawLine(enemyTrans.position, navMeshPath.corners[0]);
                    break;
            }
        }

        Gizmos.color = Color.orange;
        
        if (Physics.Raycast(eyesTrans.position, playerTrans.position - enemyTrans.position, out RaycastHit hit,
                _controller.data.followRange, _controller.raycastLayers) && hit.transform != null && hit.transform.CompareTag("Player"))
            Gizmos.color = Color.blue;
        else
            Gizmos.color = Color.red;
        Gizmos.DrawLine(eyesTrans.position, playerTrans.position);
    }
}