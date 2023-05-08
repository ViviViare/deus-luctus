using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace viviviare
{
    public class Ability_SpearThrowBehaviour : MonoBehaviour
    {

        [Header("Scriptable Object Variables")]
        public string _enemyTag;
        public float _lifetime;
        public float _damage;

        [Header("Variables")]
        [SerializeField] private float _minimumDistance;
        [SerializeField] private Transform _pinTo;
        private List<GameObject> _hitTargets = new List<GameObject>();
        private Rigidbody _rigidbody;
        private bool _hitGround;


        private void Start()
        {
            _hitGround = false;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnTriggerEnter(Collider info)
        {
            if (!_hitGround && info.tag == _enemyTag && !AlreadyHitEnemy(info.gameObject))
            {
                AI_Grunt enemy = info.GetComponent<AI_Grunt>();
                enemy.TakeDamage(_damage);
                _hitTargets.Add(info.gameObject);
            }

            StartCoroutine(DespawnSpear());
        }

        private bool AlreadyHitEnemy(GameObject enemy)
        {
            if (_hitTargets.Contains(enemy)) return true;

            return false;
        }

        private void Update()
        {
            transform.rotation.SetLookRotation(_rigidbody.velocity);
            if (_hitGround) return;

            if (Physics.Raycast(transform.position, -transform.up, _minimumDistance))
            {
                _rigidbody.isKinematic = true;
                _hitGround = true;
            }

            if (_hitTargets.Count == 0) return;

            for (int i = 0; i < _hitTargets.Count; i++ )
            {
                _hitTargets[i].transform.position = _pinTo.position;
            }
        }


        private IEnumerator DespawnSpear()
        {
            yield return new WaitForSeconds(_lifetime);
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = false;
            _hitGround = false;
            _hitTargets.Clear();
            Object_Pooling.Despawn(gameObject);
        }

        private void OnDrawGizmos()
        {
            // Visualize minimum distance in editor
            Gizmos.DrawWireSphere(transform.position, _minimumDistance);
        }
    }
}
