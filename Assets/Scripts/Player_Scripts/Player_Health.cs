using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

namespace viviviare
{
    public class Player_Health : MonoBehaviour
    {
        [Header("Primary Stats")]
        public float _health;
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _regenPercent;
        [SerializeField] private float _healthRegenRate;

        [SerializeField] private float _takeDamageCooldown;
        [SerializeField] private float _combatTime;

        [Header("Blocking stats")]
        private bool _isBlocking;
        [SerializeField] private float _blockDamageReduction;
        [SerializeField] private float _blockCapacity;
        [SerializeField] private float _blockMaxCapacity;

        [Header("Guard Clauses")]
        private bool _inCombat;
        private bool _canTakeDamage;
        private bool _currentlyHealing;

        [Header("User Interface")]
        [SerializeField] private TextMeshProUGUI _healthText;

        private void Start()
        {
            _canTakeDamage = true;
            _inCombat = false;

            _blockCapacity = _blockMaxCapacity;
            _health = _maxHealth;

            UpdateHealthText();
        }

        private void Update()
        {
            if (!_inCombat && !_currentlyHealing && _health < _maxHealth) RegenerateHealth();
        }

        private void RegenerateHealth()
        {
            _currentlyHealing = true;
            _health += _maxHealth * (_regenPercent / 100);


            // Ensure that there is no health overflow
            if (_health > _maxHealth)
            {
                _health = _maxHealth;
            }
            UpdateHealthText();

            Invoke(nameof(HealthRegenDelay), _healthRegenRate);
        }

        public void HealPlayer(float healingAmount)
        {
            _health += healingAmount;
            if (_health > _maxHealth)
            {
                _health = _maxHealth;
            }
            UpdateHealthText();
        }

        private void HealthRegenDelay()
        {
            _currentlyHealing = false;
        }

        private void UpdateHealthText()
        {
            _healthText.text = "Health: " + _health;
        }

        #region Player Damage
        public void TakeDamage(float damage)
        {
            if (!_canTakeDamage) return;
            float damageToTake = damage;


            // Restart the combat timer whenever the player is hit
            StopCoroutine(InCombatTimer());
            StartCoroutine(InCombatTimer());

            _inCombat = true;


            if (_isBlocking && _blockCapacity > damageToTake)
            {
                damageToTake *= (_blockDamageReduction / 100);
                _blockCapacity -= damageToTake;
            }

            _health -= damageToTake;
            UpdateHealthText();

            if (_health <= 0)
            {
                KillPlayer();
                return;
            }

            Invoke(nameof(AllowDamage), _takeDamageCooldown);
        }


        private IEnumerator InCombatTimer()
        {
            _inCombat = true;

            float timer = 0;
            while (timer < _combatTime)
            {
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            StartCoroutine(RegenerateBlockCapcity());
            _inCombat = false;
        }

        #region Blocking
        private IEnumerator RegenerateBlockCapcity()
        {
            while (_blockCapacity < _blockMaxCapacity)
            {
                _blockCapacity += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        public void Blocking(InputAction.CallbackContext context)
        {
            _isBlocking = true;
        }
        public void CancelBlock(InputAction.CallbackContext context)
        {
            _isBlocking = false;
        }
        #endregion

        private void KillPlayer()
        {
            gameObject.SetActive(false);
            Menu_Manager._i.PlayerHasLost();
        }

        private void AllowDamage()
        {
            _canTakeDamage = true;
        }

        #endregion

    }
}
