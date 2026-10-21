using Microsoft.UI.Xaml.Media.Imaging;

namespace Tervia.App;

internal static class AppAssets
{
    public static BitmapImage LoadBitmap(string fileName)
    {
        var assetUri = new Uri($"ms-appx:///Assets/{Uri.EscapeDataString(fileName)}");
        return new BitmapImage(assetUri);
    }
}
