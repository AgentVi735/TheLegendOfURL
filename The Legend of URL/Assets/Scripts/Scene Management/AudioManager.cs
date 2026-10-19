using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private AudioPlayer[] _playersInScene;
    [SerializeField] private AudioSource _musicSource;

    public void Initialise()
    {
        _musicSource.volume = SaveManager.Instance.SaveData.musicVolume;
        SceneController.s_OnSceneLoaded += OnChangeScene;
    }

    private void OnChangeScene(Scene loadedScene, LoadSceneMode loadMode)
    {
        _playersInScene = FindObjectsByType<AudioPlayer>(FindObjectsInactive.Include);
        float sfxVolume = SaveManager.Instance.SaveData.sfxVolume;
        foreach (AudioPlayer audioPlayer in _playersInScene)
            audioPlayer.Initialise(sfxVolume);
    }

    public void UpdateMusicVolume()
    {
        _musicSource.volume = SaveManager.Instance.SaveData.musicVolume;
    }

    public void UpdateSfxVolume()
    {
        if (_playersInScene == null || _playersInScene.Length == 0) return;
        float sfxVolume = SaveManager.Instance.SaveData.sfxVolume;
        foreach (AudioPlayer audioPlayer in _playersInScene)
            audioPlayer.Initialise(sfxVolume);
    }
}