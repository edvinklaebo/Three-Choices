using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Artifacts
{
    [CreateAssetMenu(menuName = "Artifacts/Artifact Pool")]
    public class ArtifactPool : ScriptableObject, IArtifactRepository
    {
        [SerializeField]
        private List<ArtifactDefinition> artifacts = new();

        public IReadOnlyList<ArtifactDefinition> GetAll() => artifacts;

#if UNITY_EDITOR
        [ContextMenu("Auto Populate")]
        public void AutoPopulate()
        {
            artifacts.Clear();

            var guids = AssetDatabase.FindAssets("t:ArtifactDefinition");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var artifact = AssetDatabase.LoadAssetAtPath<ArtifactDefinition>(path);

                artifacts.Add(artifact);
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}
