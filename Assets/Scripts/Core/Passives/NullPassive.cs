using System;

using Utils;

namespace Core.Passives
{
    /// <summary>
    /// Passive placeholder for unimplemented features
    /// </summary>
    [Serializable]
    public class NullPassive : IArtifact
    {
        public int Priority => 100;

        public void OnAttach(Unit owner)
        {
            Log.Warning("Passive not implemented!");
        }

        public void OnDetach(Unit owner)
        {
            //throw new System.NotImplementedException();
        }
    }
}
