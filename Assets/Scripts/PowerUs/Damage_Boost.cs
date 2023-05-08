using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace viviviare
{
    public class Damage_Boost : MonoBehaviour
    {
        [SerializeField] private float _damageMulti;
        [SerializeField] private float _damageMultiDuration;

        private void OnTriggerEnter(Collider info)
        {
            if (info.tag != "Player") return;

            Player_Combat combat = info.GetComponent<Player_Combat>();

            combat.TemporaryDamageBoost(_damageMulti, _damageMultiDuration);

            Object_Pooling.Despawn(gameObject);
        }
    }
}
