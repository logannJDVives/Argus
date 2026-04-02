using System;

namespace Argus.Dto.Projects
{
    public class UploadProjectResponseDto
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
