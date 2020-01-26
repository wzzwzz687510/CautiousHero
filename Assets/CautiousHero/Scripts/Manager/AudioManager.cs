using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("BGM")]
        public AudioClip[] bgClips;
        public AudioClip loseClip;
        public AudioClip titleClip;
        public AudioClip peacefulClip;
        public AudioClip battleClip;
        public AudioClip bossClip;
        public AudioClip endClip;

        [Header("SE")]
        public AudioClip turnChangeClip;
        public AudioClip enterClip;
        public AudioClip meetClip;
        public AudioClip victoryClip;
        public AudioClip errorClip;

        [Header("Source")]
        public AudioSource musicSource;
        public AudioSource seSource;
        private bool sourceFlag;

        private float bgmVolume = 1;
        private float seVolume = 1;

        private void Awake()
        {
            if (!Instance)
                Instance = this;
        }

        private void Start()
        {
            //StartCoroutine(BlendIntroToLoop());
            PlayTitleClip();
        }

        public void SetBGMVolume(float value)
        {
            bgmVolume = value;
            musicSource.volume = bgmVolume;
        }

        public void SetSEVolume(float value)
        {
            seVolume = value;
            seSource.volume = seVolume;
        }

        public void PlaySEClip(AudioClip clip)
        {
            seSource.PlayOneShot(clip);
        }

        public void PlayErrorClip()
        {
            PlaySEClip(errorClip);
        }


        public void PlayEnterClip()
        {
            PlaySEClip(enterClip);
        }

        public void PlayMeetClip()
        {
            PlaySEClip(meetClip);
        }

        public void PlayVictoryClip()
        {
            PlaySEClip(victoryClip);
        }

        public void PlayTitleClip()
        {
            StartCoroutine(FadeToClip(titleClip));
        }

        public void PlayEndClip()
        {
            StartCoroutine(FadeToClip(endClip));
        }

        public void PlayPeacefulClip()
        {
            StartCoroutine(FadeToClip(peacefulClip));
        }

        public void PlayBattleClip()
        {
            StartCoroutine(FadeToClip(battleClip));
        }

        public void PlayBossClip()
        {
            StartCoroutine(FadeToClip(bossClip));
        }

        public void ChangeBGM(AudioClip clip, float delay)
        {
            StartCoroutine(FadeToClip(clip, delay));
        }

        public void Gameover()
        {
            musicSource.loop = false;
            StartCoroutine(FadeToClip(loseClip));
        }

        private IEnumerator FadeToClip(AudioClip clip, float delay=0)
        {
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(FadeAudio(musicSource, 0.2f, 0));
            musicSource.Stop();
            musicSource.volume = bgmVolume;
            musicSource.clip = clip;
            musicSource.Play();
        }

        IEnumerator FadeAudio(AudioSource source, float fadeTime, float sourceVolumeTarget)
        {
            float startAudioSource1Volume = source.volume;
            while (source.volume > sourceVolumeTarget) {
                source.volume -= startAudioSource1Volume * Time.deltaTime / fadeTime;
                yield return null;
            }
        }
    }
}

