using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace viviviare
{
    public class Player_Movement : MonoBehaviour
    {

        #region Variables

        [Header("Player References")]
        private Rigidbody _rigidbody;
        [SerializeField] private Transform _playerObject;
        public Transform _playerOrientation;
        [SerializeField] private Transform _playerShoulders;
        [SerializeField] private float _playerHeight;
        private Animator _animator;

        [Header("Script References")]
        [SerializeField] private Camera_Behaviour _camBehaviour;
        private Player_Health _playerHealth;

        [Header("Player Inputs")]
        public Vector3 _inputDirection;
        private float _xRaw, _zRaw;

        [Header("Speed Variables")]
        [SerializeField] private AnimationCurve _speedByAngle = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(90f, 0.5f), new Keyframe(180, 0.5f));
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private float _speedTuning;
        private float _maxSpeed;
        [SerializeField] private float _runMultiplier;

        [Header("Jump Variables")]
        [SerializeField] private float _jumpForce;
        [SerializeField] private float _jumpMultiplier;
        [SerializeField] private float _jumpCooldown;
        [SerializeField] private float _groundedDrag;
        [SerializeField] private LayerMask _whatIsGround;

        [Header("Slope Variables")]
        [SerializeField] private float _maxSlopeAngle;
        private RaycastHit _onSlopeHit;
        private bool _exitingSlope;

        [Header("Dash Variables")]
        [SerializeField] private float _forwardDashBoost;
        private bool _isDashing;
        [SerializeField] private float _dashCooldown;

        [Header("Guard Clauses")]
        public bool _canWalk;
        private bool _canDash;
        private bool _canJump;
        public bool _isRunning;
        private bool _isGrounded;

        [Header("Gravity Variables")]
        [SerializeField] private float _gravityCoeffeicient;
        private float _previousYPosition;

        #endregion

        public void AssignInput()
        {
            // Subscribe the Dash input to the Dash() function
            Input_Handler._i._playerActions.PlayerMovement.Dash.performed += Dash;
            // Subscribe the ToggleRun input to the ToggleRun() function
            Input_Handler._i._playerActions.PlayerMovement.RunToggle.performed += ToggleRun;
            // Subscribe the Jumping input to the Jump() function
            Input_Handler._i._playerActions.PlayerMovement.Jumping.performed += Jump;
        }

        private void Start()
        {
            // Setup Variables
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();

            _canWalk = true;
            _canDash = true;
            _canJump = true;
        }

        private void FixedUpdate()
        {
            WalkRun();
            SpeedLimiter();
            _isGrounded = OnGround();

            // Add drag to rigidbody when player is on the ground and not currently dashing
            if (_isGrounded && !_isDashing)
            {
                _rigidbody.drag = _groundedDrag;
            }
            else
            {
                _rigidbody.drag = 0;

                // Add additional gravity to the player if they are currently falling
                if (transform.position.y < _previousYPosition) ArtificalGravity();
            }

            _previousYPosition = transform.position.y;
        }

        private void LateUpdate()
        {
            ShoulderOrientation();
        }

        private void ShoulderOrientation()
        {
            _playerShoulders.forward = _playerOrientation.forward;
        }

        private void WalkRun()
        {
            // Guard Clause: Do not run movement code if the player cannot walk
            if (!_canWalk) return;

            // zRaw = Forward
            // xRaw = Sideways
            _xRaw = Input_Handler._i._playerActions.PlayerMovement.Movement.ReadValue<Vector2>().x;
            _zRaw = Input_Handler._i._playerActions.PlayerMovement.Movement.ReadValue<Vector2>().y;
            _animator.SetFloat("Z_Axis", _zRaw);
            _animator.SetFloat("X_Axis", _xRaw);

            // Handling the rotation of the player
            Vector3 viewDirection = transform.position - new Vector3(_camBehaviour._mainCamera.transform.position.x, transform.position.y, _camBehaviour._mainCamera.transform.position.z);
            _playerOrientation.forward = viewDirection.normalized;

            _inputDirection = (_playerOrientation.forward * _zRaw + _playerOrientation.right * _xRaw);

            float rawSpeed = _speedByAngle.Evaluate(Vector3.Angle(_inputDirection, _playerOrientation.forward));

            float refinedSpeed = rawSpeed * _speedTuning;

            if (_isRunning)
            {
                refinedSpeed *= _runMultiplier;
            }

            #region Rotation
            // Rotate player if not standing still and moving at less than a 90° angle
            float angleToDirection = Vector3.Angle(_playerOrientation.forward, _inputDirection);

            if (_inputDirection != Vector3.zero && angleToDirection <= 90)
            {
                _playerObject.forward = Vector3.Slerp(_playerObject.forward, _inputDirection, _rotationSpeed * Time.deltaTime);
            }
            else if (_inputDirection != Vector3.zero)
            {
                // Invert the x and z directions so that when the player is moving backwards, they still face the correct sideways direction
                float invertedOrientationX = _inputDirection.x * -1;
                float invertedOrientationZ = _inputDirection.z * -1;

                Vector3 correctedInputDirection = new Vector3(invertedOrientationX, _inputDirection.y, invertedOrientationZ);

                _playerObject.forward = Vector3.Slerp(_playerObject.forward, correctedInputDirection, _rotationSpeed * Time.deltaTime);
            }
            #endregion

            MovePlayer(refinedSpeed);
        }

        private void MovePlayer(float refinedSpeed)
        {
            // Check if the player is moving at an allowed angle.
            if (OnSlope())
            {
                _rigidbody.AddForce(GetSlopeMoveDirection() * refinedSpeed * 20f, ForceMode.Force);

                if (_rigidbody.velocity.y > 0)
                {
                    // Correct player movement when on slopes so they do not fly off
                    _rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
                }
            }
            // Check if player is on the ground
            else if (_isGrounded)
            {
                _rigidbody.AddForce(_inputDirection * refinedSpeed * 10f, ForceMode.Force);
            }
            // Check if the player is in the air
            else if (!_isGrounded)
            {
                _rigidbody.AddForce(_inputDirection * refinedSpeed * 10f * _jumpMultiplier, ForceMode.Force);
            }

            _rigidbody.useGravity = !OnSlope();
        }

        private void SpeedLimiter()
        {

            if (_isDashing)
            {

            }
            // Limit y velocity when on a slope
            // except when jumping off of a slope
            else if (OnSlope() && !_exitingSlope)
            {
                if (_rigidbody.velocity.magnitude > _speedTuning)
                {
                    _rigidbody.velocity = _rigidbody.velocity.normalized * _speedTuning;
                }
            }
            else
            {
                Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
                if (flatVelocity.magnitude > _speedTuning)
                {
                    Vector3 limitedVelocity = flatVelocity.normalized * _speedTuning;
                    _rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
                }
            }
        }

        private void ToggleRun(InputAction.CallbackContext context)
        {
            _isRunning = !_isRunning;
        }

        #region Jumping
        private void Jump(InputAction.CallbackContext context)
        {
            // Guard Clause: Do not allow jumping if the player is not on the ground and is not able to jump
            if (!_canJump || !_isGrounded) return;
            _canJump = false;
            _exitingSlope = true;

            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.y);

            _rigidbody.AddForce(transform.up * _jumpForce, ForceMode.Impulse);


            Invoke(nameof(EnableJumping), _jumpCooldown);
        }
        private void ArtificalGravity()
        {
            Debug.Log("Gravity Enabled");
            _rigidbody.AddForce(Vector3.down * _gravityCoeffeicient, ForceMode.Force);
        }
        private void EnableJumping()
        {
            _canJump = true;
            _exitingSlope = false;
        }

        #endregion

        #region Player Position Checks
        private bool OnSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out _onSlopeHit,  _playerHeight * 0.5f + 0.3f))
            {
                float slopeAngle = Vector3.Angle(Vector3.up, _onSlopeHit.normal);

                // Return true if the raycast finds an angle that is less than the maximum allowed angle
                // and the raycast isn't hitting a flat surface
                return slopeAngle < _maxSlopeAngle && slopeAngle != 0;
            }
            return false;
        }

        private Vector3 GetSlopeMoveDirection()
        {
            return Vector3.ProjectOnPlane(_inputDirection, _onSlopeHit.normal).normalized;
        }

        private bool OnGround()
        {
            return Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, _whatIsGround);
        }

        #endregion

        #region Dashing
        private void Dash(InputAction.CallbackContext context)
        {
            // Guard Clause: If the player cannot walk or dash, do not run code
            if (!_canWalk || !_canDash || _isDashing) return;
            _isDashing = true;
            Vector3 forceToApply = _playerOrientation.forward * _forwardDashBoost;

            _rigidbody.AddForce(forceToApply, ForceMode.Impulse);

            Invoke(nameof(ResetDash), _dashCooldown);
        }

        private void ResetDash()
        {
            _isDashing = false;
        }


        #endregion
    }
}