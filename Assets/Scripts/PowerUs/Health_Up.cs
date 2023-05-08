using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace viviviare
{
    public class Health_Up : MonoBehaviour
    {
        [SerializeField] private float _healthToGive;

        private void OnTriggerEnter(Collider info)
        {
            if (info.tag != "Player") return;

            Player_Health health = info.GetComponent<Player_Health>();

            health.HealPlayer(_healthToGive);
            Object_Pooling.Despawn(gameObject);
        }



    }
}
