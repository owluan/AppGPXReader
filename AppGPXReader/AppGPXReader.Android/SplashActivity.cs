using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace AppGPXReader.Droid
{
    [Activity(Label = "AppGPXReader", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme.Splash", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StartActivity(typeof(MainActivity));
            Finish();

            OverridePendingTransition(0, 0);
        }
    }
}