using UniGLTF;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    public class VRM10SetupDialog : EditorWindow
    {
        Transform m_modelRoot;
        VRM10Object m_vrmObject;

        private string SaveTitle => "Save VRM10Object to...";
        private string[] SaveExtensions => new string[] { "asset" };

        const string SETUP_VRM_KEY = VRMVersion.MENU + "/Setup VRM-1.0";

        [MenuItem(SETUP_VRM_KEY, false, 0)]
        private static void Init()
        {
            var window = (VRM10SetupDialog)GetWindow(typeof(VRM10SetupDialog));
            window.titleContent = new GUIContent("Setup VRM-1.0");
            window.Show();
        }

        void OnEnable()
        {
            if (Selection.activeObject != null) {
                Transform transform;
                if (Selection.activeGameObject.TryGetComponent<Transform>(out transform))
                {
                    m_modelRoot = transform;
                }
            }
        }

        void OnGUI()
        {
            GUILayout.Label("1, Model", EditorStyles.boldLabel);

            m_modelRoot = (Transform)EditorGUILayout.ObjectField("Model Root", m_modelRoot, typeof(Transform), true);

            if (m_modelRoot != null)
            {
                if (m_modelRoot.gameObject.GetComponent<Animator>() == null)
                {
                    EditorGUILayout.HelpBox("モデルのルートにAnimatorがありません。\nモデルをHumanoidとしてインポートするのを忘れていませんか？", MessageType.Warning);
                }
            }

            EditorGUILayout.Space();

            GUILayout.Label("2, VRM10Object", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(m_vrmObject != null);
            {
                if (GUILayout.Button("Create new VRM10Object"))
                {
                    var saveName = (m_modelRoot?.name ?? "VRMObject") + ".asset";
                    var absolutePath = SaveFileDialog.GetPath(SaveTitle, saveName, SaveExtensions);
                    var path = ConvertAbsolutePathToAssetsPath(absolutePath);

                    if (string.IsNullOrEmpty(path))
                    {
                        Debug.LogError("The specified path is not inside of Assets/");
                    }
                    else
                    {
                        VRM10Object asset = ScriptableObject.CreateInstance<VRM10Object>();

                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();

                        m_vrmObject = asset;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            m_vrmObject = (VRM10Object)EditorGUILayout.ObjectField("VRM10Object", m_vrmObject, typeof(VRM10Object), true);

            EditorGUILayout.Space();

            GUILayout.Label("3, Setup", EditorStyles.boldLabel);

            var vrmInstance = m_modelRoot != null ? m_modelRoot.GetComponent<Vrm10Instance>() : null;
            var alreadyHasVrmInstance = vrmInstance != null;
            var isReady = m_modelRoot != null && m_vrmObject != null;

            EditorGUI.BeginDisabledGroup(!isReady || alreadyHasVrmInstance);
            {
                GUILayout.Label(alreadyHasVrmInstance ? "Setup done!" : "Click Setup to setup VRMInstance");
                if (GUILayout.Button("Setup"))
                {
                    vrmInstance = m_modelRoot.gameObject.AddComponent<Vrm10Instance>();
                    vrmInstance.Vrm = m_vrmObject;

                    Selection.activeGameObject = m_modelRoot.gameObject;
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Convert a given path relative from `Assets`.
        /// If given path is not inside of `Assets` it will return null instead.
        /// </summary>
        string ConvertAbsolutePathToAssetsPath(string path) {
            if (path == null)
            {
                return null;
            }

            if (!path.StartsWith(Application.dataPath))
            {
                return null;
            }

            return "Assets" + path.Substring(Application.dataPath.Length);
        }
    }
}
