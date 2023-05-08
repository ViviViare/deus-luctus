using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace viviviare
{
    public class MonoInstance : MonoBehaviour
    {
        public static MonoInstance _instance;

        private void Awake()
        {
            MonoInstance._instance = this;

            // Errors appear in build when this is enabled
            DebugManager.instance.enableRuntimeUI = false;
        }

    }
}
