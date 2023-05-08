using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace viviviare
{
    public class Player_Combat : MonoBehaviour
    {
        #region Variables
        [Header("Script References")]
        private Player_Health _playerHealth;
        private Player_Movement _playerMovement;
        [SerializeField] private Camera_Behaviour _playerCamera;

        [Header("Component References")]
        [SerializeField] private Animator _animator;

        [Header("Light Attack")]
        [SerializeField] private float _damage;
        [SerializeField] private GameObject _lightHitbox;
        [SerializeField] private float _lightAttackComboTime;
        private float _lightAttackUseTimer;
        private int _lightComboStage; //0 = No hits, 1 = 1 hit, 2 = 2 hits.
        [SerializeField] private float _lightAttackCooldown;
        [SerializeField] private float _lightAttackAnimationCutoff;

        [Header("Abilities")]
        private float _damageModifier;
        [SerializeField] private Ability_Blueprint _ability1;
        [SerializeField] private string _ability1Animation;
        [SerializeField] private float _ability1Cooldown;
        [SerializeField] private Image _ability1CooldownVisual;


        [SerializeField] private Ability_Blueprint _ability2;
        [SerializeField] private string _ability2Animation;
        [SerializeField] private float _ability2Cooldown;
        [SerializeField] private Image _ability2CooldownVisual;



        [SerializeField] private Ability_Blueprint _ability3;
        [SerializeField] private string _ability3Animation;
        [SerializeField] private float _ability3Cooldown;
        [SerializeField] private Image _ability3CooldownVisual;


        [SerializeField] private Ability_Blueprint[] _possibleAbilities;

        [Header("Guard Clauses")]
        private bool _canLightAttack;
        private bool _usingLightAttack;
        private bool _canAbility1;
        private bool _canAbility2;
        private bool _canAbility3;

        #endregion

        private void Awake()
        {
            _playerHealth = GetComponent<Player_Health>();
            _playerMovement = GetComponent<Player_Movement>();
        }

        public void AssignInput()
        {
            Input_Handler._i._playerActions.PlayerCombat.Block.performed += GetComponent<Player_Health>().Blocking;
            Input_Handler._i._playerActions.PlayerCombat.Block.canceled += GetComponent<Player_Health>().CancelBlock;

            Input_Handler._i._playerActions.PlayerCombat.Light_Attack.performed += LightAttack_Input;
            Input_Handler._i._playerActions.PlayerCombat.Ability1.performed += Ability1_Input;
            Input_Handler._i._playerActions.PlayerCombat.Ability2.performed += Ability2_Input;
            Input_Handler._i._playerActions.PlayerCombat.Ability3.performed += Ability3_Input;
        }

        private void Start()
        {
            _lightComboStage = 0;
            _lightAttackUseTimer = 0;
            _canLightAttack = true;

            _canAbility1 = true;
            _canAbility2 = true;
            _canAbility3 = true;

        }

        private void Update()
        {
            if (_lightAttackUseTimer > 0.1f && _canLightAttack) LightAttack_Timer();
        }

        #region Light Attack
        private void LightAttack_Input(InputAction.CallbackContext context)
        {
            if (!_canLightAttack || _usingLightAttack) return;
            _usingLightAttack = true;

            switch (_lightComboStage)
            {
                // Combo not started, Start attack 1
                case 0:
                    _lightComboStage++;
                    LightAttack_PlayAnimation("Light_Attack_1");

                    break;
                // 1 attack in the combo, Start attack 2
                case 1:
                    _lightComboStage++;
                    LightAttack_PlayAnimation("Light_Attack_2");

                    break;
                // 2 attacks in the combo, Start attack 3 / Final attack before cooldown.
                case 2:
                    _canLightAttack = false;
                    _lightComboStage++;
                    LightAttack_PlayAnimation("Light_Attack_3");

                    Invoke(nameof(LightAttack_Cooldown), _lightAttackCooldown);

                    break;
            }
        }

        private void LightAttack_PlayAnimation(string animationID)
        {
            // Increment combo stage and restart the timer if this attack is not the last
            if (_lightComboStage != 3) _lightAttackUseTimer = _lightAttackComboTime;

            // Animation event triggers LightAttack_TriggerHitbox()
            _animator.Play(animationID);
        }

        private void LightAttack_TriggerHitbox()
        {
            // Create a new (or reuse an existing) hitbox in the direction of the player's camera / orientation
            Vector3 hitboxPosition = transform.position + _playerMovement._playerOrientation.forward;
            GameObject newHitbox = Object_Pooling.Spawn(_lightHitbox, hitboxPosition, _playerMovement._playerOrientation.rotation);

            // Find all objects within the hitbox
            Collider[] hitColliders = Physics.OverlapBox(newHitbox.transform.position, newHitbox.transform.localScale / 2, Quaternion.identity);

            // Iterate through all colliders within the hitbox
            foreach (var c in hitColliders)
            {
                // Attack all enemies that are within the hitbox range
                if (c.tag == "Enemy")
                {
                    AI_Grunt enemy = c.GetComponent<AI_Grunt>();

                    enemy.TakeDamage(_damage + (1 * _damageModifier));
                }

            }
            StartCoroutine(LightAttack_DestroyHitbox(newHitbox));
            StartCoroutine(LightAttack_AllowNextHit());
        }

        private IEnumerator LightAttack_AllowNextHit()
        {
            yield return new WaitForSeconds(_animator.GetCurrentAnimatorClipInfo(1).Length - _lightAttackAnimationCutoff);
            _usingLightAttack = false;
        }

        private IEnumerator LightAttack_DestroyHitbox(GameObject hitbox)
        {
            yield return new WaitForSeconds(_animator.GetCurrentAnimatorClipInfo(1).Length);

            // Despawn the hitbox for later reuse
            Object_Pooling.Despawn(hitbox);
        }

        private void LightAttack_Timer()
        {
            _lightAttackUseTimer -= Time.deltaTime;
            if (_lightAttackUseTimer <= 0.1f)
            {
                _lightComboStage = 0;
                _lightAttackUseTimer = 0;

                // Invoke a half cooldown for failing to complete the combo
                Invoke(nameof(LightAttack_Cooldown), _lightAttackCooldown / 2);
            }
        }

        private void LightAttack_Cooldown()
        {
            _lightComboStage = 0;
            _canLightAttack = true;
            _usingLightAttack = false;
        }

        #endregion

        #region Ability inputs

        // Input connector for first ability keybind
        private void Ability1_Input(InputAction.CallbackContext context)
        {
            // Guard Clause: Do not run ability code if there is no ability set for this slot
            // or the ability is currently on cooldown
            if (!_canAbility1) return;
            _canAbility1 = false;

            _ability1.Activate(gameObject, _playerMovement._playerOrientation, "Enemy");
            StartCoroutine(Ability1_Cooldown());
        }

        private IEnumerator Ability1_Cooldown()
        {
            float timer = 0;
            while (timer < _ability1Cooldown)
            {
                timer += Time.deltaTime;
                _ability1CooldownVisual.fillAmount = (float)timer / (float)_ability1Cooldown;
                yield return new WaitForEndOfFrame();
            }

            _canAbility1 = true;
        }

        // Input connector for second ability keybind
        private void Ability2_Input(InputAction.CallbackContext context)
        {
            // Guard Clause: Do not run ability code if there is no ability set for this slot
            // or the ability is currently on cooldown
            if (!_canAbility2) return;
            _canAbility2 = false;

            _ability2.Activate(gameObject, _playerMovement._playerOrientation, "Enemy");
            StartCoroutine(Ability2_Cooldown());
        }

        private IEnumerator Ability2_Cooldown()
        {
            float timer = 0;
            while (timer < _ability2Cooldown)
            {
                timer += Time.deltaTime;
                _ability2CooldownVisual.fillAmount = (float)timer / (float)_ability2Cooldown;
                yield return new WaitForEndOfFrame();
            }

            _canAbility2 = true;
        }

        // Input connector for third ability keybind
        private void Ability3_Input(InputAction.CallbackContext context)
        {
            // Guard Clause: Do not run ability code if there is no ability set for this slot
            // or the ability is currently on cooldown
            if (!_canAbility3) return;
            _canAbility3 = false;

            _ability3.Activate(gameObject, _playerMovement._playerOrientation, "Enemy");
            StartCoroutine(Ability3_Cooldown());
        }

        private IEnumerator Ability3_Cooldown()
        {
            float timer = 0;
            while (timer < _ability3Cooldown)
            {
                timer += Time.deltaTime;
                _ability3CooldownVisual.fillAmount = (float)timer / (float)_ability3Cooldown;
                yield return new WaitForEndOfFrame();
            }

            _canAbility3 = true;
        }

        #endregion

        public void TemporaryDamageBoost(float modifier, float timeUntilRemoved)
        {
            _damageModifier += modifier;
            StartCoroutine(RemoveDamageModifier(modifier, timeUntilRemoved));
        }

        private IEnumerator RemoveDamageModifier(float modifier, float timeUntilRemoved)
        {
            yield return new WaitForSeconds(timeUntilRemoved);
            _damageModifier -= modifier;
        }


    }
}
