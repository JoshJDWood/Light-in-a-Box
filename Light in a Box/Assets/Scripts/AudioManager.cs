using UnityEngine.Audio;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private Sound[] sounds;
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sFXSlider;

    private void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.mute = s.mute;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.output;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return;
        }
        s.source.Play();
    }

    public void MuteSounds(bool mute)
    {
        foreach (Sound s in sounds)
        {
            s.source.mute = mute;
        }
    }

    public void MuteMusic(bool mute)
    {
        music.mute = mute;
    }

    public void SetVolumeMusic(float volume)
    {
        audioMixer.SetFloat("VolumeMusic", volume);
        MuteMusic(volume == musicSlider.minValue);
    }

    public void SetVolumeSFX(float volume)
    {
        audioMixer.SetFloat("VolumeSFX", volume);
        MuteSounds(volume == sFXSlider.minValue);        
    }

    public Vector2 GetAudioSettings()
    {
        return new Vector2(musicSlider.value, sFXSlider.value);
    }

    public void SetAudioSettings(Vector2 audioSettings)
    {
        StartCoroutine(DelayAudioSettings(audioSettings));
    }

    IEnumerator DelayAudioSettings(Vector2 audioSettings)
    {
        yield return new WaitForFixedUpdate();
        SetVolumeMusic(audioSettings.x);
        musicSlider.value = audioSettings.x;
        SetVolumeSFX(audioSettings.y);
        sFXSlider.value = audioSettings.y;
    }
}
