using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace viviviare
{
    public class AI_Grunt : MonoBehaviour
    {
        #region Variables

        [Header("Movement Variables")]
        public float _movementSpeed;
        private Vector3 walkPoint;
        private bool walkPointSet;
        [SerializeField] private float _chaseRange, _combatRange, _walkPointRange;
        [SerializeField] private float _chaseRangeIncreased;
        [SerializeField] private bool _playerInChaseRange, _playerInCombatRange, _playerInIncreasedChaseRange;

        [Header("Combat Variables")]
        [SerializeField] private float _damage;
        [SerializeField] private float _maxHP;
        [SerializeField] private float _cooldownPeriod;
        private bool _onAttackCooldown;
        public float _currentHP;
        public bool _combatEnabled;

        [Header("State machine")]

        public EnemyState _state;

        public enum EnemyState
        {
            Roaming,
            Chasing,
            Combat
        }

        private Animator _animator;

        [Header("References")]
        //[SerializeField] private EnemyHealthBehaviour healthBarScript;
        [SerializeField] private NavMeshAgent _agent;
        private Transform _player;
        [SerializeField] private AI_Brain _aiBrain;
        [SerializeField] private LayerMask _whatIsGround, _whatIsPlayer;
        [SerializeField] private Image _healthBar;
        [SerializeField] private Canvas _healthBarHolder;

        #endregion


        private void OnEnable()
        {
            _animator = GetComponent<Animator>();
            _aiBrain = GameObject.FindGameObjectWithTag("AI_Brain").GetComponent<AI_Brain>();
            _player = _aiBrain._player;

            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = _movementSpeed;

            _combatEnabled = false;
            _onAttackCooldown = false;

            _healthBarHolder.gameObject.SetActive(false);

            _damage = Mathf.Round((_damage + variance()) * 100f) / 100f;
            _maxHP = Mathf.Round((_maxHP * variance()) * 100f) / 100f;

            _currentHP = _maxHP;

            _aiBrain._allEnemies.Add(gameObject);
        }



        public void TakeDamage(float dmg)
        {
            _currentHP -= dmg;

            Debug.Log(gameObject + " took damage");

            _healthBarHolder.gameObject.SetActive(true);
            _healthBar.fillAmount = (float)_currentHP / (float)_maxHP;
            if (_currentHP <= 0)
            {
                _aiBrain.RemoveEnemy(gameObject);
                Object_Pooling.Despawn(gameObject);
            }
        }

        #region Navigation
        private void Update()
        {
            _playerInChaseRange = Physics.CheckSphere(transform.position, _chaseRange, _whatIsPlayer);
            _playerInCombatRange = Physics.CheckSphere(transform.position, _combatRange, _whatIsPlayer);
            _playerInIncreasedChaseRange = Physics.CheckSphere(transform.position, _chaseRangeIncreased, _whatIsPlayer);

            if (_playerInCombatRange && _state != EnemyState.Combat)
            {
                _state = EnemyState.Combat;

                if (_combatEnabled && !_onAttackCooldown) combatInitiation();

                else if (_combatEnabled) WaitForAction();

                else AttemptInitation();

                return;
            }

            // Increase the chase range if the player is already found
            if (_playerInChaseRange || (_state == EnemyState.Chasing && _playerInIncreasedChaseRange))
            {
                _state = EnemyState.Chasing;
                Chasing();
                return;
            }

            Roaming();

        }

        private void Roaming()
        {
            if (!walkPointSet) SearchWalkPoint();

            if (walkPointSet)
            {
                _agent.SetDestination(walkPoint);
            }

            _animator.SetBool("Attacking", false);
            _animator.SetBool("Moving", true);

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            if (distanceToWalkPoint.magnitude < 1f){
                walkPointSet = false;
            }
        }

        // Choose a random position around current position to move to
        private void SearchWalkPoint()
        {
            float randomx = UnityEngine.Random.Range(-_walkPointRange, _walkPointRange);
            float randomz = UnityEngine.Random.Range(-_walkPointRange, _walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomx, transform.position.y, transform.position.z + randomz);

            if (Physics.Raycast(walkPoint, -transform.up, 2f, _whatIsGround))
            {
                walkPointSet = true;
            }
        }

        private void Chasing()
        {
            _agent.SetDestination(_player.position);
            _animator.SetBool("Attacking", false);
            _animator.SetBool("Moving", true);

        }

        #endregion

        #region Combat

        private void AttemptInitation()
        {
            // Check if the enemy can become combat enabled
            if (!_combatEnabled) _aiBrain.BecomeCombatEnabled(gameObject);

            // If successful then initate combat
            if (_combatEnabled) combatInitiation();
        }

        private void combatInitiation()
        {
            _agent.SetDestination(transform.position);
            transform.LookAt(_player);

            _animator.SetBool("Attacking", true);
            _animator.SetBool("Moving", false);

            //healthBarScript.healthBar.gameObject.SetActive(true);
        }

        private void WaitForAction()
        {
            _agent.SetDestination(transform.position);
            transform.LookAt(_player);
        }

        private void AttackPlayer()
        {
            _onAttackCooldown = true;
            _player.GetComponent<Player_Health>().TakeDamage(_damage);
            Invoke(nameof(AttackCooldown), _cooldownPeriod);
        }

        private void AttackCooldown()
        {
            _onAttackCooldown = false;
        }

        #endregion
        private float variance()
        {
            float randomNum = Random.Range(0.95f, 1.5f);
            return randomNum;
        }

        // Visualize the walk and increased chase range when selected
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _walkPointRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _chaseRangeIncreased);
        }

        // Always visualize the combat and chase ranges
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _combatRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _chaseRange);
        }
    }
}