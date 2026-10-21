using Microsoft.UI.Xaml.Media.Imaging;

namespace Tervia.App;

internal static class AppAssets
{
    public static BitmapImage LoadBitmap(string fileName)
    {
        var assemblyDirectory = Path.GetDirectoryName(typeof(App).Assembly.Location);
        var assetDirectory = string.IsNullOrWhiteSpace(assemblyDirectory)
            ? AppContext.BaseDirectory
            : assemblyDirectory;
        var assetPath = Path.Combine(assetDirectory, "Assets", fileName);
        return new BitmapImage(new Uri(assetPath, UriKind.Absolute));
    }
}
