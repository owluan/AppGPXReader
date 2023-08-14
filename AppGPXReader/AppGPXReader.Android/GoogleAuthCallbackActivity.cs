using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Xamarin.Essentials;

namespace AppGPXReader.Droid
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataSchemes = new[] { "com.googleusercontent.apps.165711430590-ekm6av6c7230kcq68v917h17oc8g4ntc" },
        DataPath = "/oauth2redirect")]
    public class GoogleAuthCallbackActivity : WebAuthenticatorCallbackActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }
    }
}
