using UnityEngine;

public class SpiderQueenObject : MonoBehaviour
{
    [SerializeField] private SpiderQueenController _controller;

    public void FinishAttackAnimation() => _controller.FinishAttackAnimation();

    // ReSharper disable once UnusedMember.Global
    // This method is used in the LandingOnGround animation on the Spider Queen.
    // For some reason ReSharper doesn't see it.
    public void FinishLandAnimation() => _controller.FinishLandAnimation();
}