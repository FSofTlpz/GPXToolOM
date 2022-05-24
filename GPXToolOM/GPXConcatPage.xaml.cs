using FSofTUtils.Xamarin;
using FSofTUtils.Xamarin.DependencyTools;
using FSofTUtils.Xamarin.Page;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GPXToolOM {

   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class GPXConcatPage : ContentPage {


      const string CONCATFILE = "ConcatFile";
      const string CONCATDESTFILE = "ConcatDestFile";

      const string CONCATSWITCHSHORT = "ConcatSwitchShort";
      const string CONCATSWITCHINSERTWAYPOINT = "ConcatSwitchWaypoint";
      const string CONCATSWITCHINSERTROUTE = "ConcatSwitchRoute";
      const string CONCATSWITCHDELETE = "ConcatSwitchDelete";

      object androidactivity;

      enum FileType {
         SrcFile,
         Destfile
      }

      FileType ChooseFile = FileType.SrcFile;

      PseudoFileList SrcFileList;

      string LastSrcFile = "";

      /// <summary>
      /// Noch bei der Arbeit?
      /// </summary>
      bool isBusy = false;

      Color NormalButtonBackgroundColor;

      Color DisabledButtonBackgroundColor;

      StorageHelper sh;


      public GPXConcatPage() {
         InitializeComponent();
      }

      public GPXConcatPage(object androidactivity, StorageHelper sh) : this() {
         this.androidactivity = androidactivity;
         this.sh = sh;
      }

      protected override void OnAppearing() {
         base.OnAppearing();

         if (!DesignMode.IsDesignModeEnabled) {
            SrcFileList = new PseudoFileList(this.FindByName<global::Xamarin.Forms.StackLayout>("PseudoListviewSrcFiles"),
                                         "PseudolistCellFrame",
                                         "PseudolistCellDat1",
                                         "PseudolistCellDat2");
            if (androidactivity != null) { // sonst nur im Designer
               SrcFileList.Clear();
               SrcFileList.OnFrameTapped += SrcFileList_OnFrameTapped;
               GetAppProps4SrcFile();
            }

            //SrcFileList.Add(new PseudoList.Item("abd apxgrax", "19847585871"));
            //SrcFileList.Add(new PseudoList.Item("ösjtgb abd apxgrax", "19847585871"));

            // im Konstruktor fkt. noch nicht alle Oberflächenfunktionen (z.B. Scrollen)

            // fkt. nur mit android.permission.WRITE_USER_DICTIONARY und READ_USER_DICTIONARY !
            GetAppProps4SrcFile();
            Helper.SrollToEnd(SrcFileList.ParentStack);
            SetFilename(FileType.Destfile, Application.Current.Properties.ContainsKey(CONCATDESTFILE) ? Application.Current.Properties[CONCATDESTFILE] as string : "");
            switchShortConcat.IsToggled = Application.Current.Properties.ContainsKey(CONCATSWITCHSHORT) ? (bool)Application.Current.Properties[CONCATSWITCHSHORT] : true;
            switchInsertWaypoint.IsToggled = Application.Current.Properties.ContainsKey(CONCATSWITCHINSERTWAYPOINT) ? (bool)Application.Current.Properties[CONCATSWITCHINSERTWAYPOINT] : true;
            switchInsertRoute.IsToggled = Application.Current.Properties.ContainsKey(CONCATSWITCHINSERTROUTE) ? (bool)Application.Current.Properties[CONCATSWITCHINSERTROUTE] : true;
            switchFileDelete.IsToggled = Application.Current.Properties.ContainsKey(CONCATSWITCHDELETE) ? (bool)Application.Current.Properties[CONCATSWITCHDELETE] : false;

            NormalButtonBackgroundColor = ButtonStart.BackgroundColor;
            DisabledButtonBackgroundColor = Color.FromHsla(NormalButtonBackgroundColor.Hue,
                                                           NormalButtonBackgroundColor.Saturation,
                                                           NormalButtonBackgroundColor.Luminosity * 1.2);
         }
      }

      private void ButtonFile1_Tapped(object sender, EventArgs e) {
         FileTapped(FileType.SrcFile);
      }

      private void ButtonDestFile_Tapped(object sender, EventArgs e) {
         FileTapped(FileType.Destfile);
      }

      void FileTapped(FileType type) {
         string path = type == FileType.SrcFile ? LastSrcFile :
                               labelDestFile.Text.Trim().Length == 0 ? LastSrcFile : labelDestFile.Text;
         if (path != "")
            path = Path.GetDirectoryName(path);
         if (path == "")
            path = ChooseFilePage.LastChoosedPath;

         ChooseFilePage page = new ChooseFilePage() {
            AndroidActivity = androidactivity,
            Title = type == FileType.SrcFile ? "Trackdatei auswählen" :
                                               "Zieldatei auswählen",
            OnlyExistingFile = type != FileType.Destfile,
            Path = path,
            Filename = "",
         };
         page.ChooseFileReadyEvent += ChooseFileReadyEvent;
         ChooseFile = type; // für die Auswertung im Event-Handler
         Navigation.PushAsync(page);
      }

      private void ChooseFileReadyEvent(object sender, FSofTUtils.Xamarin.Control.ChooseFile.ChoosePathAndFileEventArgs e) {
         if (e.OK) {
            SetFilename(ChooseFile, Path.Combine(e.Path, e.Filename));
            ChooseFilePage.LastChoosedPath = e.Path;
         }
      }

      async void SetFilename(FileType type, string filename) {
         if (filename != "") {
            switch (type) {
               case FileType.SrcFile:
                  if (Path.GetExtension(filename).ToLower() != ".gpx") {
                     await Helper.MessageBox(this, "Fehler", "Es sind nur GPX-Dateien erlaubt.");
                     return;
                  }
                  long len = sh.GetFileAttributes(filename, false, out bool canread, out bool canwrite, out DateTime lastmod);
                  LastSrcFile = filename;
                  SrcFileList.Add(new PseudoFileList.Item(filename,
                                                          string.Format("{0} Byte, {1} kByte, {2} MByte / {3}",
                                                                len,
                                                                Math.Round(len / 1024.0, 1),
                                                                Math.Round(len / (1024.0 * 1024.0), 1),
                                                                lastmod.ToString("G"))));
                  Helper.SrollToEnd(SrcFileList.ParentStack);
                  SetAppProps4SrcFile();
                  break;

               case FileType.Destfile:
                  if (Path.GetExtension(filename).ToLower() != ".gpx")
                     filename += ".gpx";
                  labelDestFile.Text = filename;
                  Helper.SrollToEnd(labelDestFile);
                  Application.Current.Properties[CONCATDESTFILE] = filename;
                  break;
            }
         }
      }

      void SetAppProps4SrcFile() {
         List<string> tmp = new List<string>();
         for (int i = 0; i < SrcFileList.Count; i++) {
            tmp.Add(SrcFileList.Get(i).Data1);
            tmp.Add(SrcFileList.Get(i).Data2);
         }
         Application.Current.Properties[CONCATFILE] = string.Join("|", tmp);
      }

      void GetAppProps4SrcFile() {
         string old = "";
         if (Application.Current.Properties.ContainsKey(CONCATFILE))
            old = Application.Current.Properties[CONCATFILE] as string;
         if (old.Length > 0) {
            string[] tmp = old.Split(new char[] { '|' });
            if (tmp.Length > 0) {
               SrcFileList.Clear();
               for (int i = 1; i < tmp.Length; i += 2) {
                  SrcFileList.Add(new PseudoFileList.Item(tmp[i - 1], tmp[i]));
                  LastSrcFile = tmp[i - 1];
               }
            }
         }
      }

      private async void SrcFileList_OnFrameTapped(object sender, PseudoFileList.TappedEventArgs e) {
         bool remove = await Helper.MessageBox(this, "Achtung", "Soll die Datei '" + e.Item.Data1 + "' entfernt werden?", "ja", "nein");
         if (remove) {
            SrcFileList.RemoveAt(e.Position);
            SetAppProps4SrcFile();
         }
      }

      private void SwitchShortConcat_Toggled(object sender, ToggledEventArgs e) {
         Application.Current.Properties[CONCATSWITCHSHORT] = (sender as Switch).IsToggled;
      }

      private void SwitchInsertWaypoint_Toggled(object sender, ToggledEventArgs e) {
         Application.Current.Properties[CONCATSWITCHINSERTWAYPOINT] = (sender as Switch).IsToggled;
      }

      private void SwitchInsertRoute_Toggled(object sender, ToggledEventArgs e) {
         Application.Current.Properties[CONCATSWITCHINSERTROUTE] = (sender as Switch).IsToggled;
      }

      private void SwitchFileDelete_Toggled(object sender, ToggledEventArgs e) {
         Application.Current.Properties[CONCATSWITCHDELETE] = (sender as Switch).IsToggled;
      }


      private async void ButtonStart_Tapped(object sender, EventArgs e) {
         if (SrcFileList.Count < 1) {
            await Helper.MessageBox(this, "Info", "Es sind keine Ausgangsdateien angegeben.");
            return;
         }

         if (labelDestFile.Text.Trim().Length == 0) {
            await Helper.MessageBox(this, "Info", "Es ist keine Zieldatei angegeben.");
            return;
         }

         bool overwritedestfile = false;
         if (sh.FileExists(labelDestFile.Text.Trim())) {
            overwritedestfile = await Helper.MessageBox(this, "Achtung", "Die Zieldatei existiert schon. Soll sie überschrieben werden?", "ja", "nein");
            if (!overwritedestfile) {
               return;
            }
         }

         SetBusyStatus(true);

         List<string> srcfiles = new List<string>();
         for (int i = 0; i < SrcFileList.Count; i++)
            srcfiles.Add(SrcFileList.Get(i).Data1);

#pragma warning disable 4014  // async-Aufruf ohne await
         ConcatFilesAsync(srcfiles,
                          labelDestFile.Text.Trim(),
                          switchInsertWaypoint.IsToggled,
                          switchInsertRoute.IsToggled,
                          switchShortConcat.IsToggled,
                          switchFileDelete.IsToggled,
                          overwritedestfile,
                          ConcatFilesReady);
#pragma warning restore 4014
      }

      /// <summary>
      /// Callback-Funktion für die async-Funktion
      /// </summary>
      /// <param name="ok"></param>
      /// <param name="destfilename"></param>
      /// <param name="errormsg"></param>
      async void ConcatFilesReady(bool ok, string destfilename, string errormsg) {
         SetBusyStatus(false);

         if (errormsg == "")
            await Helper.MessageBox(this, "Verbinden", "Die Datei '" + destfilename + "' wurde erzeugt.");
         else
            await Helper.MessageBox(this, "Fehler beim Verbinden", errormsg);
      }

      /// <summary>
      /// i.W. die Kapselung von <see cref="ConcatFiles"/> in eine async-Funktion
      /// <para>Nach Abschluss wird eine Callbak-Funktion aufgerufen, die auch die Fehlermeldung einer ev. aufgetretenen Exception erhält.</para>
      /// </summary>
      /// <param name="filename1"></param>
      /// <param name="filename2"></param>
      /// <param name="destfilename"></param>
      /// <param name="shortestdist"></param>
      /// <param name="removeorgfiles"></param>
      /// <param name="overwritedestfile"></param>
      /// <param name="ready"></param>
      /// <returns></returns>
      private async Task ConcatFilesAsync(IList<string> srcfilename,
                                          string destfilename,
                                          bool insertwp,
                                          bool insertroute,
                                          bool shortestdist,
                                          bool removeorgfiles,
                                          bool overwritedestfile,
                                          Action<bool, string, string> ready) {
         string errormsg = "";
         bool res = await Task.Run(() => {
            bool ok = true;
            try {
               ConcatFiles(srcfilename, destfilename, insertwp, insertroute, shortestdist, removeorgfiles, overwritedestfile);
            } catch (Exception ex) {
               errormsg = ex.Message;
               ok = false;
            }
            return ok;
         });
         ready(res, destfilename, errormsg);
      }

      /// <summary>
      /// hier findet die eigentliche Arbeit statt
      /// </summary>
      /// <param name="filename1"></param>
      /// <param name="filename2"></param>
      /// <param name="destfilename"></param>
      /// <param name="shortestdist"></param>
      /// <param name="removeorgfiles"></param>
      /// <param name="overwritedestfile"></param>
      /// <returns></returns>
      void ConcatFiles(IList<string> srcfilename,
                       string destfilename,
                       bool insertwp,
                       bool insertroute,
                       bool shortestdist,
                       bool removeorgfiles,
                       bool overwritedestfile) {
         GpxFileExt destfile = new GpxFileExt(sh, destfilename);
         destfile.ConcatFiles(null,
                              srcfilename,
                              shortestdist,
                              insertwp,
                              insertroute,
                              removeorgfiles,
                              overwritedestfile);
      }

      void SetBusyStatus(bool isbusy) {
         isBusy = isbusy;
         ButtonStart.IsEnabled = !isbusy;
         ButtonStart.BackgroundColor = isBusy ? DisabledButtonBackgroundColor : NormalButtonBackgroundColor;
      }

      /// <summary>
      /// Event that is raised when the hardware back button is pressed. This event is not raised on iOS.
      /// </summary>
      /// <returns></returns>
      protected override bool OnBackButtonPressed() {
         if (!ButtonStart.IsEnabled)
            return true; // Disable Backbutton

         return base.OnBackButtonPressed(); // Standard
      }

   }

}