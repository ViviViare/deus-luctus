using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace viviviare
{
    [CreateAssetMenu]
    public class Ability_SpearThrow : Ability_Blueprint
    {
        #region variables
        [SerializeField] private float _velocityToApply;
        [SerializeField] private float _spawnOffsetX;
        [SerializeField] private float _spawnOffsetY;
        [SerializeField] private float _spearLifetime;
        [SerializeField] private float _turnSpeed;

        [SerializeField] private GameObject _damageHitbox;
        [SerializeField] private GameObject _groundHitbox;

        #endregion

        public override void Activate(GameObject parent, Transform orientaiton, string enemyTag)
        {
            // Remove any previously existing values
            Debug.Log("Activated");
            _parent = parent;
            _enemyTag = enemyTag;
            _orientation = orientaiton;
            InitializeSpear();
        }

        private void InitializeSpear()
        {
            Vector3 offset = (_orientation.up * _spawnOffsetY) + (_orientation.right * _spawnOffsetX);
            Vector3 spearPosition = _parent.transform.position + offset;

            GameObject newSpear = Object_Pooling.Spawn(_prop, spearPosition, _orientation.rotation);

            newSpear.GetComponent<Ability_SpearThrowBehaviour>()._enemyTag = _enemyTag;
            newSpear.GetComponent<Ability_SpearThrowBehaviour>()._lifetime = _spearLifetime;
            newSpear.GetComponent<Ability_SpearThrowBehaviour>()._damage= _damage;

            Rigidbody rigidbody = newSpear.GetComponent<Rigidbody>();
            Vector3 forceDirection = _orientation.forward;

            RaycastHit hit;
            if (Physics.Raycast(_orientation.transform.position, _orientation.forward, out hit, 500f))
            {
                forceDirection = (hit.point - spearPosition).normalized;
            }
            Vector3 forceToApply = forceDirection * _velocityToApply;

            rigidbody.AddForce(forceToApply, ForceMode.Impulse);
            MonoInstance._instance.StartCoroutine(ResetValues(newSpear));
        }

        private IEnumerator ResetValues(GameObject spear)
        {
            yield return new WaitForSeconds(_spearLifetime);

            _parent = null;
            _orientation = null;
            _enemyTag = "";
        }

    }
}
