using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace viviviare
{
    public class Ability_SpikeBehaviour : MonoBehaviour
    {
        [Header("Scriptable Object Variables")]
        public string _enemyTag;
        public float _lifetime;
        public float _damage;

        private void OnTriggerEnter(Collider info)
        {
            Debug.Log(info.tag);
            // Attack all enemies that are within the hitbox range
            if (info.tag == _enemyTag)
            {
                Debug.Log("HIT ENEMY");
                AI_Grunt enemy = info.GetComponent<AI_Grunt>();
                enemy.TakeDamage(_damage);
            }
        }
    }
}
