using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class SimpleCameraRotation : MonoBehaviour
    {
        public float speed = 1.5f;
        public Camera m_camera;

        private Vector3 axis;

        private void Awake()
        {
            System.Random r = new System.Random();
            axis = new Vector3(r.Next(-100, 100) / 100.0f, r.Next(-100, 100) / 100.0f, r.Next(-100, 100) / 100.0f);
        }

        private void LateUpdate()
        {
            m_camera.transform.Rotate(axis, speed* Time.deltaTime);
        }

    }
}

