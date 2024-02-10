using UnityEngine;
using UnityEngine.EventSystems;

public class FPSController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float rotationSpeed = 2f;
    public float speedUpMultiplier = 2f;

    private bool isFreeToMove = true;
    public float verticalSpeed = 10f;


    private void Update()
    {
        LookAround();
        RotateCamera();


        //HandleMovementInput();
        //ApplyDeceleration();
        if (!ASWD_down())
        {
            shiftHoldTimer = 0f;
            //ApplyDeceleration();
            //Debug.Log("deacc");
        }

        if(isFreeToMove)
        {
            HandleLateralMovement();
            HandleVerticalMovement();
        }



        if (Input.GetKey(KeyCode.Space))
        {
            StartDeceleration();
        }
        else
        {
            HandleMovementInput();
        }

        if (isDecelerating)
        {
            ApplyDeceleration();
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
    private float shiftHoldDuration = 5f; // Set the duration after which speed increases

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
    private bool isDecelerating = false;
    private float deaccelerationTimer = 0f;
    public float deaccelerationDuration = 1f; // Adjust the duration as needed


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
                moveDirection = directionRN.normalized;
            }

            float currentSpeed = velocity.magnitude;
            if (ASWD_down())
            {
                currentSpeed = CalculateSpeed();
            }
            
            velocity = moveDirection * currentSpeed;

            // Apply momentum to the position
            transform.position += velocity * Time.deltaTime;

            directionRN = moveDirection;

            // Handle vertical movement (up and down)
            HandleVerticalMovement();

            // Handle lateral movement (left and right)
            HandleLateralMovement();
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
        if (Input.GetKey(KeyCode.O))
        {
            // Get the current camera rotation
            Quaternion currentRotation = transform.rotation;

            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, 0f, -rotationAmount));

            // Lerp towards the target rotation
            transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        }

        // Rotate camera anticlockwise on key press (q)
        if (Input.GetKey(KeyCode.U))
        {
            // Get the current camera rotation
            Quaternion currentRotation = transform.rotation;

            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, 0f, rotationAmount));

            // Lerp towards the target rotation
            transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
        }

    }
    private void HandleVerticalMovement()
    {
        float verticalInput = 0f;

        if (Input.GetKey(KeyCode.R)) // Move up
        {
            verticalInput = 1f;
        }
        else if (Input.GetKey(KeyCode.F)) // Move down
        {
            verticalInput = -1f;
        }

        // Smoothly translate the camera/player vertically
        transform.Translate(Vector3.up * verticalInput * verticalSpeed * Time.deltaTime, Space.World);
    }

    private void HandleLateralMovement()
    {
        float lateralInput = 0f;

        if (Input.GetKey(KeyCode.Q)) // Move to the left
        {
            lateralInput = -1f;
        }
        else if (Input.GetKey(KeyCode.E)) // Move to the right
        {
            lateralInput = 1f;
        }

        // Rotate the camera/player locally
        transform.Rotate(0f, lateralInput * rotationSpeed * Time.deltaTime, 0f);
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
        //float baseSpeed = velocity.magnitude;

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
    private void StartDeceleration()
    {
        if (!isDecelerating && velocity.magnitude > 0f)
        {
            // Start deceleration
            isDecelerating = true;
            deaccelerationTimer = 0f;
        }
    }

    private void ApplyDeceleration()
    {
        deaccelerationTimer += Time.deltaTime;

        // Smoothly interpolate to zero velocity
        float smoothMultiplier = Mathf.Lerp(1f, 0f, deaccelerationTimer / deaccelerationDuration);
        velocity *= smoothMultiplier;

        // Ensure velocity doesn't go negative during deceleration
        velocity = Vector3.Max(velocity, Vector3.zero);

        // Apply momentum to the position
        transform.position += velocity * Time.deltaTime;

        // Stop decelerating when velocity becomes very small
        if (velocity.magnitude < 0.01f)
        {
            isDecelerating = false;
            velocity = Vector3.zero;
            directionRN = Vector3.zero;
        }
    }

    private void ToggleMovement()
    {
        isFreeToMove = !isFreeToMove;
        Cursor.lockState = isFreeToMove ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isFreeToMove;
    }
}
