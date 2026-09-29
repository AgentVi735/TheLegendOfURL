using System;
using System.IO;
using Heathen.Ogham;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    
    public SaveData SaveData => _dataObject;
    [SerializeField] private SaveData _dataObject;

    [SerializeField] private string _savePath;
    [SerializeField] private string _storySavePath;

    public bool CanSave;

    [Header("References")]
    [SerializeField] private PlayerController _playerController;

    public void Initialise()
    {
        if (Instance != null)
        {
            Debug.LogWarning("An instance of SaveManager already exists");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_dataObject == null)
        {
            Debug.LogError("Data Object is empty. Please create a SaveData scriptable object");
            return;
        }
        
        if (File.Exists(Application.persistentDataPath + _savePath))
            LoadSave();
        else
            CreateSave();

        CanSave = true;
    }
    
    public void Save()
    {
        if (Instance != this || !CanSave)
            return;
        
        SaveData.doesDataExist = true;
        
        _playerController.SaveData();
        SaveData.SaveEnemyIDsKilled();
        
        string json = JsonUtility.ToJson(SaveData);
        try
        {
            File.WriteAllText(Application.persistentDataPath + _savePath, json);
        }
        catch
        {
            Debug.LogError("Save data could not be saved");
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Successfully saved data");
#endif
    }

    private void LoadSave()
    {
        string json = File.ReadAllText(Application.persistentDataPath + _savePath);

        try
        {
            JsonUtility.FromJsonOverwrite(json, _dataObject);
        }
        catch (Exception e)
        {
            Debug.LogError($"Save file failed to load with exception: {e}");
            CreateSave();
#if UNITY_EDITOR
            return;
#endif
        }
        
        SaveData.InitialiseData();
        
#if UNITY_EDITOR
        Debug.Log("Successfully loaded data");
#endif
    }
    
    private void CreateSave()
    {
#if UNITY_EDITOR
        Debug.Log("Creating new save data");
#endif
        SaveData.ResetData();
        SaveStory();
    }

    private void OnDestroy()
    {
        Save();
    }

    public void SaveStory()
    {
        if (Instance != this || !CanSave)
            return;

        OghamSaveState saveState = Storyteller.Snapshot();
        if (saveState == null)
        {
            Debug.Log("Story save state is empty");
            return;
        }
        string json = JsonUtility.ToJson(saveState);
        try
        {
            File.WriteAllText(Application.persistentDataPath + _storySavePath, json);
        }
        catch
        {
            Debug.LogError("Story save data could not be saved");
            return;
        }
#if UNITY_EDITOR
        Debug.Log("Successfully saved story data");
#endif
    }

    public void LoadStory()
    {
        if (!File.Exists(Application.persistentDataPath + _storySavePath))
        {
            Debug.Log("No story save file found, creating new save file...");
            SaveStory();
            return;
        }
        string json = File.ReadAllText(Application.persistentDataPath + _storySavePath);

        try
        {
            OghamSaveState saveState = JsonUtility.FromJson<OghamSaveState>(json);
            Storyteller.Restore(saveState);
        }
        catch (Exception e)
        {
            Debug.LogError($"Story save file failed to load with exception: {e}");
            CreateSave();
#if UNITY_EDITOR
            return;
#endif
        }
        
        Storyteller.Resume();
        
#if UNITY_EDITOR
        Debug.Log("Successfully loaded story data");
#endif
    }
}