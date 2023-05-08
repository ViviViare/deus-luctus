using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace viviviare
{
    [CreateAssetMenu]
    public class Ability_SpikePath : Ability_Blueprint
    {
        #region Variables
        [SerializeField] private float _distanceBetweenSpikes;
        [SerializeField] private float _initalOffset;
        [SerializeField] private int _amountOfSpikes;
        [SerializeField] private float _spikeLifetime;
        [SerializeField] private float _spikeGroundInset;
        private Vector3 _lastSpikePosition;
        [SerializeField] private float _spikeSpawnDelay;

        #endregion

        public override void Activate(GameObject parent, Transform orientaiton, string enemyTag)
        {
            _parent = parent;
            _enemyTag = enemyTag;
            _orientation = orientaiton;

            // Coroutines and invoke cannot be called within scriptable objects
            // using a script that exists solely for the purpose of running the despawn coroutine
            // mitigates this issue
            MonoInstance._instance.StartCoroutine(StartSpawning());
        }

        private IEnumerator StartSpawning()
        {
            for(int spikes = 0; spikes < _amountOfSpikes; spikes++)
            {
                SpawnSpike();
                yield return new WaitForSeconds(_spikeSpawnDelay);
            }
        }

        private void SpawnSpike()
        {
            Vector3 spikePosition = _parent.transform.position + (_orientation.forward * _initalOffset);
            spikePosition.y -= _spikeGroundInset;

            GameObject newspike = Object_Pooling.Spawn(_prop, spikePosition, _orientation.rotation);
            newspike.GetComponent<Ability_SpikeBehaviour>()._damage = _damage;
            newspike.GetComponent<Ability_SpikeBehaviour>()._enemyTag = "Enemy";
            newspike.GetComponent<Ability_SpikeBehaviour>()._lifetime = _spikeLifetime;

            Animator animator = newspike.GetComponent<Animator>();
            animator.Play("Spike_Path_Segment");
            if (_lastSpikePosition != Vector3.zero)
            {
                newspike.transform.position = _lastSpikePosition + (_orientation.forward * _distanceBetweenSpikes);
            }
            _lastSpikePosition = newspike.transform.position;


            MonoInstance._instance.StartCoroutine(DespawnSpikes(newspike));

        }

        private IEnumerator DespawnSpikes(GameObject spike)
        {
            yield return new WaitForSeconds(_spikeLifetime);
            ResetValues();
            Object_Pooling.Despawn(spike);
        }
        private void ResetValues()
        {
            _lastSpikePosition = Vector3.zero;
            _parent = null;
            _orientation = null;
            _enemyTag = "";
        }
    }
}
