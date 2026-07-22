namespace Tasks.Api.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;
        public string? Assignee { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }


        public bool CanTransitionTo(TaskStatus newStatus)
        {
            return (Status, newStatus) switch
            {
                (TaskStatus.ToDo, TaskStatus.InProgress) => true,
                (TaskStatus.InProgress, TaskStatus.Done) => true,
                _ => false 
            };
        }
    }
}