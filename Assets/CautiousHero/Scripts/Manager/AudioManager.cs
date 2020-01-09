using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wing.RPGSystem
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public AudioClip[] bgClips;
        public AudioClip loseClip;
        public AudioClip titleClip;
        public AudioClip peacefulClip;
        public AudioClip battleClip;

        public AudioSource[] source;
        private bool sourceFlag;

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

        public void PlayTitleClip()
        {
            StartCoroutine(FadeToClip(titleClip));
        }

        public void PlayPeacefulClip()
        {
            StartCoroutine(FadeToClip(peacefulClip));
        }

        public void PlayBattleClip()
        {
            StartCoroutine(FadeToClip(battleClip));
        }

        private IEnumerator FadeToClip(AudioClip clip)
        {
            yield return StartCoroutine(FadeAudio(source[0], 0.2f, 0));
            source[0].Stop();
            source[0].volume = 1;
            source[0].clip = clip;
            source[0].Play();
        }

        public void Gameover()
        {
            StartCoroutine(FadeAudio(source[sourceFlag ? 1 : 0], 0.2f, 0));
            source[sourceFlag ? 0 : 1].Play();
            sourceFlag = !sourceFlag;
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

