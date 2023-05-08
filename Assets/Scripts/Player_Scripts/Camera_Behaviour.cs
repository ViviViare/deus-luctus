using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace viviviare
{
    public class Camera_Behaviour : MonoBehaviour
    {

        #region Variables

        [SerializeField] private GameObject _player;
        private Player_Movement _playerMovement;
        public Camera _mainCamera;
        [SerializeField] private Animator _cameraStateMachine;

        // Cinemachine camera objects
        [SerializeField] private CinemachineFreeLook _shoulderCam;
        [SerializeField] private CinemachineFreeLook _shoulderCamRunning;
        [SerializeField] private CinemachineVirtualCamera _birdsEyeCam;
        [SerializeField] private CinemachineVirtualCamera _sideViewCam;
        [SerializeField] private CinemachineBrain _cinemachineBrain;


        // Camera view directions
        public Vector3 _shoulderCamViewDireciton;
        public Vector3 _birdsEyeViewDirection;
        public Vector3 _sideViewDirection;

        // Shoulders
        private bool _canSwapShoulders;
        [SerializeField] private CinemachineTargetGroup _playerShoulders;
        [SerializeField] private float _shoulderInterpolationSpeed;
        [SerializeField] private float _positiveShoulderOutcome;
        [SerializeField] private float _negativeShoulderOutcome;

        // Camera state machine
        public CameraState _currentCameraState;
        private bool _usingShoulderRunningCamera;
        public enum CameraState
        {
            Shoulder,
            Shoulder_Running,
            Birds_Eye,
            Side_View
        }

        #endregion
        public void AssignInput()
        {
            Input_Handler._i._cameraInput.PlayerCamera.Swap_Shoulder.performed += SwapShoulderTarget;
        }

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _playerMovement = _player.GetComponent<Player_Movement>();

            _mainCamera = Camera.main;
            _cinemachineBrain = _mainCamera.GetComponent<CinemachineBrain>();

            _currentCameraState = CameraState.Shoulder;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _canSwapShoulders = true;

            _playerShoulders.m_Targets[0].weight = _negativeShoulderOutcome;
            _playerShoulders.m_Targets[1].weight = _positiveShoulderOutcome;


        }


        private void LateUpdate()
        {
            _shoulderCamViewDireciton = transform.position - new Vector3(_mainCamera.transform.position.x, transform.position.y, _mainCamera.transform.position.z);
        }

        private void Update()
        {
            CameraStatemachine();
        }

        private void CameraStatemachine()
        {

            switch (_currentCameraState)
            {
                case (CameraState.Shoulder):

                    break;
                case (CameraState.Shoulder_Running):

                    break;
                case (CameraState.Birds_Eye):

                    break;
                case (CameraState.Side_View):

                    break;

            }
        }

        #region Shoulder camera
        private void SwapShoulderTarget(InputAction.CallbackContext context)
        {
            // Guard Clause: Only allow the player to swap shoulders if the game is not currently swapping shoulders and is in the shoulder camera state
            if (!_canSwapShoulders || _currentCameraState != CameraState.Shoulder) return;

            Debug.Log("swapping");
            _canSwapShoulders = false;
            // Right shoulder being used
            if (_playerShoulders.m_Targets[0].weight == _negativeShoulderOutcome)
            {
                StartCoroutine(ShoulderLerp(_positiveShoulderOutcome, _negativeShoulderOutcome, true));
            }
            // Left shoulder being used
            else if (_playerShoulders.m_Targets[0].weight >= _positiveShoulderOutcome * 0.9f)
            {
                StartCoroutine(ShoulderLerp(_negativeShoulderOutcome, _positiveShoulderOutcome, false));
            }
        }

        private IEnumerator ShoulderLerp(float leftOutcome, float rightOutcome, bool rightToLeft)
        {
            // Loop through a lerp between each shoulder objects weight value until it reaches its final position (either 0.1 or 0.9)
            // If going from right to left, then wait until the left shoulder is at 0.89 then snap to 0.9
            // Else if going from left to right, then wait until the right shoulder is at 0.89 then snap to 0.9
            while (rightToLeft && _playerShoulders.m_Targets[0].weight <= leftOutcome * 0.99f || !rightToLeft && _playerShoulders.m_Targets[1].weight <= rightOutcome * 0.99f)
            {
                _playerShoulders.m_Targets[0].weight = Mathf.Lerp(_playerShoulders.m_Targets[0].weight, leftOutcome, _shoulderInterpolationSpeed);
                _playerShoulders.m_Targets[1].weight = Mathf.Lerp(_playerShoulders.m_Targets[1].weight, rightOutcome, _shoulderInterpolationSpeed);

                // Perform the while loop each frame, the wait ensures the loop is not instantanious and does not jitter through the slerp
                yield return new WaitForEndOfFrame();
            }
            _playerShoulders.m_Targets[0].weight = leftOutcome;
            _playerShoulders.m_Targets[1].weight = rightOutcome;

            // Only allow the player to swap shoulders again once the current shoulder swapping operation has been completed
            _canSwapShoulders = true;
        }

        private void SwapShoulderCameras()
        {

            if (_playerMovement._isRunning && isMovingSideways())
            {
                Debug.Log("Running camera");
                _currentCameraState = CameraState.Shoulder_Running;
                _cameraStateMachine.Play("Shoulder_Running");
                _canSwapShoulders = false;

            }
            else if (_currentCameraState != CameraState.Shoulder)
            {
                Debug.Log("Shoulder camera");
                _currentCameraState = CameraState.Shoulder;
                _cameraStateMachine.Play("Shoulder");
                _canSwapShoulders = true;
            }
        }

        private bool isMovingSideways()
        {
            if (_playerMovement._inputDirection.z > 0.5f || _playerMovement._inputDirection.z < -0.5f) return false;

            if (_playerMovement._inputDirection.x > 0.5f || _playerMovement._inputDirection.x < -0.5f) return true;

            return false;
        }

        #endregion

    }
}
