using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmergencyXR.UI
{
    public class SceneLoadButton : MonoBehaviour
    {
        [SerializeField]
        private string _sceneName;

        public void LoadScene()
        {
            if (string.IsNullOrWhiteSpace(_sceneName))
            {
                Debug.LogWarning("[SceneLoadButton] Scene name is empty.", this);
                return;
            }

            StartCoroutine(LoadSceneRoutine());
        }

        private IEnumerator LoadSceneRoutine()
        {
            // Disable OVRCameraRig one frame before loading to let
            // FromOVRHandDataSource.OnDisable() complete while transforms still exist.
            // Without this, the Meta XR SDK throws MissingReferenceException because
            // TrackingToWorldTransformerOVR tries to call TransformPoint on an already-
            // destroyed Transform during scene teardown.
            OVRCameraRig rig = FindFirstObjectByType<OVRCameraRig>();
            if (rig != null)
                rig.gameObject.SetActive(false);

            yield return null;

            SceneManager.LoadScene(_sceneName);
        }
    }
}
