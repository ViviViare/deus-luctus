using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace viviviare
{
    public class Input_Handler : MonoBehaviour
    {
        public static Input_Handler _i {get; private set;}
        public Player_Actions _playerActions;
        public Player_Camera _cameraInput;

        [Header("Scripts requiring input")]
        [SerializeField] private Player_Movement _playerMovement;
        [SerializeField] private Player_Combat _playerCombat;
        [SerializeField] private Camera_Behaviour _cameraBehaviour;
        [SerializeField] private Menu_Manager _menuManager;
        private void Awake()
        {
            _i = this;
            _playerActions = new Player_Actions();
            _cameraInput = new Player_Camera();

            // Enable Player Actions
            _playerActions.PlayerCombat.Enable();
            _playerActions.PlayerMovement.Enable();
            _playerActions.PlayerMenu.Enable();

            // Enable Camera Actions
            _cameraInput.PlayerCamera.Enable();



            TriggerInputAssignment();
        }

        private void TriggerInputAssignment()
        {
            _playerMovement.AssignInput();
            _playerCombat.AssignInput();
            _cameraBehaviour.AssignInput();
            _menuManager.AssignInput();
        }
    }
}
