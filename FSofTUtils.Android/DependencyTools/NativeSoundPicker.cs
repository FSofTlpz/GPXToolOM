using Android.Content;
using Android.Media;
using Android.Provider;
using FSofTUtils.Xamarin;
using System;
using System.Collections.Generic;

[assembly: Xamarin.Forms.Dependency(typeof(FSofTUtils.Android.DependencyTools.NativeSoundPicker))]
namespace FSofTUtils.Android.DependencyTools {
   public class NativeSoundPicker : FSofTUtils.Xamarin.DependencyTools.INativeSoundPicker {

      public List<NativeSoundData> GetNativeSoundData(bool intern,
                                                      bool isalarm,
                                                      bool isnotification,
                                                      bool isringtone,
                                                      bool ismusic) {
         List<NativeSoundData> lst = new List<NativeSoundData>();
         ContentResolver contentResolver = global::Android.App.Application.Context.ContentResolver;
         string where = "";
         if (isalarm)
            where = (where.Length == 0 ? "" : " or ") + MediaStore.Audio.Media.InterfaceConsts.IsAlarm + ">0";
         if (isnotification)
            where = (where.Length == 0 ? "" : " or ") + MediaStore.Audio.Media.InterfaceConsts.IsNotification + ">0";
         if (isringtone)
            where = (where.Length == 0 ? "" : " or ") + MediaStore.Audio.Media.InterfaceConsts.IsRingtone + ">0";
         if (ismusic)
            where = (where.Length == 0 ? "" : " or ") + MediaStore.Audio.Media.InterfaceConsts.IsMusic + ">0";
         global::Android.Database.ICursor cursor = contentResolver.Query(intern ?
                                                                     MediaStore.Audio.Media.InternalContentUri :
                                                                     MediaStore.Audio.Media.ExternalContentUri,
                                                                 new string[] {
                                                                    MediaStore.Audio.Media.InterfaceConsts.Id,
                                                                    MediaStore.Audio.Media.InterfaceConsts.Title,
                                                                    MediaStore.Audio.Media.InterfaceConsts.Data,
                                                                 },
                                                                 where,
                                                                 null,
                                                                 null);
         if (cursor != null && cursor.Count > 0) {
            cursor.MoveToFirst();
            do {
               string id = "";
               string name = "";
               string data = "";
               for (int i = 0; i < cursor.ColumnCount; i++) {
                  string colname = cursor.GetColumnName(i);
                  if (colname == MediaStore.Audio.Media.InterfaceConsts.Id)
                     id = cursor.GetString(i);
                  else if (colname == MediaStore.Audio.Media.InterfaceConsts.Title)
                     name = cursor.GetString(i);
                  else if (colname == MediaStore.Audio.Media.InterfaceConsts.Data)
                     data = cursor.GetString(i);
               }
               lst.Add(new NativeSoundData(intern, id, name, data));
            }
            while (!cursor.IsAfterLast && cursor.MoveToNext());
            cursor.Close();
         }
         return lst;
      }

      static Ringtone exclusiveRingtone = null;

      public void PlayExclusiveNativeSound(NativeSoundData nativeSoundData, float volume = 1.0F, bool looping = true) {
         StopExclusiveNativeSound();
         global::Android.Net.Uri uri = nativeSoundData.ID == "" ?
                                             uri = global::Android.Net.Uri.Parse(nativeSoundData.Data) :
                                             uri = global::Android.Net.Uri.Parse((nativeSoundData.Intern ?
                                                                                                MediaStore.Audio.Media.InternalContentUri :
                                                                                                MediaStore.Audio.Media.ExternalContentUri).ToString() + "/" + nativeSoundData.ID);
         PlayExclusiveNativeSound(uri.SchemeSpecificPart, volume, looping);
      }

      public void PlayExclusiveNativeSound(string uri, float volume = 1.0F, bool looping = true) {
         StopExclusiveNativeSound();
         Ringtone rt = RingtoneManager.GetRingtone(global::Android.App.Application.Context, global::Android.Net.Uri.Parse(uri));
         if (rt != null) {
            rt.AudioAttributes = new AudioAttributes.Builder()
                                     .SetUsage(AudioUsageKind.Alarm)
                                     .SetContentType(AudioContentType.Music)
                                     .Build();
            rt.Looping = looping;
            rt.Volume = Math.Max(0F, Math.Min(volume, 1F));
            rt.Play();
            exclusiveRingtone = rt;
         }
      }

      public void StopExclusiveNativeSound() {
         if (exclusiveRingtone != null && exclusiveRingtone.IsPlaying)
            exclusiveRingtone.Stop();
         if (exclusiveRingtone != null)
            exclusiveRingtone.Dispose();
         exclusiveRingtone = null;
      }

   }
}