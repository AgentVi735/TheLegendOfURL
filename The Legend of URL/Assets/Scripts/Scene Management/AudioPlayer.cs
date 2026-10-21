using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioSource _persistentAudioSource;

    [SerializeField] private AudioClip[] _clips;

    public void Initialise(float volume)
    {
        if (_audioSource != null)
            _audioSource.volume = volume;
        if (_persistentAudioSource != null)
            _persistentAudioSource.volume = volume;
    }

    public void TryPlayPersistentClip(int index, bool loop = false)
    {
        if (index >= _clips.Length)
        {
            Debug.LogError($"{index} is higher than amount of audioClips in {gameObject.name}");
            return;
        }
        
        AudioClip clip = _clips[index];
        if (clip == null)
        {
            Debug.LogError($"Audio clip in {gameObject.name} could not be loaded with index {index}");
            return;
        }
        
        if (_persistentAudioSource == null)
        {
            Debug.LogError($"Persistent Audio Source in AudioPlayer on {gameObject.name}");
            return;
        }

        if (_persistentAudioSource.isPlaying)
        {
            if (_persistentAudioSource.clip == clip)
                return;
            
            _persistentAudioSource.Stop();
        }
        
        _persistentAudioSource.clip = clip;
        _persistentAudioSource.loop = loop;
        _persistentAudioSource.Play();
    }
    
    public void PlayClip(int index, bool isPersistent = false, bool loop = false)
    {
        if (index >= _clips.Length)
        {
            Debug.LogError($"{index} is higher than amount of audioClips in {gameObject.name}");
            return;
        }
        
        AudioClip clip = _clips[index];
        if (clip == null)
        {
            Debug.LogError($"Audio clip in {gameObject.name} could not be loaded with index {index}");
            return;
        }
        
        if (isPersistent)
        {
            if (_persistentAudioSource == null)
            {
                Debug.LogError($"Persistent Audio Source in AudioPlayer on {gameObject.name}");
                return;
            }
            
            if (_persistentAudioSource.isPlaying)
                _persistentAudioSource.Stop();
            _persistentAudioSource.clip = clip;
            _persistentAudioSource.loop = loop;
            _persistentAudioSource.Play();
        }
        else
        {
            if (_audioSource == null)
            {
                Debug.LogError($"Audio Source in AudioPlayer on {gameObject.name}");
                return;
            }
            
            _audioSource.PlayOneShot(clip);
        }
    }

    public void StopPersistentAudio()
    {
        if (_persistentAudioSource.isPlaying)
            _persistentAudioSource.Stop();
    }
}