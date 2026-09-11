using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Firaw.SnapCopyText.Launcher;
using Xunit;

namespace Firaw.SnapCopyText.Tests;

public sealed class UpdateClientTests
{
    [Fact]
    public async Task FindUpdateAsync_SelectsCurrentArchitecture()
    {
        const string manifest = """
            {
              "version": "2.0.0",
              "releaseNotes": "Atualização de teste",
              "packages": {
                "x64": { "url": "setup-x64.exe", "sha256": "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "size": 10 },
                "x86": { "url": "setup-x86.exe", "sha256": "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", "size": 8 }
              }
            }
            """;
        var client = new UpdateClient(new HttpClient(new StaticResponseHandler(manifest)));

        UpdatePlan? plan = await client.FindUpdateAsync(
            new Uri("http://10.81.66.10/firaw-snapcopytext/update.json"),
            new Version(1, 1, 0),
            Architecture.X86);

        Assert.NotNull(plan);
        Assert.Equal(new Uri("http://10.81.66.10/firaw-snapcopytext/setup-x86.exe"), plan.PackageUri);
        Assert.Equal(new Version(2, 0, 0), plan.Version);
    }

    [Fact]
    public async Task FindUpdateAsync_ReturnsNullForCurrentVersion()
    {
        string manifest = CreateManifest("1.1.0", new string('A', 64), 3);
        var client = new UpdateClient(new HttpClient(new StaticResponseHandler(manifest)));

        UpdatePlan? plan = await client.FindUpdateAsync(
            new Uri("http://10.81.66.10/firaw-snapcopytext/update.json"),
            new Version(1, 1, 0),
            Architecture.X64);

        Assert.Null(plan);
    }

    [Fact]
    public async Task DownloadAndVerifyAsync_RejectsChangedPackage()
    {
        byte[] payload = Encoding.UTF8.GetBytes("conteúdo alterado");
        var client = new UpdateClient(new HttpClient(new StaticResponseHandler(payload)));
        var plan = new UpdatePlan(
            new Version(2, 0, 0),
            string.Empty,
            new Uri("http://10.81.66.10/firaw-snapcopytext/setup-x64.exe"),
            new string('A', 64),
            payload.Length);
        string destination = Path.Combine(Path.GetTempPath(), $"firaw-update-{Guid.NewGuid():N}.exe");

        await Assert.ThrowsAsync<CryptographicException>(() =>
            client.DownloadAndVerifyAsync(plan, destination));
        Assert.False(File.Exists(destination));
    }

    [Fact]
    public async Task DownloadAndVerifyAsync_AcceptsMatchingPackage()
    {
        byte[] payload = Encoding.UTF8.GetBytes("instalador Firaw");
        string hash = Convert.ToHexString(SHA256.HashData(payload));
        var client = new UpdateClient(new HttpClient(new StaticResponseHandler(payload)));
        var plan = new UpdatePlan(
            new Version(2, 0, 0),
            string.Empty,
            new Uri("http://10.81.66.10/firaw-snapcopytext/setup-x64.exe"),
            hash,
            payload.Length);
        string destination = Path.Combine(Path.GetTempPath(), $"firaw-update-{Guid.NewGuid():N}.exe");

        try
        {
            await client.DownloadAndVerifyAsync(plan, destination);
            Assert.Equal(payload, await File.ReadAllBytesAsync(destination));
        }
        finally
        {
            File.Delete(destination);
        }
    }

    private static string CreateManifest(string version, string hash, int size) => $$"""
        {
          "version": "{{version}}",
          "packages": {
            "x64": { "url": "setup-x64.exe", "sha256": "{{hash}}", "size": {{size}} },
            "x86": { "url": "setup-x86.exe", "sha256": "{{hash}}", "size": {{size}} }
          }
        }
        """;

    private sealed class StaticResponseHandler : HttpMessageHandler
    {
        private readonly byte[] _content;

        public StaticResponseHandler(string content) : this(Encoding.UTF8.GetBytes(content))
        {
        }

        public StaticResponseHandler(byte[] content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(_content)
            });
    }
}
