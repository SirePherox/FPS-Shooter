using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameDefaultSettings;

public class WeaponController : MonoBehaviour
{
    private PlayerCharacterController playerCharacterController;

    [Header("Setttings")]
    public WeaponSettings weaponSettings; 

    public void Initialise(PlayerCharacterController characterController)
    {
        playerCharacterController = characterController;
    }
}
