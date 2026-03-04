using UnityEngine;

public class ViewmodelAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Update()
    {
        animator.SetBool("isMoving", PlayerCharacter.instance.GetState().Velocity != Vector3.zero);
        animator.SetBool("isGrounded", PlayerCharacter.instance.GetState().Grounded);
        animator.SetBool("isSliding", PlayerCharacter.instance.GetState().Stance == Stance.Slide);
    }
}
