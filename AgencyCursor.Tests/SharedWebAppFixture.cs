using Xunit;

namespace AgencyCursor.Tests;

/// <summary>
/// Shared collection fixture that starts the web app once for ALL tests
/// instead of starting/stopping it for each test class
/// </summary>
[CollectionDefinition("Web App Collection")]
public class WebAppCollection : ICollectionFixture<PlaywrightFixture>
{
    // This class has no code, it's just used to define the collection
}
