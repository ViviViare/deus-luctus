using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace viviviare
{
    public class Pool
    {
        //Stack of all inactive gameobjects in this pool
        public Stack<GameObject> _inactive = new Stack<GameObject>();
        public GameObject _parent;

        public Pool(GameObject parent)
        {
            this._parent = parent;
        }
    }
}
