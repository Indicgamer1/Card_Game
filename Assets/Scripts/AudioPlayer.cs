using UnityEngine;

// class to play audio
public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private new AudioClip[] audio;

    public static float Vol { get; } = 1;

    void Awake()
    {
        Instance = this;
    }
    public void PlayAudio(int _id)
    {
        audioSource.PlayOneShot(audio[_id]);
    }
    public void PlayAudio(int _id, float _vol)
    {
        audioSource.PlayOneShot(audio[_id], _vol);
    }

}
