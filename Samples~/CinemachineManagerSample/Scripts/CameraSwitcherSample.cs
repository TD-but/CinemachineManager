using UnityEngine;

namespace BUT.Utils.CineCameraManager
{
    public class CameraSwitcherSample : MonoBehaviour
    {
        public void EnableCamera(int num)
        {
            Debug.Log("Switching to camera " + num);
            CineCameraManager.Instance.SwitchTo(num);
        }
    }
}

