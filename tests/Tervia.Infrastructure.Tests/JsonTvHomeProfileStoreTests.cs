using Tervia.Core;
using Tervia.Infrastructure;
using Xunit;

namespace Tervia.Infrastructure.Tests;

public sealed class JsonTvHomeProfileStoreTests
{
    [Fact]
    public async Task Save_and_load_keeps_each_tv_profile_local_and_normalized()
    {
        var root = Path.Combine(Path.GetTempPath(), $"tervia-home-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var paths = new TerviaPaths(root);
            using var store = new JsonTvHomeProfileStore(paths);
            var profile = new TvHomeProfile
            {
                Title = "Cinema",
                Theme = "violet",
                PinnedApplicationIds = ["com.example.video"],
            };

            await store.SaveAsync("192.168.1.42", profile, CancellationToken.None);
            var loaded = await store.LoadAsync("192.168.1.42", CancellationToken.None);

            Assert.NotNull(loaded);
            Assert.Equal(profile.Title, loaded.Title);
            Assert.Equal(profile.Theme, loaded.Theme);
            Assert.Equal(profile.PinnedApplicationIds, loaded.PinnedApplicationIds);
            Assert.Null(await store.LoadAsync("192.168.1.43", CancellationToken.None));
            Assert.Single(Directory.GetFiles(Path.Combine(paths.Profiles, "home"), "*.json"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
