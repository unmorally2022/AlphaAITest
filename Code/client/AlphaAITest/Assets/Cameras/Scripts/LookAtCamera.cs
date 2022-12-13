using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityStandardAssets.Cameras
{
    public enum CameraState
    {
        ToPlayer, FreeCam, RotateAgaintsPlayer
    }

    public class LookAtCamera : MonoBehaviour
    {
        public GameObject target;
        [SerializeField]
        float damping = 1;

        //[SerializeField]
        Vector3 offset1, offset2, offset3;
        //[SerializeField]
        //Vector3 adder;

        [SerializeField]
        public CameraState cameraState;

        private Vector3 m_EulerAngleVelocity;

        private float currentAngle;
        private float desiredAngle;
        private float angle;
        private Quaternion rotation;

        void Start()
        {
            //offset = (target.transform.position) - transform.position;
            offset1 = new Vector3(0, -0.8199997f, 2.8f);
            offset2 = new Vector3(0, -20f, 75f);
            offset3 = new Vector3(0, -0.8199997f, 4.8f);
        }

        void LateUpdate()
        {
            switch (cameraState)
            {
                case CameraState.ToPlayer:
                    if (target != null)
                    {
                        currentAngle = transform.eulerAngles.y;
                        desiredAngle = target.transform.eulerAngles.y;
                        angle = Mathf.LerpAngle(currentAngle, desiredAngle, Time.deltaTime * damping);

                        rotation = Quaternion.Euler(0, angle, 0);
                        transform.position = (target.transform.position) - (rotation * offset1);

                        transform.LookAt(target.transform);

                        transform.position += new Vector3(0, 1.8f, 0);
                    }
                    break;
                case CameraState.FreeCam:
                    currentAngle = transform.eulerAngles.y;
                    desiredAngle += 0.1f;
                    if (desiredAngle > 360)
                        desiredAngle = 0;
                    angle = Mathf.LerpAngle(currentAngle, desiredAngle, Time.deltaTime * damping);

                    rotation = Quaternion.Euler(0, angle, 0);
                    transform.position = (new Vector3(0, 0, 0)) - (rotation * offset2);

                    transform.LookAt(new Vector3(0, 0, 0));

                    transform.position += new Vector3(0, 1.8f, 0);
                    break;
                case CameraState.RotateAgaintsPlayer:
                    if (target != null)
                    {
                        currentAngle = transform.eulerAngles.y;
                        desiredAngle += 0.5f;
                        if (desiredAngle > 360)
                            desiredAngle = 0;

                        angle = Mathf.LerpAngle(currentAngle, desiredAngle, Time.deltaTime * damping);

                        rotation = Quaternion.Euler(0, angle, 0);
                        transform.position = (target.transform.position) - (rotation * offset1);

                        transform.LookAt(target.transform);

                        transform.position += new Vector3(0, 0.8f, 0);
                    }
                    break;
            }
        }
    }
}