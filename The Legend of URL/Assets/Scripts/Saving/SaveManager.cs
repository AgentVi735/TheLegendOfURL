using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    
    public SaveData SaveData => _dataObject;
    [SerializeField] private SaveData _dataObject;

    [SerializeField] private string savePath;

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
        
        if (File.Exists(Application.persistentDataPath + savePath))
            LoadSave();
        else
            CreateSave();
    }

    public void Save()
    {
        SaveData.doesDataExist = true;
        
        _playerController.SaveData();
        SaveData.SaveEnemyIDsKilled();
        
        string json = JsonUtility.ToJson(SaveData);
        try
        {
            File.WriteAllText(Application.persistentDataPath + savePath, json);
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
        string json = File.ReadAllText(Application.persistentDataPath + savePath);

        try
        {
            // _dataObject.LoadData(JsonUtility.FromJson<SaveData>(json));
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
    }

    private void OnDestroy()
    {
        Save();
    }
}