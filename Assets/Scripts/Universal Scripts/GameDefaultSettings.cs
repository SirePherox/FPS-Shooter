using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameDefaultSettings
{
    #region -Player Settings-
    public enum PlayerStances
    {
        Stand,
        Crouch,
        Prone
    }

    [Serializable]
    public class PlayerSettings
    {
        [Header("View Settings")]
        public float viewXSensitivity;
        public float viewYSensitivity;        
        
        public bool viewXInverted;
        public bool viewYInverted;

        [Header("Movement Settings - Running")]
        public float runningForwardSpeed;
        public float runningStrafeSpeed;

        [Header("Movement Settings - Walking")]
        public float walkingForwardSpeed;
        public float walkingBackwardSpeed;
        public float walkingStrafeSpeed;

        [Header("Movement Settings")]
        public bool shouldHoldForSpriting;
        public float movementSmoothing;

        [Header("Jump Settings")]
        public float jumpHeight;
        public float jumpFallOffTime;
        public float jumpSmoothing;

        [Header("Speed Effector")]
        public float speedEffector = 1;
        public float crouchSpeedEffector;
        public float proneSpeedEffector;
        public float fallingSpeedEffector;
    }

    [Serializable]
    public class PlayerStanceHeight //height of  camera for each of the player stances
    {
        public float cameraHeight;
        public CapsuleCollider StanceCollider;
    }
    #endregion

    #region Weapon Settings

    [Serializable]
    public class WeaponSettings
    {
        [Header("Sway Settings")]
        public float swayAmount;
        public float swaySmoothing;

    }
    #endregion
}
