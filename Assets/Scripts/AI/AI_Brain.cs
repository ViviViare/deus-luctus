using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace viviviare
{
    public class AI_Brain : MonoBehaviour
    {
        #region variables

        public Transform _player;
        public GameObject[] _combatEnabledEnemies;
        public List<GameObject> _allEnemies = new List<GameObject>();
        public float _enlistDistance;

        #endregion

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void Update()
        {
            // Do not iterate through enemies if there are no enemies
            if (_combatEnabledEnemies[0] == null) return;
            for (int i = 0; i < _combatEnabledEnemies.Length; i++)
            {
                if (_combatEnabledEnemies[i] == null) return;

                CheckIfStillCombatEnabled(_combatEnabledEnemies[i], i);
            }
        }

        private void CheckIfStillCombatEnabled(GameObject enemyToCheck, int arrayPosition)
        {
            if (CheckDistanceToPlayer(enemyToCheck) || enemyToCheck.GetComponent<AI_Grunt>()._currentHP > 0) return;
            Debug.Log("Removed " + enemyToCheck + " from the pool");
            enemyToCheck.GetComponent<AI_Grunt>()._combatEnabled = false;
            _combatEnabledEnemies[arrayPosition] = null;
        }

        public void BecomeCombatEnabled(GameObject enemyToCheck)
        {
            // Check if there is an avaliable slot for the new enemy to go into
            for (int i = 0; i < _combatEnabledEnemies.Length;)
            {
                if (_combatEnabledEnemies[i] == enemyToCheck) break;

                if (_combatEnabledEnemies[i] != null)
                {
                    i++;
                }
                else
                {
                    // If new enemy is within the distance range, then they are an enemy that is allowed to deal damage
                    if (!CheckDistanceToPlayer(enemyToCheck)) return;

                    _combatEnabledEnemies[i] = enemyToCheck;
                    enemyToCheck.GetComponent<AI_Grunt>()._combatEnabled = true;
                    break;
                }
            }
        }

        public void RemoveEnemy(GameObject enemy)
        {
            _allEnemies.Remove(enemy);

            // If all enemies are dead, then player has won
            if (_allEnemies.Count <= 0)
            {
                Menu_Manager._i.PlayerHasWon();
            }
        }

        // Check the enemy if they are within the neccissary distance to be counted as a combat enemy
        private bool CheckDistanceToPlayer(GameObject enemyToCheck)
        {
            float distanceToPlayer = Vector3.Distance(enemyToCheck.transform.position, _player.position);

            if (distanceToPlayer <= _enlistDistance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnDrawGizmos()
        {
            // Visualize minimum distance in editor
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_player.position, _enlistDistance);

        }
    }
}
