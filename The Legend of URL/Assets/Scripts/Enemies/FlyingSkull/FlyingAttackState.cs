using UnityEngine;
using UnityEngine.AI;

public class FlyingAttackState : IEnemyState
{
    private FlyingSkullController _controller;

    private Transform enemyTrans;
    private Transform eyesTrans;
    private Transform playerTrans;
    private Transform targetTrans;

    private float timeSpent;
    private float totalTime;
    private float distance;

    private Quaternion enemyStartRotation;
    private Quaternion enemyEndRotation;
    private Quaternion targetStartRotation;
    private Quaternion targetEndRotation;

    private LayerMask raycastLayers;
    private bool hasHitPlayer;
    
    public void Initialise(EnemyController controller)
    {
        _controller = (FlyingSkullController) controller;
        enemyTrans = _controller.transform;
        eyesTrans = _controller.eyesTransform;
        playerTrans = _controller.player.transform;
        targetTrans = _controller.targetTrans;
        totalTime = _controller.data.attackRadius;
        raycastLayers = _controller.raycastLayers;
    }

    public void UpdateState()
    {
        float ratio = timeSpent / totalTime;
        Quaternion targetRot = Quaternion.Slerp(targetStartRotation, targetEndRotation, ratio);
        Quaternion enemyRot = Quaternion.Slerp(enemyStartRotation, enemyEndRotation, ratio);

        targetTrans.rotation = targetRot;
        enemyTrans.position = targetTrans.position + targetTrans.forward * distance;

        enemyTrans.rotation = enemyRot;

        if (!hasHitPlayer && Physics.Raycast(eyesTrans.position, playerTrans.position - enemyTrans.position, out RaycastHit hit,
                0.3f, raycastLayers) && hit.transform != null && hit.transform.CompareTag("Player"))
        {
            hasHitPlayer = true;
            _controller.player.OnHit(_controller.data.damage);
        }
        
        timeSpent += Time.deltaTime;

        if (timeSpent >= totalTime)
            _controller.Stun(_controller.data.attackCooldown);
    }

    public void OnEnter()
    {
        timeSpent = 0;
        _controller.meshRenderer.material.color = Color.darkRed;
        Vector3 targetPos = playerTrans.position;
        targetPos.y = enemyTrans.position.y;
        targetTrans.position = targetPos;
        enemyTrans.LookAt(targetTrans.position);
        targetTrans.rotation = enemyTrans.rotation;
        enemyStartRotation = Quaternion.Euler(89, enemyTrans.rotation.eulerAngles.y, 0);
        enemyEndRotation = Quaternion.Euler(-90, enemyTrans.rotation.eulerAngles.y, 0);
        targetStartRotation = Quaternion.Euler(180, enemyTrans.rotation.eulerAngles.y, 0);
        targetEndRotation = Quaternion.Euler(360, enemyTrans.rotation.eulerAngles.y, 0);
        // distance = Vector3.Distance(targetTrans.position, enemyTrans.position);
        NavMesh.SamplePosition(playerTrans.position, out NavMeshHit lastSeenPosNavMesh, 6, _controller.navMeshFilter);
        distance = Mathf.Abs(playerTrans.position.y + _controller.player._characterController.height / 2 -
                             lastSeenPosNavMesh.position.y - _controller.characterController.height / 2);
        hasHitPlayer = false;
    }

    public void OnExit()
    {
        enemyTrans.rotation = Quaternion.Euler(0, enemyTrans.rotation.eulerAngles.y, 0);
    }

    public void OnHurt()
    {
    }

    public void OnDrawGizmosSelected()
    {
    }
}