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

    private bool audioBlendInprogress = false;

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

    //----------------------------------
    // AUDIO CROSSFADE
    //----------------------------------
    private IEnumerator CrossFadeAudio(AudioSource audioSource1, AudioSource audioSource2, float crossFadeTime, float audioSource2VolumeTarget)
    {
        string debugStart = "<b><color=red>ERROR:</color></b> ";
        int maxLoopCount = 1000;
        int loopCount = 0;
        float startAudioSource1Volume = audioSource1.volume;

        if (audioSource1 == null || audioSource2 == null) {
            Debug.Log(debugStart + transform.name + ".EngineControler.CrossFadeAudio recieved NULL value.\n*audioSource1=" + audioSource1.ToString() + "\n*audioSource2=" + audioSource2.ToString(), gameObject);
            yield return null;
        }
        else {
            audioBlendInprogress = true;

            audioSource2.volume = 0f;
            audioSource2.Play();

            while ((audioSource1.volume > 0f && audioSource2.volume < audioSource2VolumeTarget) && loopCount < maxLoopCount) {
                audioSource1.volume -= startAudioSource1Volume * Time.deltaTime / crossFadeTime;
                audioSource2.volume += audioSource2VolumeTarget * Time.deltaTime / crossFadeTime;
                loopCount++;
                yield return null;
            }

            if (loopCount < maxLoopCount) {
                audioSource1.Stop();
                audioSource1.volume = startAudioSource1Volume;
                audioBlendInprogress = false;
            }
            else {
                Debug.Log(debugStart + transform.name + ".EngineControler.CrossFadeAudio.loopCount reached max value.\nloopCount=" + loopCount + "\nmaxLoopCount=" + maxLoopCount, gameObject);
            }
        }
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
