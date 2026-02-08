using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class AudioPlayer : MonoBehaviour
    {
        [Inject] private SoundLibrary _soundLibrary;

        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource[] sfxAudioSources;

        public void PlayMusic(string key)
        {
            var entry = _soundLibrary.Get(key);
            musicAudioSource.DOFade(0, 1f).OnComplete(() =>
            {
                musicAudioSource.clip = entry.RandomClip;
                musicAudioSource.volume = entry.volume;
                musicAudioSource.Play();
                musicAudioSource.DOFade(entry.volume, 1f);
            });
        }

        public void StopMusic(bool withFade = false)
        {
            if (!withFade)
            {
                musicAudioSource.Stop();
                return;
            }

            musicAudioSource.DOFade(0, 1f).OnComplete(() => musicAudioSource.Stop());
        }

        public void PlaySfx(string key)
        {
            var entry = _soundLibrary.Get(key);

            foreach (var source in sfxAudioSources)
            {
                if (source.isPlaying) continue;
                source.clip = entry.RandomClip;
                source.volume = entry.volume;
                source.loop = false;
                source.Play();
                return;
            }

            sfxAudioSources[0].clip = entry.RandomClip;
            sfxAudioSources[0].volume = entry.volume;
            sfxAudioSources[0].Play();
        }

        public void StopSfx(string key)
        {
            var entry = _soundLibrary.Get(key);
            foreach (var source in sfxAudioSources)
            {
                if (System.Array.IndexOf(entry.clips, source.clip) < 0) continue;
                source.Stop();
            }
        }
    }
}
