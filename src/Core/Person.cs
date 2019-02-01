namespace Core
{
    public class Person
    {
        public enum UserLevels
        {
            Employee = 0,
            GroupManager = 1,
            DivisionManager = 2,
            DepartmentManager = 3
        }

        public enum WorkPlaces
        {
            Group,
            Division,
            Department
        }

        public string Name { get; set; }
        public UserLevels Level { get; set; }
        public WorkPlaces WorkPlace { get; set; }
    }
}