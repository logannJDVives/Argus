namespace Argus.Services.Exceptions
{
    /// <summary>
    /// Thrown when an uploaded file fails validation (size, type, name).
    /// The Message is safe to return directly to the client.
    /// </summary>
    public sealed class UploadValidationException : Exception
    {
        public UploadValidationException(string message)
            : base(message) { }
    }
}
