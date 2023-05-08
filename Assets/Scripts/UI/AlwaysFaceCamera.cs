using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace viviviare
{
    public class AlwaysFaceCamera : MonoBehaviour
    {
        private Camera cam;
        private void Start()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            //Face the center position of the camera
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
}
