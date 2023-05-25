using Monocle;

namespace TowerfallAi.Core {
  public static class Sound {
    private static bool soundStoped;
    private static float musicVolume;

    public static void StopSound() {
      if (soundStoped) return;
      musicVolume = Music.MasterVolume;
      Music.MasterVolume = 0;
      soundStoped = true;
    }

    public static void ResumeSound() {
      if (!soundStoped) return;
      Music.MasterVolume = musicVolume;
      soundStoped = false;
    }
  }
}
