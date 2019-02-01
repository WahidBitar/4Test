using Stateless.Graph;

namespace Core
{
    public class Decision
    {
        public enum DecisionResults
        {
            Undetermined = 0,
            Approved = 1,
            Rejected = 2,
            //AskForModification = 3
        }

        public Person Approver { get; set; }
        public DecisionResults Result { get; set; }
        public string Notes { get; set; }
        public int Id { get; set; }
    }

}