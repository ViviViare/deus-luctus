using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace viviviare
{
    public class Ability_Blueprint : ScriptableObject
    {
        #region Variables
        [Header("Combat Variables")]
        [SerializeField] protected float _damage;
        protected float _damageModifier;

        [Header("Configuration Variables")]
        [SerializeField] protected GameObject _prop;
        protected string _enemyTag;
        protected GameObject _parent;
        protected Transform _orientation;

        #endregion

        public virtual void Activate(GameObject parent, Transform orientation, string enemyTag){}
    }
}
