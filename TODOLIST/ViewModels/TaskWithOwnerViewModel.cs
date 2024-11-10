namespace TODOLIST.ViewModels
{
    public class TaskWithOwnerViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsActive { get; set; }
        public string Owner_FullName { get; set; }
        public string OwnerEmail { get; set; }
    }
}