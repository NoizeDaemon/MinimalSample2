using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace MinimalSample2.Android
{
    [Activity(Label = "MinimalSample2.Android", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon", LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AvaloniaMainActivity
    {
    }
}