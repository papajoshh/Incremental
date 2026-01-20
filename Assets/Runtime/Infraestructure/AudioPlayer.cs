using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Runtime.Infrastructure
{
    public class AudioPlayer: MonoBehaviour
    {
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource[] sfxAudioSources;

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void PlayMusic(AudioClip clip, float volume = 0.2f, Vector3 pos = new Vector3())
        {
            if (clip == null)
                return;

            if (musicAudioSource)
            {
                musicAudioSource.DOFade(0, 1f).OnComplete(() =>
                {
                    musicAudioSource.transform.position = pos;
                    musicAudioSource.spatialBlend = pos != Vector3.zero ? 1 : 0;
                    musicAudioSource.clip = clip;
                    musicAudioSource.volume = volume;
                    musicAudioSource.Play();
                    musicAudioSource.DOFade(volume, 1f);
                });
            }
        }

        public void StopMusic(bool withFade = false)
        {
            if (withFade)
            {
                musicAudioSource.DOFade(0, 1f).OnComplete(() => {musicAudioSource.Stop();});
            }
            else
            {
                musicAudioSource.Stop();
            }
        }
        public void PlaySFX(AudioClip clip, float volume = 0.2f, bool loop = false, Vector3 pos = new Vector3())
        {
            if (clip == null)
                return;
            
            var hasBeenPlayed = false;
            foreach (var sfxAudioSource in sfxAudioSources)
            {
                if (sfxAudioSource.isPlaying) continue;
                sfxAudioSource.transform.position = pos;
                sfxAudioSource.spatialBlend = pos != Vector3.zero ? 1 : 0;
                sfxAudioSource.clip = clip;
                sfxAudioSource.volume = volume;
                sfxAudioSource.loop = loop;
                sfxAudioSource.Play();
                hasBeenPlayed = true;
                return;
            }

            if (hasBeenPlayed) return;
            sfxAudioSources[0].clip = clip;
            sfxAudioSources[0].Play();
        }

        public void StopSFX(AudioClip clip, bool withFade = false)
        {
            foreach (var sfxAudioSource in sfxAudioSources)
            {
                if (sfxAudioSource.clip != clip) continue;
                if (withFade)
                {
                    sfxAudioSource.DOFade(0, 1f).OnComplete(() => {sfxAudioSource.Stop();});
                }
                else
                {
                    sfxAudioSource.Stop();
                }
                
            }
        }
        
        public void PlaySfxWithDelay(AudioClip clip, float volume = 0.2f, bool loop = false, float delay = 0f, Vector3 pos = new Vector3())
        {
            StartCoroutine(WaitToPlaySfx(clip, volume, loop, delay, pos));
        }
        
        private IEnumerator WaitToPlaySfx(AudioClip clip, float volume, bool loop, float delay, Vector3 pos = new Vector3())
        {
            yield return new WaitForSeconds(delay);
            PlaySFX(clip, volume, loop, pos);
        }
    }
}