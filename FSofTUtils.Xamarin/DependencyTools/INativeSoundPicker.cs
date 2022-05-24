using System.Collections.Generic;

namespace FSofTUtils.Xamarin.DependencyTools {
   public interface INativeSoundPicker {

      List<NativeSoundData> GetNativeSoundData(bool intern,
                                               bool isalarm,
                                               bool isnotification,
                                               bool isringtone,
                                               bool ismusic);

      void PlayExclusiveNativeSound(NativeSoundData nativeSoundData, float volume, bool looping);

      void PlayExclusiveNativeSound(string uri, float volume, bool looping);

      void StopExclusiveNativeSound();

   }
}
