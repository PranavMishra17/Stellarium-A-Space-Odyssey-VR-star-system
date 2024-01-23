using UnityEngine;
using UnityEngine.EventSystems;

public class FPSController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float rotationSpeed = 2f;
    public float speedUpMultiplier = 2f;

    private bool isFreeToMove = true;

    private void Update()
    {
        LookAround();
        if (AnyKeyDown())
        {
            HandleMovementInput();
        }
        else
        {
            ApplyDeceleration();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleMovement();
        }
    }
    private bool AnyKeyDown()
    {
        // Check if any key is currently pressed
        return Input.anyKey;
    }

    private float shiftHoldTimer = 0f;
    private float shiftHoldDuration = 5f; // Set the duration after which speed increases
    public float targetSpeedMultiplier = 4f; // Set the intended speed multiplier

    private float decelerationFactor = 3f; // Adjust the deceleration factor as needed

    private Vector3 velocity; // Player's movement velocity

    private bool isForwardPressed = false;
    private bool isBackPressed = false;
    private bool isLeftPressed = false;
    private bool isRightPressed = false;
    private Vector3 directionRN;

    private void HandleMovementInput()
    {
        if (isFreeToMove)
        {
            UpdateKeyStates();

            // Rotate input based on camera
            Vector3 forwardDirection = Camera.main.transform.forward;
            Vector3 rightDirection = Camera.main.transform.right;

            Vector3 moveDirection = forwardDirection * (isForwardPressed ? 1f : 0f)
                                   + forwardDirection * (isBackPressed ? -1f : 0f)
                                   + rightDirection * (isRightPressed ? 1f : 0f)
                                   + rightDirection * (isLeftPressed ? -1f : 0f);

            float currentSpeed = CalculateSpeed();

            velocity = moveDirection * currentSpeed;

            // Apply momentum to the position
            transform.position += velocity * Time.deltaTime;

            
            directionRN = moveDirection;
        }
    }

    private void LookAround()
    {
        // Look up and down
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = -Input.GetAxis("Mouse Y") * rotationSpeed;

        transform.Rotate(0f, mouseX, 0f);

        // Adjust rotation for up and down movement
        Camera.main.transform.Rotate(mouseY, 0f, 0f);
    }

    private void UpdateKeyStates()
    {
        isForwardPressed = Input.GetKey(KeyCode.W);
        isBackPressed = Input.GetKey(KeyCode.S);
        isLeftPressed = Input.GetKey(KeyCode.A);
        isRightPressed = Input.GetKey(KeyCode.D);
    }

    private float CalculateSpeed()
    {
        float baseSpeed = movementSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            shiftHoldTimer += Time.deltaTime;

            // Smoothly interpolate to the target speed multiplier
            float smoothMultiplier = Mathf.Lerp(1f, targetSpeedMultiplier, shiftHoldTimer / shiftHoldDuration);
            baseSpeed *= smoothMultiplier;
        }

        // Ensure speed doesn't go negative during deceleration
        baseSpeed = Mathf.Max(baseSpeed, 0f);

        return baseSpeed;
    }
    private void ApplyDeceleration()
    {
        // Smoothly decelerate when movement buttons are released
        velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * decelerationFactor);

        //velocity = directionRN * velocity.magnitude;

        // Apply momentum to the position
        transform.position += velocity * Time.deltaTime;
    }

    private void ToggleMovement()
    {
        isFreeToMove = !isFreeToMove;
        Cursor.lockState = isFreeToMove ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isFreeToMove;
    }
}
