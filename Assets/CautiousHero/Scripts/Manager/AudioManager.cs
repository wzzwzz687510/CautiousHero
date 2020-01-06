using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioClip[] bgClips;
    public AudioClip loseClip;

    public AudioSource source0;
    public AudioSource source1;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }

    private void Start()
    {
        StartCoroutine(BlendIntroToLoop());
    }

    IEnumerator BlendIntroToLoop()
    {
        source0.clip = bgClips[0];
        source0.loop = false;
        source0.Play();
        while (source0.isPlaying) {
            yield return null;
        }

        source0.clip = bgClips[1];
        source0.loop = true;
        source0.Play();
    }

    public void Gameover()
    {
        StartCoroutine(FadeAudio(source0, 0.2f, 0));
        source1.Play();
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
