using UnityEngine;

public class FPSController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float rotationSpeed = 2f;
    public float speedUpMultiplier = 2f;

    private bool isFreeToMove = true;

    private void Update()
    {
        HandleMovementInput();

        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleMovement();
        }
    }

    private float shiftHoldTimer = 0f;
    private float shiftHoldDuration = 5f; // Set the duration after which speed increases
    public float targetSpeedMultiplier = 4f; // Set the intended speed multiplier

    private float decelerationFactor = 3f; // Adjust the deceleration factor as needed

    private void HandleMovementInput()
    {
        if (isFreeToMove)
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.W)) vertical = 1f;
            if (Input.GetKey(KeyCode.S)) vertical = -1f;
            if (Input.GetKey(KeyCode.A)) horizontal = -1f;
            if (Input.GetKey(KeyCode.D)) horizontal = 1f;

            // Rotate input based on camera
            Vector3 forwardDirection = Camera.main.transform.forward;
            Vector3 rightDirection = Camera.main.transform.right;

            Vector3 moveDirection = forwardDirection * vertical + rightDirection * horizontal;

            float currentSpeed = CalculateSpeed();

            Vector3 targetTranslation = moveDirection * currentSpeed * Time.deltaTime;

            // Smoothly adjust position using Translate
            transform.Translate(targetTranslation, Space.World);

            // Look up and down
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = -Input.GetAxis("Mouse Y") * rotationSpeed;

            transform.Rotate(0f, mouseX, 0f);

            // Adjust rotation for up and down movement
            Camera.main.transform.Rotate(mouseY, 0f, 0f);
        }
    }

    private float CalculateSpeed()
    {
        float baseSpeed = Input.GetKey(KeyCode.LeftShift) ? movementSpeed * speedUpMultiplier : movementSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            shiftHoldTimer += Time.deltaTime;

            // Smoothly interpolate to the target speed multiplier
            float smoothMultiplier = Mathf.Lerp(1f, targetSpeedMultiplier, shiftHoldTimer / shiftHoldDuration);
            baseSpeed *= smoothMultiplier;
        }
        else
        {
            // Smoothly decelerate when movement buttons are released
            baseSpeed = Mathf.Lerp(baseSpeed, 0f, Time.deltaTime * decelerationFactor);
        }

        // Ensure speed doesn't go negative during deceleration
        baseSpeed = Mathf.Max(baseSpeed, 0f);

        return baseSpeed;
    }


    private void ToggleMovement()
    {
        isFreeToMove = !isFreeToMove;
        Cursor.lockState = isFreeToMove ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isFreeToMove;
    }
}
