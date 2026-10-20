using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private AudioPlayer[] _playersInScene;
    [SerializeField] private AudioPlayer _musicSource;
    [SerializeField] private AudioPlayer _uiSource;
    private bool _isPlayingBossMusic;
    
    public bool IsInitialised { get; private set; }

    public void Initialise()
    {
        IsInitialised = true;
        _musicSource.Initialise(SaveManager.Instance.SaveData.musicVolume);
        SceneController.s_OnSceneLoaded += OnChangeScene;
        PlayMainMusic();
    }

    private void OnChangeScene(Scene loadedScene, LoadSceneMode loadMode)
    {
        _playersInScene = FindObjectsByType<AudioPlayer>(FindObjectsInactive.Include);
        float sfxVolume = SaveManager.Instance.SaveData.sfxVolume;
        foreach (AudioPlayer audioPlayer in _playersInScene)
        {
            if (audioPlayer == _musicSource) continue;
            audioPlayer.Initialise(sfxVolume);
        }
    }

    public void UpdateMusicVolume()
    {
        _musicSource.Initialise(SaveManager.Instance.SaveData.musicVolume);
    }

    public void UpdateSfxVolume()
    {
        if (_playersInScene == null || _playersInScene.Length == 0) return;
        float sfxVolume = SaveManager.Instance.SaveData.sfxVolume;
        foreach (AudioPlayer audioPlayer in _playersInScene)
        {
            if (audioPlayer == _musicSource) continue;
            audioPlayer.Initialise(sfxVolume);
        }
    }

    public void GoBackToMainMusic()
    {
        Debug.Log($"is playing boss music? {_isPlayingBossMusic}");
        if (_isPlayingBossMusic)
            PlayMainMusic();
    }

    private void PlayMainMusic()
    {
        _musicSource.PlayClip(0, true, true);
        _isPlayingBossMusic = false;
    }

    public void PlayBossMusic()
    {
        _musicSource.PlayClip(1, true, true);
        _isPlayingBossMusic = true;
    }

    public void PlayUIClickSfx() => _uiSource.PlayClip(0);
}