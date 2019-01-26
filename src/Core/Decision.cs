namespace Core
{
    public class Decision
    {
        public enum DecisionType
        {
            Approved,
            Rejected
        }

        public Approver Approver { get; set; }
        public DecisionType Type { get; set; }
        public string Notes { get; set; }
    }
}