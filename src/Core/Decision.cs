namespace Core
{
    public class Decision
    {
        public enum DecisionResults
        {
            Approved = 1,
            Rejected = 2,
            AskForModification = 3
        }

        public Person Approver { get; set; }
        public DecisionResults Result { get; set; }
    }

}