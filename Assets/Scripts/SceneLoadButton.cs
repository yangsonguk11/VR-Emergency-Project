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

            SceneManager.LoadScene(_sceneName);
        }
    }
}
