using System.ComponentModel.DataAnnotations;

namespace Argus.Dto.Projects
{
    public class UpdateProjectDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }
}
