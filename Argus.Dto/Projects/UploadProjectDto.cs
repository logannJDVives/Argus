namespace Argus.Dto.Projects
{
    /// <summary>
    /// DTO for file upload endpoint
    /// This is a marker DTO used with [FromForm] for multipart/form-data binding.
    /// Swagger will generate the correct file upload form based on this DTO.
    /// </summary>
    public class UploadProjectDto
    {
        /// <summary>
        /// Project name (optional - can be extracted from file path)
        /// </summary>
        public string ProjectName { get; set; }
    }
}
