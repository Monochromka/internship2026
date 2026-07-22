using System.ComponentModel.DataAnnotations;

namespace Tasks.Api.Models
{
    public class ChangeTaskStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        public Entities.TaskStatus Status { get; set; }
    }
}