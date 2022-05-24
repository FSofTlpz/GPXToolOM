using Xamarin.Forms;

namespace GPXToolOM {
   public partial class App : Application {

      /// <summary>
      /// paramterloser Konstruktor nur für Designer nötig
      /// </summary>
      public App() {
         InitializeComponent();

         //MainPage = new MainPage();
      }

      public App(object androidactivity) : this() {
         InitializeComponent();

         //MainPage = new MainPage();
         //MainPage = new MainPage(androidactivity);

         MainPage = new NavigationPage(new MainPage(androidactivity)) {
            BarBackgroundColor = Color.LightGreen,
            BarTextColor = Color.Black,
         };
      }

      protected override void OnStart() {
         // Handle when your app starts
      }

      protected override void OnSleep() {
         // Handle when your app sleeps
      }

      protected override void OnResume() {
         // Handle when your app resumes
      }
   }
}
