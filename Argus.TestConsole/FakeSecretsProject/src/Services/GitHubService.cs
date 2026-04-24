namespace FakeSecretsProject.Services;

public class GitHubService
{
    // Fake PAT â€” not a real token
    private const string token = "ghp_ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890ab";

    private const string api_key = "SomeThirdPartyApiKeyThatIsLongEnoughForDetection";

    public Task<string> GetReposAsync() => Task.FromResult("[]");
}
