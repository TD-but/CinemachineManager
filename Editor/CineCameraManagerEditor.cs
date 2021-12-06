using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace BUT.Utils.CineCameraManager
{
// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
    [CustomEditor(typeof(CineCameraManager))]
    [CanEditMultipleObjects]
    public class CineCameraManagerEditor : Editor
    {
        // Reorderable List for serializing dictionary items 
        private ReorderableList list;

        // Reference to instance of CineCameraManger (target)
        private CineCameraManager manager;

        private SerializedProperty virtualCam_Prop;
        private bool showPrefabs;

        public void OnEnable()
        {
            manager = (CineCameraManager) target;
            manager.GetCineMachineBrain();
            list = new ReorderableList(serializedObject, serializedObject.FindProperty("entries"), false, true, false,
                false);
            list.drawHeaderCallback = DrawHeader;
            list.drawElementCallback = DrawListItems; // Delegate to draw the elements on the list
            virtualCam_Prop = serializedObject.FindProperty("virtualCamPrefab");
        }

        // Draws the header
        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 35, EditorGUIUtility.singleLineHeight), "ID");
            EditorGUI.LabelField(new Rect(rect.x + 45, rect.y, 160, EditorGUIUtility.singleLineHeight), "Camera");
            EditorGUI.LabelField(new Rect(EditorGUIUtility.currentViewWidth - 170, rect.y, 50, EditorGUIUtility.singleLineHeight), "Default");
            EditorGUI.LabelField(new Rect(EditorGUIUtility.currentViewWidth - 120, rect.y, 100, EditorGUIUtility.singleLineHeight), "Transition");
        }

        // Draws the elements on the list
        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginDisabledGroup(true);
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 35, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("key"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + 45, rect.y, EditorGUIUtility.currentViewWidth - 250, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("value"), GUIContent.none);
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.Toggle(new Rect(EditorGUIUtility.currentViewWidth - 160, rect.y, 50, EditorGUIUtility.singleLineHeight),
                serializedObject.FindProperty("defaultIndex").intValue == element.FindPropertyRelative("key").intValue))
            {
                if (serializedObject.FindProperty("defaultIndex").intValue !=
                    element.FindPropertyRelative("key").intValue)
                    manager.SetDefaultCamera(element.FindPropertyRelative("key").intValue);
            }

            if (GUI.Button(new Rect(EditorGUIUtility.currentViewWidth - 120, rect.y, 95, EditorGUIUtility.singleLineHeight), "Solo Camera"))
            {
                // Debug.Log($"<color=orange> Pressed Button : {index} </color>");
                manager.SwitchTo(element.FindPropertyRelative("key").intValue);
            }
        }

        void DrawPrefabsDrawer()
        {
            EditorGUILayout.Space(20);
            showPrefabs = EditorGUILayout.Foldout(showPrefabs, "CineMachine Camera Prefabs");
            if (showPrefabs)
            {
                EditorGUILayout.PropertyField(virtualCam_Prop, new GUIContent("Virtual Camera Prefab"));
            }
        }

        // Called every frame to draw inspector GUI
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            list.DoLayoutList();
            if (GUILayout.Button("Initialize Cameras")) manager.InitializeCameras();
            if (GUILayout.Button("Add Virtual Camera")) manager.CreateVirtualCamera();
            if (GUILayout.Button("Add Linked Virtual Camera")) manager.CreateLinkedVirtualCamera();
            DrawPrefabsDrawer();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
