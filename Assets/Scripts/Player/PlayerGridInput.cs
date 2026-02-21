using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerGridInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerGridStepper stepper;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveForward;
    [SerializeField] private InputActionReference moveBackward;
    [SerializeField] private InputActionReference turnLeft;
    [SerializeField] private InputActionReference turnRight;

    private void OnEnable()
    {
        this.Bind(this.moveForward);
        this.Bind(this.moveBackward);
        this.Bind(this.turnLeft);
        this.Bind(this.turnRight);
    }

    private void OnDisable()
    {
        this.Unbind(this.moveForward);
        this.Unbind(this.moveBackward);
        this.Unbind(this.turnLeft);
        this.Unbind(this.turnRight);
    }

    private void Bind(InputActionReference a)
    {
        if (a == null || a.action == null)
        {
            return;
        }

        a.action.Enable();
        a.action.performed += this.OnPerformed;
    }

    private void Unbind(InputActionReference a)
    {
        if (a == null || a.action == null)
        {
            return;
        }

        a.action.performed -= this.OnPerformed;
        a.action.Disable();
    }

    private void OnPerformed(InputAction.CallbackContext ctx)
    {
        if (this.stepper == null)
        {
            return;
        }

        InputAction action = ctx.action;

        if (this.moveForward != null && action == this.moveForward.action)
        {
            this.stepper.RequestMoveForward();
        }
        else if (this.moveBackward != null && action == this.moveBackward.action)
        {
            this.stepper.RequestMoveBackward();
        }
        else if (this.turnLeft != null && action == this.turnLeft.action)
        {
            this.stepper.RequestTurnLeft();
        }
        else if (this.turnRight != null && action == this.turnRight.action)
        {
            this.stepper.RequestTurnRight();
        }
    }
}
