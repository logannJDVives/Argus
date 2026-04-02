namespace Argus.Interfaces.Models
{
    public enum FileCategory
    {
        SourceCode,   // .cs
        ProjectFile,  // .csproj, .sln, .props, .targets
        Config,       // .json, .xml, .config, .yaml, .yml
        Environment,  // .env, .env.*
        Docker,       // Dockerfile, .dockerfile
        CI            // azure-pipelines.yml, .github/workflows/*.yml
    }
}
