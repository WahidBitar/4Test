namespace Core.DomainModel.User
{
    public class UserAdded : IDomainEvent
    {
        public UserAdded(User user)
        {
            User = user;
        }


        public User User { get; set; }
    }
}