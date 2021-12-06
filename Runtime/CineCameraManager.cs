using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEditor;
using UnityEngine;

namespace BUT.Utils.CineCameraManager
{
    public class CineCameraManager : MonoBehaviour, ISerializationCallbackReceiver
    {
        #region Public Variables 
        
        private static CineCameraManager _instance;
        public static CineCameraManager Instance { get { return _instance; } }
        
        #endregion
        
        #region Private Variables

        [SerializeField] private GameObject virtualCamPrefab;
        
        private CinemachineBrain _brain;
        private Dictionary<int, CinemachineVirtualCamera> _cameras = new Dictionary<int, CinemachineVirtualCamera>();
        private CinemachineVirtualCamera _currentCam;
        private bool _isCineMachineBrainNull;
        private bool _allowDebug;

        #endregion

        #region Internal Serialization

        [SerializeField] private int defaultIndex;

        // Hide this, because it will not work correctly without the custom editor.
        [HideInInspector] [SerializeField] private List<CameraEntry> entries = new List<CameraEntry>();

        // Dictionary -> List
        public void OnBeforeSerialize()
        {
            entries.Clear();
            // You can iterate a dictionary like this if you want.
            foreach (KeyValuePair<int, CinemachineVirtualCamera> kvp in _cameras)
            {
                entries.Add(new CameraEntry(kvp.Key, kvp.Value));
            }
        }

        // List -> Dictionary
        public void OnAfterDeserialize()
        {
            _cameras.Clear();
            foreach (CameraEntry entry in entries)
            {
                _cameras.Add(entry.key, entry.value);
            }
        }

        [System.Serializable]
        public class CameraEntry
        {
            public int key;
            public CinemachineVirtualCamera value;

            public CameraEntry()
            {
            }

            public CameraEntry(int key, CinemachineVirtualCamera value)
            {
                this.key = key;
                this.value = value;
            }
        }

        #endregion
        
        #region MonoBehaviour Callbacks
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            } else {
                _instance = this;
            }
            SwitchTo(defaultIndex);
        }

        #endregion

        #region Public Methods 
        
        public void InitializeCameras()
        {
            _cameras = new Dictionary<int, CinemachineVirtualCamera>();
            GetChildRecursive(gameObject);
            SwitchTo(defaultIndex);
        }

        public void SwitchTo(int id)
        {
            CinemachineVirtualCamera cam;
            if ((cam = GetCamera(id)) == null) return;
            if (_currentCam != null && _currentCam == cam) return;
            if(_allowDebug) Debug.Log($"<color=aqua> Switching to Camera : {id} </color>");
            if (_currentCam != null) _currentCam.Priority = 1;
            cam.Priority = 10;
            _currentCam = cam;
            if (Application.isPlaying || _isCineMachineBrainNull) return;
            _brain.enabled = false;
            _brain.enabled = true;
        }

        public void SetDefaultCamera(int id)
        {
            if (!_cameras.ContainsKey(id)) return;
            if(_allowDebug) Debug.Log($"<color=aqua> DEFAULT CALLED WITH {id} </color>");
            defaultIndex = id;
            SwitchTo(id);
        }
        
        public CinemachineVirtualCamera GetCamera(int id)
        {
            return _cameras.ContainsKey(id) ? _cameras[id] : null;
        }
        
        #endregion
        
        #region Private Methods 

        private void GetChildRecursive(GameObject obj)
        {
            if (null == obj)
                return;

            foreach (Transform child in obj.transform)
            {
                if (null == child) continue;
                //child.gameobject contains the current child you can do whatever you want like add it to an array
                if (child.HasComponent<CinemachineVirtualCamera>())
                    AddCamera(child.gameObject);
                GetChildRecursive(child.gameObject);
            }
        }

        private void AddCamera(GameObject camera)
        {
            if(_allowDebug) Debug.Log($"<color=aqua> Adding Camera : {camera.name} </color>");
            var stringID = camera.name.Substring(3, 3);
            if(_allowDebug) Debug.Log($"<color=aqua> Camera ID : {stringID} </color>");
            if (!int.TryParse(stringID, out var id)) return;
            if(_allowDebug) Debug.Log($"<color=aqua> PARSED CAMERA </color>");
            var cineCamera = camera.GetComponent<CinemachineVirtualCamera>();
            cineCamera.Priority = 1;
            _cameras.Add(id, cineCamera);
        }
        
        #endregion

        #region Editor Only Methods

#if UNITY_EDITOR
        public void GetCineMachineBrain()
        {
            if (Camera.main == null)
            {
                Debug.LogError($"CineCameraManager: Main Camera Not Found in Game Scene");
                _isCineMachineBrainNull = true;
                return;
            }

            var mainCam = Camera.main;
            if (!mainCam.gameObject.HasComponent<CinemachineBrain>())
            {
                Debug.LogError($"CineCameraManager: Main Camera Does NOT Have CineMachine Brain Component");
                // mainCam.gameObject.AddComponent<CinemachineBrain>();
                _isCineMachineBrainNull = true;
                return;
            }

            _brain = Camera.main.GetComponent<CinemachineBrain>();
        }

        [ContextMenu("Toggle Debug Mode")]
        public void ToggleDebug()
        {
            _allowDebug = !_allowDebug;
        }

        [ContextMenu("Create Linked Virtual Camera")]
        public void CreateLinkedVirtualCamera()
        {
            //cast instantiated prefab into a gameobject.
            GameObject virtualCam = PrefabUtility.InstantiatePrefab(virtualCamPrefab) as GameObject;
            
            //set camera's position
            virtualCam.transform.position = Vector3.zero;
            //set camera's parent
            virtualCam.transform.parent = transform;
            //set camera's name.
            virtualCam.name = $"CM_{GetNextID()}_";
            //add new camera to the dictionary
            AddCamera(virtualCam);
            
            // Important! set the editor dirty so it can serialize newly added gameobject and then prompt save 
            EditorUtility.SetDirty(virtualCam);
        }
        
        [ContextMenu("Create Virtual Camera")]
        public void CreateVirtualCamera()
        {
            //cast instantiated prefab into a gameobject.
            GameObject virtualCam = Instantiate(virtualCamPrefab,Vector3.zero, Quaternion.identity, transform);
            
            //set camera's name.
            virtualCam.name = $"CM_{GetNextID()}_";
            //add new camera to the dictionary
            AddCamera(virtualCam);
            
            // Important! set the editor dirty so it can serialize newly added gameobject and then prompt save 
            EditorUtility.SetDirty(virtualCam);
        }

        private string GetNextID()
        {
            return _cameras.Keys.Count == 0 ? "000" : $"{(_cameras.Keys.Max() + 1):D3}";
        }

        private void EnableObjectOnDelay(GameObject obj)
        {
            obj.SetActive(true);
        }
#endif

        #endregion
    }
}

