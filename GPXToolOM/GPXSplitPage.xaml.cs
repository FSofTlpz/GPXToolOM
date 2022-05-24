using FSofTUtils.Xamarin;
using FSofTUtils.Xamarin.DependencyTools;
using FSofTUtils.Xamarin.Page;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GPXToolOM {

   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class GPXSplitPage : ContentPage {

      const string SPLITFILE1 = "SplitFile1";
      const string SPLITFILE2 = "SplitFile2";
      const string SPLITDESTFILE = "SplitDestFile";

      const string SPLITSWITCHDELETE = "SplitSwitchDelete";
      const string SPLITSWITCHDELETEDEST = "SplitSwitchDeleteDest";

      object androidactivity;

      enum FileType {
         FileTrack,
         FilePoint,
         Destfilebasename
      }

      /// <summary>
      /// Art des gewünschten Split
      /// </summary>
      public enum SplitType {
         WithPoints,
         OnlyTracksegments
      }


      FileType ChooseFile = FileType.FileTrack;

      bool isBusy = false;

      SplitType Splittype;

      Color NormalButtonBackgroundColor;

      Color DisabledButtonBackgroundColor;

      StorageHelper sh;


      public GPXSplitPage() {
         InitializeComponent();
      }

      public GPXSplitPage(object androidactivity, StorageHelper sh, SplitType splittype) : this() {
         this.androidactivity = androidactivity;
         this.sh = sh;
         Splittype = splittype;
      }

      protected override void OnAppearing() {
         base.OnAppearing();

         if (!DesignMode.IsDesignModeEnabled) {
            // im Konstruktor fkt. noch nicht alle Oberflächenfunktionen (z.B. Scrollen)

            // fkt. nur mit android.permission.WRITE_USER_DICTIONARY und READ_USER_DICTIONARY !
            SetFilename(FileType.FileTrack, Application.Current.Properties.ContainsKey(SPLITFILE1) ? Application.Current.Properties[SPLITFILE1] as string : "");
            SetFilename(FileType.FilePoint, Application.Current.Properties.ContainsKey(SPLITFILE2) ? Application.Current.Properties[SPLITFILE2] as string : "");
            SetFilename(FileType.Destfilebasename, Application.Current.Properties.ContainsKey(SPLITDESTFILE) ? Application.Current.Properties[SPLITDESTFILE] as string : "");
            switchFileDelete.IsToggled = Application.Current.Properties.ContainsKey(SPLITSWITCHDELETE) ? (bool)Application.Current.Properties[SPLITSWITCHDELETE] : false;
            switchDelDest.IsToggled = Application.Current.Properties.ContainsKey(SPLITSWITCHDELETEDEST) ? (bool)Application.Current.Properties[SPLITSWITCHDELETEDEST] : false;

            if (Splittype == SplitType.OnlyTracksegments) {
               buttonFile2.IsVisible =
               ScrollViewFile2.IsVisible =
               horizontalLine2.IsVisible = false;
               Title = "Tracksegmente trennen";
            } else {
               Title = "Tracks an Punkten trennen";
            }

            NormalButtonBackgroundColor = buttonStart.BackgroundColor;
            DisabledButtonBackgroundColor = Color.FromHsla(NormalButtonBackgroundColor.Hue,
                                                           NormalButtonBackgroundColor.Saturation,
                                                           NormalButtonBackgroundColor.Luminosity * 1.1);
         }
      }

      private void ButtonTrackfile_Tapped(object sender, EventArgs e) {
         FileTapped(FileType.FileTrack);
      }

      private void ButtonPointfile_Tapped(object sender, EventArgs e) {
         FileTapped(FileType.FilePoint);
      }

      private void ButtonDestBasefilename_Tapped(object sender, EventArgs e) {
         FileTapped(FileType.Destfilebasename);
      }

      void FileTapped(FileType type) {
         string path = type == FileType.FileTrack ? labelFile1.Text :
                       type == FileType.FilePoint ? labelFile2.Text :
                                                    labelDestFile.Text;
         if (path != "")
            path = Path.GetDirectoryName(path);
         if (path == "")
            path = ChooseFilePage.LastChoosedPath;

         ChooseFilePage page = new ChooseFilePage() {
            AndroidActivity = androidactivity,
            Title = type == FileType.FileTrack ? "Trackdatei auswählen" :
                    type == FileType.FilePoint ? "Pointdatei auswählen" :
                                                 "Basiszieldatei auswählen",
            OnlyExistingFile = true,
            Path = path,
            Filename = Path.GetFileName(type == FileType.FileTrack ? labelFile1.Text:
                                        type == FileType.FilePoint ? labelFile1.Text :
                                                                     labelDestFile.Text),
         };
         page.ChooseFileReadyEvent += ChooseFileReadyEvent;
         ChooseFile = type; // für die Auswertung im Event-Handler (Dialog ist modal!)
         Navigation.PushAsync(page);
      }

      private void ChooseFileReadyEvent(object sender, FSofTUtils.Xamarin.Control.ChooseFile.ChoosePathAndFileEventArgs e) {
         if (e.OK) {
            ChooseFilePage.LastChoosedPath = e.Path;
            SetFilename(ChooseFile, e.Path, e.Filename);
         }
      }

      void SetFilename(FileType type, string fullfilename) {
         if (fullfilename == "")
            SetFilename(type, "", "");
         else
            SetFilename(type, Path.GetDirectoryName(fullfilename), Path.GetFileName(fullfilename));
      }

      void SetFilename(FileType type, string path, string shortfilename) {
         string fullfilename = path.Length > 0 && shortfilename.Length > 0 ? Path.Combine(path, shortfilename) : "";
         Element element4scroll = null;
         Label elementinfo = null;
         Label label = labelFile1;
         string dictkey = "";

         switch (type) {
            case FileType.FileTrack:
               if (fullfilename.Length > 0 &&
                   Path.GetExtension(fullfilename).ToLower() != ".gpx") {
#pragma warning disable 4014  // async-Aufruf ohne await
                  Helper.MessageBox(this, "Fehler", "Es sind nur GPX-Dateien erlaubt. ('" + fullfilename + "')");
#pragma warning restore 4014
                  return;
               }
               label = labelFile1;
               dictkey = SPLITFILE1;
               elementinfo = labelInfo1;
               element4scroll = PseudoListviewSrcFile1;
               break;

            case FileType.FilePoint:
               if (fullfilename.Length > 0 && Path.GetExtension(fullfilename).ToLower() != ".gpx") {
#pragma warning disable 4014  // async-Aufruf ohne await
                  Helper.MessageBox(this, "Fehler", "Es sind nur GPX-Dateien erlaubt. ('" + fullfilename + "')");
#pragma warning restore 4014
                  return;
               }
               label = labelFile2;
               dictkey = SPLITFILE2;
               elementinfo = labelInfo2;
               element4scroll = PseudoListviewSrcFile2;
               break;

            case FileType.Destfilebasename:
               if (fullfilename.Length > 0 && Path.GetExtension(fullfilename).ToLower() != ".gpx")
                  fullfilename += ".gpx";
               label = labelDestFile;
               dictkey = SPLITDESTFILE;
               element4scroll = labelDestFile;
               break;
         }

         label.Text = fullfilename;
         Application.Current.Properties[dictkey] = label.Text;
         Helper.SrollToEnd(element4scroll);

         if (elementinfo != null &&
             fullfilename.Length > 0) {
            string infotxt = "";
            try {
               long len = sh.GetFileAttributes(fullfilename, false, out bool canread, out bool canwrite, out DateTime lastmod);
               infotxt = string.Format("{0} Byte, {1} kByte, {2} MByte / {3}",
                                        len,
                                        Math.Round(len / 1024.0, 1),
                                        Math.Round(len / (1024.0 * 1024.0), 1),
                                        lastmod.ToString("G"));

            } catch (Exception) {
               infotxt = "";
#pragma warning disable 4014  // async-Aufruf ohne await
               Helper.MessageBox(this, "Fehler", "Infos für '" + fullfilename + "' können nicht ermittelt werden.");
#pragma warning restore 4014
            } finally {
               elementinfo.Text = infotxt;
            }
         }
      }

      private void SwitchFileDelete_Toggled(object sender, ToggledEventArgs e) {
         Application.Current.Properties[SPLITSWITCHDELETE] = (sender as Switch).IsToggled;
      }

      private void SwitchDelDest_Toggled(object sender, ToggledEventArgs e) {
         Application.Current.Properties[SPLITSWITCHDELETEDEST] = (sender as Switch).IsToggled;
      }

      async private void ButtonSplit_Tapped(object sender, EventArgs e) {
         buttonStart.IsEnabled = false;

         if (labelFile1.Text.Trim().Length == 0) {
            await Helper.MessageBox(this, "Info", "Die Trackdatei ist nicht angegeben.", "OK");
            buttonStart.IsEnabled = true;
            return;
         }
         if (Splittype == SplitType.WithPoints &&
             labelFile2.Text.Trim().Length == 0) {
            await Helper.MessageBox(this, "Info", "Die Pointdatei ist nicht angegeben.", "OK");
            buttonStart.IsEnabled = true;
            return;
         }
         if (labelDestFile.Text.Trim().Length == 0) {
            await Helper.MessageBox(this, "Info", "Der Basisname der Zieldateien ist nicht angegeben.", "OK");
            buttonStart.IsEnabled = true;
            return;
         }

         SetBusyStatus(true);

#pragma warning disable 4014  // async-Aufruf ohne await
         SplitFileAsync(labelFile1.Text.Trim(),
                        Splittype == SplitType.WithPoints ? labelFile2.Text.Trim() : "",
                        labelDestFile.Text.Trim(),
                        switchFileDelete.IsToggled,
                        switchDelDest.IsToggled,
                        SplitFileReady);
#pragma warning restore 4014
      }

      /// <summary>
      /// Callback-Funktion für die async-Funktion
      /// </summary>
      /// <param name="ok"></param>
      /// <param name="srcfilename"></param>
      /// <param name="errormsg"></param>
      async void SplitFileReady(int count, string srcfilename, string errormsg) {
         SetBusyStatus(false);

         if (errormsg == "")
            await Helper.MessageBox(this, "Auftrennen", "Die Dateie '" + srcfilename + "' wurde in " + count.ToString() + " Dateien aufgeteilt.");
         else
            await Helper.MessageBox(this, "Fehler beim Auftrennen", errormsg);
      }

      /// <summary>
      /// i.W. die Kapselung von <see cref="SplitFile"/> in eine async-Funktion
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
      private async Task SplitFileAsync(string filename1, string filename2, string destfilename, bool removeorgfiles, bool overwritedestfile, Action<int, string, string> ready) {
         string errormsg = "";
         int res = await Task.Run(() => {
            int count = 0; ;
            try {
               count = SplitFile(filename1, filename2, destfilename, removeorgfiles, overwritedestfile);
            } catch (Exception ex) {
               errormsg = ex.Message;
               count = 0;
            }
            return count;
         });
         ready(res, filename1, errormsg);
      }

      /// <summary>
      /// hier findet die eigentliche Arbeit statt
      /// </summary>
      /// <param name="trackfilename"></param>
      /// <param name="splitpointfilename"></param>
      /// <param name="destbasefilename"></param>
      /// <param name="removeorgfiles"></param>
      /// <param name="overwritedestfiles"></param>
      /// <returns></returns>
      int SplitFile(string trackfilename, string splitpointfilename, string destbasefilename, bool removeorgfiles, bool overwritedestfiles) {
         int extlength = Path.GetExtension(destbasefilename).Length; // wenn Ext. ex., dann Länge einschließlich '.' (wenn nur '.' dann Länge 0)
         if (extlength > 0)
            destbasefilename = destbasefilename.Substring(0, destbasefilename.Length - extlength); // nur Basisdateiname (ohne Ext.)

         GpxFileExt trackfile = new GpxFileExt(sh, trackfilename);
         return Splittype == SplitType.OnlyTracksegments ?
                           trackfile.SplitTracks(destbasefilename, removeorgfiles, overwritedestfiles, "GPXTool") :
                           trackfile.SplitTracks(splitpointfilename, destbasefilename, removeorgfiles, overwritedestfiles, "GPXTool");
      }

      void SetBusyStatus(bool isbusy) {
         isBusy = isbusy;
         buttonStart.IsEnabled = !isbusy;
         buttonStart.BackgroundColor = isBusy ? DisabledButtonBackgroundColor : NormalButtonBackgroundColor;
      }


      /// <summary>
      /// Event that is raised when the hardware back button is pressed. This event is not raised on iOS.
      /// </summary>
      /// <returns></returns>
      protected override bool OnBackButtonPressed() {
         if (!buttonStart.IsEnabled)
            return true; // Disable Backbutton

         return base.OnBackButtonPressed(); // Standard
      }

   }
}