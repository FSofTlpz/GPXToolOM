using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace GPXToolOM.Droid {

   // Groß-/Kleinschreibung für Buttons: siehe .\Test4FSofTUtils.Android\Resources\values\styles.xml


   [Activity(Label = "GPXToolOM", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
   public partial class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
      protected override void OnCreate(Bundle savedInstanceState) {
         //TabLayoutResource = Resource.Layout.Tabbar;
         //ToolbarResource = Resource.Layout.Toolbar;
         base.OnCreate(savedInstanceState);

         Xamarin.Essentials.Platform.Init(this, savedInstanceState);
         global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
         //LoadApplication(new App(this));
         //LoadApplication(new App());

         // zusätzlich
         onCreateExtend(savedInstanceState,
                        new string[] {
                              Manifest.Permission.AccessNetworkState,
                              Manifest.Permission.WriteExternalStorage,    // u.a. für Cache
                              Manifest.Permission.ReadExternalStorage,     // u.a. für Karten und Konfig.
                              //Manifest.Permission.AccessFineLocation,      // GPS-Standort            ACHTUNG: Dieses Recht muss ZUSÄTZLICH im Manifest festgelegt sein, sonst wird es NICHT angefordert!
                        }
                        );

      }

      public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
         Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

         // zusätzlich
         onRequestPermissionsResult(requestCode, permissions, grantResults);

         base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
      }


   }
}