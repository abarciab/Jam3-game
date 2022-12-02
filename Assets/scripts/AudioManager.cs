using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this is an audiomanage I've used for a few projects now, and it's pretty straightforward. add clips to this script in the inspector (don't forget to set their ID), then call audioManager.instance.PlayGlobal(), .PlayHere(), or .PlayMusic()
//it's not the most elegant thing ever, but it should work just fine
public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public int ID;
        public AudioClip clip;
        [Range(0, 1)]
        public float volume = 1;
        [Range(-3, 3)]
        public float pitch = 1;     //if your sound isn't playing, make sure to check that the pitch isn't 0. if it is, it'll be muted
        public bool looping;
    }
    public List<Sound> sounds = new List<Sound>();

    public static AudioManager instance;

    public AudioSource music;
    public AudioSource global;
    int currentPriority = -1;

    private void Awake() {
        instance = this;
    }

    private Sound getSoundFromID(int soundID) {
        for (int i = 0; i < sounds.Count; i++) {
            if (sounds[i].ID == soundID) {
                return sounds[i];
            }
        }
        Debug.LogError("Tried to get sound with invalid ID. ID: " + soundID);
        return null;
    }

    public void SetMusicVolume(float vol, bool playIfPaused = true)     //set volume on music audiosource
    {
        music.volume = vol;
        if (playIfPaused && !music.isPlaying) {
            music.Play();
        }
    }

    public void StopSoundHere(int soundID, AudioSource source)      //stop given sound at given audiosource
    {
        Sound toPlay = getSoundFromID(soundID);

        if (source.clip == toPlay.clip && source.isPlaying) {
            source.Stop();
        }
    }

    public void PlayGlobal(int soundID, float volume = -1, bool restart = false, int _priority = -1)      //play given sound from global audiosource. priority is optional, but if you want to make sure one sound plays in its entirety, pass a priority value. see an example in enemy.cs
    {

        PlayHere(soundID, global, volume, restart, priority: _priority);
    }

    public void PlayMusic(int soundID, float volume = -1, bool restart = false)      //play given sound from global audiosource
    {
        //Sound toPlay = getSoundFromID(soundID);
        //if (music.clip == toPlay.clip && music.isPlaying)
        PlayHere(soundID, music, volume, restart);
    }

    public void PlayHere(int soundID, AudioSource source, float volume = -1, bool restart = false, int priority = -1)      //play given sound from given audiosource
    {
        if (priority != -1 && priority < currentPriority && source.isPlaying) {
            return;
        }
        currentPriority = priority;
        

        Sound toPlay = getSoundFromID(soundID);

        source.volume = volume == -1 ? toPlay.volume : volume;
        source.pitch = toPlay.pitch;
        source.loop = toPlay.looping;

        if (source.clip != toPlay.clip || !source.isPlaying || restart == true) {
            source.clip = toPlay.clip;
            source.Play();
        }
    }
}
