using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Artifact Reward Event")]
public class ArtifactRewardEventChannel : ScriptableObject
{
    public event Action<ArtifactDefinition> OnRaised;

    public void Raise(ArtifactDefinition artifact)
    {
        OnRaised?.Invoke(artifact);
    }
}
