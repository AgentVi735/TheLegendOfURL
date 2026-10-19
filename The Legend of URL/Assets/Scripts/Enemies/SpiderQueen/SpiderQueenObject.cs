using UnityEngine;

public class SpiderQueenObject : MonoBehaviour
{
    [SerializeField] private SpiderQueenController _controller;

    public void FinishAttackAnimation() => _controller.FinishAttackAnimation();
}