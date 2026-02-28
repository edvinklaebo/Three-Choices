namespace Core.Passives
{
    /// <summary>
    /// Passive placeholder for unimplemented features
    /// </summary>
    public class NullPassive : IPassive
    {
        public int Priority { get; } = 100;
        
        public void OnAttach(Unit owner)
        {
            //throw new System.NotImplementedException();
        }

        public void OnDetach(Unit owner)
        {
            //throw new System.NotImplementedException();
        }
    }
}