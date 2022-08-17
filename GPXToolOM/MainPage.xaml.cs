using FSofTUtils.Xamarin;
using FSofTUtils.Xamarin.DependencyTools;
using FSofTUtils.Xamarin.Page;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace GPXToolOM {
   public partial class MainPage : ContentPage {

      const string TITLE = "GPXToolOM, © by FSofT 17.8.2022";

      const string ORUXMAPGPXDB = "OruxmapGpxDb";

      object androidactivity;

      StorageHelper sh;

      /// <summary>
      /// paramterloser Konstruktor nur für Designer nötig
      /// </summary>
      public MainPage() {
         InitializeComponent();
      }

      public MainPage(object androidactivity) : this() {
         if (!DesignMode.IsDesignModeEnabled) {
            this.androidactivity = androidactivity;

            ////////////////////////////

            // nur zum einfacheren Testen
            //Application.Current.Properties[ORUXMAPGPXDB] = "/storage/emulated/0/oruxmaps/tracklogs/oruxmapstracks.db";

            ////////////////////////////

         }
      }

      protected override void OnAppearing() {
         base.OnAppearing();

         Title = TITLE + " (v" + Xamarin.Essentials.AppInfo.VersionString + ")";

         if (!DesignMode.IsDesignModeEnabled) {
            if (sh == null)
               sh = DepToolsWrapper.GetStorageHelper(androidactivity);
         }
      }

      protected override bool OnBackButtonPressed() { // wegen der Änderung in MainActivity.cs sowohl für Hard- als auch Software-Backbutton
         IReadOnlyList<Page> stack = Navigation.NavigationStack;
         if (stack.Count > 1) {
            Page actualpage = stack[stack.Count - 1];
            if (actualpage.SendBackButtonPressed())   // Weiterleiten an die akt. Seite
               return true;
         }
         return base.OnBackButtonPressed();
      }

      private void ButtonOruxmapConcatTapped(object sender, EventArgs e) {
         ButtonOruxmapTapped(OruxmapPage.TypeOfWork.ConcatTracks);
      }

      private void ButtonOruxmapSplitTapped(object sender, EventArgs e) {
         ButtonOruxmapTapped(OruxmapPage.TypeOfWork.SplitTracksegments);
      }

      private void ButtonOruxmapSplit2Tapped(object sender, EventArgs e) {
         ButtonOruxmapTapped(OruxmapPage.TypeOfWork.SplitTrack);
      }

      async private void ButtonOruxmapTapped(OruxmapPage.TypeOfWork omtype) {
         string oruxmapfile = "";
         if (Application.Current.Properties.ContainsKey(ORUXMAPGPXDB))
            oruxmapfile = Application.Current.Properties[ORUXMAPGPXDB] as string;

         OruxmapPage page = null;
         try {
            page = new OruxmapPage(androidactivity, sh, oruxmapfile, omtype);
         } catch (Exception ex) {
            page = null;
            await Helper.MessageBox(this, "Fehler", ex.Message);
         }
         if (page != null)
            await Navigation.PushAsync(page);
      }

      async private void ButtonConcatTapped(object sender, EventArgs e) {
         GPXConcatPage page = new GPXConcatPage(androidactivity, sh);
         await Navigation.PushAsync(page);
      }

      async private void ButtonSplitTapped(object sender, EventArgs e) {
         GPXSplitPage page = new GPXSplitPage(androidactivity, sh, GPXSplitPage.SplitType.OnlyTracksegments);
         await Navigation.PushAsync(page);
      }

      async private void ButtonSplitTapped2(object sender, EventArgs e) {
         GPXSplitPage page = new GPXSplitPage(androidactivity, sh, GPXSplitPage.SplitType.WithPoints);
         await Navigation.PushAsync(page);
      }

      /// <summary>
      /// ToolbarItem für die Oruxmaps-Datenbank wurde ausgewählt
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      async private void ToolbarItem_Clicked(object sender, EventArgs e) {
         string oruxmapfile = "";
         if (Application.Current.Properties.ContainsKey(ORUXMAPGPXDB))
            oruxmapfile = Application.Current.Properties[ORUXMAPGPXDB] as string;
         ChooseFilePage page = new ChooseFilePage() {
            AndroidActivity = androidactivity,
            Title = "Oruxmap Track-DB auswählen",
            OnlyExistingFile = true,
            Path = string.IsNullOrEmpty(oruxmapfile) ? "" : Path.GetDirectoryName(oruxmapfile),
            Filename = string.IsNullOrEmpty(oruxmapfile) ? "" : Path.GetFileName(oruxmapfile),
         };
         page.ChooseFileReadyEvent += OruxmapPage_ChooseFileReadyEvent;

         await Navigation.PushAsync(page);
      }

      private void OruxmapPage_ChooseFileReadyEvent(object sender, FSofTUtils.Xamarin.Control.ChooseFile.ChoosePathAndFileEventArgs e) {
         if (e.OK) {
            string filename = Path.Combine(e.Path, e.Filename);
            Application.Current.Properties[ORUXMAPGPXDB] = filename; // Auswahl speichern
         }
      }


   }
}
