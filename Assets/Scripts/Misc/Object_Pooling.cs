using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace viviviare
{
    public static class Object_Pooling
    {
        private static Dictionary<string, Pool> _allPools = new Dictionary<string, Pool>();


        // Recreating instantiation with object pooling functionality
        public static GameObject Spawn(GameObject go, Vector3 pos, Quaternion rot)
        {
            GameObject obj;
            // Remove the (Clone) from the new objects name
            string key = go.name.Replace("(Clone)", "");

            if (_allPools.ContainsKey(key))
            {
                // If there is no inactive object in a pool, then create a new object to be used in the pool.
                if (_allPools[key]._inactive.Count == 0)
                {
                    obj = Object.Instantiate(go, pos, rot, _allPools[key]._parent.transform);
                }
                else
                {
                    //Return and remove highest inactive object
                    obj = _allPools[key]._inactive.Pop();
                    //Re-use an inactive object instead of creating a new one.
                    obj.transform.position = pos;
                    obj.transform.rotation = rot;
                    obj.SetActive(true);
                }
            }
            else // No pre-existing pool found for object.
            {
                // Create new parent gameobject for all pooled items to go under
                GameObject newParent = new GameObject($"{key}_Pool");
                obj = Object.Instantiate(go, pos, rot, newParent.transform);
                // Create a new pool
                Pool newPool = new Pool(newParent);
                // Add pool to the _allPools dictionary for later access
                _allPools.Add(key, newPool);

            }

            return(obj);
        }

        //Recreating the Destroy() function with object pooling functionality
        public static void Despawn(GameObject go)
        {
            string key = go.name.Replace("(Clone)", "");

            if(_allPools.ContainsKey(key))
            {
                //Add the gameobject to the inactive pool list at the end of the list.
                _allPools[key]._inactive.Push(go);
                go.transform.position = _allPools[key]._parent.transform.position;
                go.SetActive(false);
            }
            else
            {
                GameObject newParent = new GameObject($"{key}_Pool");
                Pool newPool = new Pool(newParent);

                go.transform.SetParent(newParent.transform);

                _allPools.Add(key, newPool);
                _allPools[key]._inactive.Push(go);
                go.SetActive(false);
            }
        }

        //Clear the _allPools Dictionary
        public static void ClearPools()
        {
            _allPools.Clear();
        }
    }
}
