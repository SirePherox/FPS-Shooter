using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameDefaultSettings;

public class PlayerCharacterController : MonoBehaviour
{
   
    private CharacterController characterController;
    private DefaultActions inputActions;
    private Vector2 input_Movement;
    private Vector2 input_View;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder;
    public Transform feetTransform;

    [Header("Weapon Settings")]
    public WeaponController weaponController;


    [Header("Settings")]
    public PlayerSettings playerSettings;
    public float viewYMin;
    public float viewYMax;

    public LayerMask playerMask;

    [Header("Gravity Settings")]
    public float gravityAmount;
    public float gravityMin;
    public float playerGravity;
    private float yMinValue = -0.1f;

    [Header("Jump Settings")]
    public Vector3 jumpForce;
    public Vector3 jumpingForceVelocity;

    [Header("Stance")]
    public PlayerStances playerStance;
    public float playerStanceSmoothTime;
    private float stanceCheckErrorMargin = 0.05f;
    public PlayerStanceHeight playerStandStance;
    public PlayerStanceHeight playerCrouchStance;
    public PlayerStanceHeight playerProneStance;

    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceCapsuleCenterVelocity;
    private float stanceCapsuleHeightVelocity;

    private bool isSprinting;

    private Vector3 newMovement;
    private Vector3 newMovementVelocity;



    private void Awake()
    {
        //new input system
        inputActions = new DefaultActions();

        inputActions.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        inputActions.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        inputActions.Character.Jump.performed += e => Jump();
        inputActions.Character.Crouch.performed += e => Crouch();
        inputActions.Character.Prone.performed += e => Prone();
        inputActions.Character.Sprint.performed += e => ToggleSprint();
        inputActions.Character.SprintReleased.performed += e => StopSprint();

        inputActions.Enable();

        //

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;  //gets camera holder position

        //sets weapon controller initialization
        if (weaponController)
        {
            weaponController.Initialise(this);
        }
    }

    private void Update()
    {
        CalculateMovement();
        CalculateView();
        CalculateJump();
        CalculateStance();

    }

    private void CalculateView()
    {
        //also ,check for inversion of the view , does moving the mouse up ,move the view up??
        newCameraRotation.x += (playerSettings.viewYInverted ? input_View.y : -input_View.y) * playerSettings.viewYSensitivity * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewYMin, viewYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);

        //we would want to rotate the player for the Y rotation, so the forward direction can also change
        //rotating the character also rotate the cameraHolder, which rotates the camera
        //also check for view inversion
        newCharacterRotation.y += (playerSettings.viewXInverted ? -input_View.x : input_View.x) * playerSettings.viewXSensitivity * Time.deltaTime;

        transform.localRotation = Quaternion.Euler(newCharacterRotation);
    }

    private void CalculateMovement()
    {
        if(input_Movement.y <= 0.2f)
        {
            isSprinting = false; //make Sprinting false , if user isnt attempting to move 
        }


        var verticalSpeed = playerSettings.walkingForwardSpeed;
        var horizontalSpeed = playerSettings.walkingStrafeSpeed;

        if (isSprinting)
        {
            //changes player speed if the player is currently sprinting
            verticalSpeed = playerSettings.runningForwardSpeed;
            horizontalSpeed = playerSettings.runningStrafeSpeed;
        }
        #region Speed Effectors
        //change moving speed based on player stance
        if (!characterController.isGrounded)
        {
            playerSettings.speedEffector = playerSettings.fallingSpeedEffector;
        }
        else if(playerStance == PlayerStances.Crouch)
        {
            playerSettings.speedEffector = playerSettings.crouchSpeedEffector;
        }
        else if(playerStance == PlayerStances.Prone)
        {
            playerSettings.speedEffector = playerSettings.proneSpeedEffector;
        }
        else
        {
            playerSettings.speedEffector = 1;
        }

        verticalSpeed *= verticalSpeed * playerSettings.speedEffector;
        horizontalSpeed *= horizontalSpeed * playerSettings.speedEffector;
        #endregion
        newMovement = Vector3.SmoothDamp(newMovement, new Vector3(input_Movement.x * horizontalSpeed * Time.deltaTime, 0,input_Movement.y * verticalSpeed * Time.deltaTime),
                        ref newMovementVelocity,characterController.isGrounded ? playerSettings.movementSmoothing : playerSettings.jumpSmoothing);
       var movementSpeed = transform.TransformDirection(newMovement); //makes the movement relative to the player

        if(playerGravity > gravityMin ) //apply gravity if player isnt making the player jump
        {
            playerGravity -= gravityAmount * Time.deltaTime;
        }
        
        if(playerGravity < yMinValue&& characterController.isGrounded)
        {
            playerGravity = yMinValue;
        }

        movementSpeed.y += playerGravity; //apply gravity , when user is not on the ground

        movementSpeed += jumpForce * Time.deltaTime;

        characterController.Move(movementSpeed);
    }

    public void CalculateJump()
    {
        jumpForce = Vector3.SmoothDamp(jumpForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.jumpFallOffTime);
    }

    private void CalculateStance()
    {
        var currentStance = playerStandStance;

        if(playerStance == PlayerStances.Crouch)
        {
            currentStance = playerCrouchStance;
        }
        else if(playerStance == PlayerStances.Prone)
        {
            currentStance = playerProneStance;
        }

        cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, currentStance.cameraHeight, ref cameraHeightVelocity, playerStanceSmoothTime);      
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, cameraHeight, cameraHolder.localPosition.z);

        //change the capsule collider based on player stance
        characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StanceCollider.height, ref stanceCapsuleHeightVelocity, playerStanceSmoothTime);
        characterController.center = Vector3.SmoothDamp(characterController.center, currentStance.StanceCollider.center, ref stanceCapsuleCenterVelocity, playerStanceSmoothTime);
    }
    private void Jump()
    {
        if (!characterController.isGrounded || playerStance == PlayerStances.Prone)
        {
            return;
        }

        if(playerStance == PlayerStances.Crouch)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                //On changing to Stand-stance ,if player's capsule will collide with any other object, return from the function               
                return;
            }
            //if player is crouching and attempts to jump, goto stand stance
            playerStance = PlayerStances.Stand;
            return;
        }

        jumpForce = Vector3.up * playerSettings.jumpHeight;
        playerGravity = 0;
    }

    private void Crouch()
    {                      
        if(playerStance == PlayerStances.Crouch)
        { 
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                //On changing to Stand-stance ,if player's capsule will collide with any other object, return from the function               
                return;
            }

            //if player is already crouching, goto Stand stance
            playerStance = PlayerStances.Stand;
            return;
        }

        if (StanceCheck(playerCrouchStance.StanceCollider.height))
        {
            //before going to crouch stance, check if player's capsule will collide with any other object
            return;
        }
        playerStance = PlayerStances.Crouch;

    }
    private void Prone()
    {
        playerStance = PlayerStances.Prone;
    }

    private bool StanceCheck(float stanceCheckHeight)
    {
        var start = new Vector3(feetTransform.position.x, feetTransform.position.y + stanceCheckErrorMargin + characterController.radius, feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - stanceCheckErrorMargin - characterController.radius + stanceCheckHeight, feetTransform.position.z);

        return Physics.CheckCapsule(start,end,characterController.radius,playerMask);
    }

    private void ToggleSprint()
    {
        if (input_Movement.y <= 0.2f)
        {
            isSprinting = false; //make Sprinting false , if user isnt attempting to move 
            return;
        }
        isSprinting = !isSprinting; //toggles the boolean-value "isSprinting" 
    }
    private void StopSprint()
    {
        if (playerSettings.shouldHoldForSpriting)
        {
            isSprinting = false;
        }
             
    }
}
