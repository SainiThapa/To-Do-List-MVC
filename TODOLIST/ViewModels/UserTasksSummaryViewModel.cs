namespace TODOLIST.ViewModels
{

    public class UserTasksSummaryViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string UserName {get; set;}
        public int TaskCount => Tasks?.Count ?? 0; 
        public List<TaskViewModel> Tasks { get; set; }
    }

}