using UnityEngine;
namespace FlipperHockey
{
    [RequireComponent(typeof(Camera))]
    public class MainGameObjectCamera : MonoBehaviour
    {
        public static Camera Instance;

        void Awake()
        {
            // We already have a main camera and don't need a new one.
            if (Instance != null)
            {
                Debug.LogError("GRRR");
                Destroy(gameObject);
                return;
            }
            Instance = GetComponent<Camera>();
        }

        void OnDestroy()
        {
            if (Instance == GetComponent<Camera>())
            {
                Instance = null;
            }
        }
    }
}
