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
        RotateCamera();


        //HandleMovementInput();
        //ApplyDeceleration();
        if (!ASWD_down())
        {
            ApplyDeceleration();
            Debug.Log("deacc");
        }
        else
        {
            HandleMovementInput();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleMovement();
        }
    }
    private bool ASWD_down()
    {
        // Check if any of the ASWD keys are currently pressed
        return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W);
    }


    private float shiftHoldTimer = 0f;
    private float deaccTimer = 0f;
    private float shiftHoldDuration = 5f; // Set the duration after which speed increases
    private float deaccelerationDuration = 5f;

    public float targetSpeedMultiplier = 4f; // Set the intended speed multiplier

    public float decelerationFactor = 3f; // Adjust the deceleration factor as needed

    public Vector3 velocity; // Player's movement velocity

    private bool isForwardPressed = false;
    private bool isBackPressed = false;
    private bool isLeftPressed = false;
    private bool isRightPressed = false;
    private Vector3 directionRN;

    public float rotationAmount = 100;
    public float rotationLerpSpeed = 5f;

    public Vector3 moveDirection;

    private void HandleMovementInput()
    {
        if (isFreeToMove)
        {
            UpdateKeyStates();

            // Rotate input based on camera
            Vector3 forwardDirection = Camera.main.transform.forward;
            Vector3 rightDirection = Camera.main.transform.right;

            moveDirection = forwardDirection * (isForwardPressed ? 1f : 0f)
                                   + forwardDirection * (isBackPressed ? -1f : 0f)
                                   + rightDirection * (isRightPressed ? 1f : 0f)
                                   + rightDirection * (isLeftPressed ? -1f : 0f);

            // If no keys are pressed, maintain the last non-zero movement direction
            if (moveDirection.magnitude == 0f && directionRN.magnitude != 0f)
            {
                moveDirection = directionRN;
            }

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

    void RotateCamera()
    {
        // Rotate camera clockwise on key press (e)
        if (Input.GetKey(KeyCode.E))
        {
            // Get the current camera rotation
            Quaternion currentRotation = transform.rotation;

            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, 0f, -rotationAmount));

            // Lerp towards the target rotation
            transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        }

        // Rotate camera anticlockwise on key press (q)
        if (Input.GetKey(KeyCode.Q))
        {
            // Get the current camera rotation
            Quaternion currentRotation = transform.rotation;

            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, 0f, rotationAmount));

            // Lerp towards the target rotation
            transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        }

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

        float baseSpeed = movementSpeed;

        if (!ASWD_down())
        {
            deaccTimer += Time.deltaTime;

            // Smoothly interpolate to the target speed multiplier
            float smoothMultiplier = Mathf.Lerp(1f, 0f, deaccTimer / deaccelerationDuration);
            baseSpeed *= smoothMultiplier;
        }
        else
        {
            // Reset the timer when any ASWD key is pressed
            deaccTimer = 0f;
        }

        // Ensure speed doesn't go negative during deceleration
        baseSpeed = Mathf.Max(baseSpeed, 0f);

        velocity = moveDirection * baseSpeed;

        // Apply momentum to the position
        transform.position += velocity * Time.deltaTime;

        directionRN = moveDirection;

    }

    private void ToggleMovement()
    {
        isFreeToMove = !isFreeToMove;
        Cursor.lockState = isFreeToMove ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isFreeToMove;
    }
}
