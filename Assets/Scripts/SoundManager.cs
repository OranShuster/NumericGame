using UnityEngine;

public class SoundManager : MonoBehaviour {

    //crincle sound found here: http://freesound.org/people/volivieri/sounds/37171/

    public AudioClip CrincleAudioClip;
    AudioSource _crincle;


    void Awake()
    {
        _crincle = AddAudio(CrincleAudioClip);
    }

    AudioSource AddAudio( AudioClip audioClip)
    {
        var audioSource = this.gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = audioClip;
        return audioSource;
    }

    public void PlayCrincle()
    {
        _crincle.Play();
    }
}
