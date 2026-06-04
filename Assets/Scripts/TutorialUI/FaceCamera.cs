using UnityEngine;

namespace EmergencyXR.Tutorial
{
    /// <summary>
    /// 카메라의 위치만 기준으로 UI가 카메라를 바라보게 합니다.
    /// faceCamera를 끄면 UI가 월드 공간에 고정됩니다.
    /// </summary>
    public class FaceCamera : MonoBehaviour
    {
        [Tooltip("켜면 항상 카메라를 바라봅니다. 끄면 월드 공간에 고정됩니다.")]
        public bool faceCamera = true;

        private Transform _cameraTransform;
        private Quaternion _initialRotation;

        private void Start()
        {
            _initialRotation = transform.rotation;
            FindCameraTransform();
        }

        private void FindCameraTransform()
        {
            GameObject centerEye = GameObject.Find("CenterEyeAnchor");
            if (centerEye != null)
            {
                _cameraTransform = centerEye.transform;
                return;
            }

            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            if (!faceCamera)
            {
                transform.rotation = _initialRotation;
                return;
            }

            if (_cameraTransform == null)
            {
                FindCameraTransform();
                return;
            }

            // Z+를 카메라 반대 방향으로 향하게 해야 Canvas 앞면이 카메라를 향합니다.
            Vector3 dirAwayFromCamera = transform.position - _cameraTransform.position;
            transform.rotation = Quaternion.LookRotation(dirAwayFromCamera, Vector3.up);
        }
    }
}


