using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Save Data")]
public class SaveData : ScriptableObject
{
    [Header("Generic")]
    public bool doesDataExist;
    public int currentSceneIdx = 1;
    
    [Header("Player")]
    public Vector3 playerPosition;
    public Vector3 playerRotation;
    public short health = 100;
    
    [Header("World")]
    [SerializeField] private string[] enemyIdsKilledArray;
    private Dictionary<string, bool> enemyIdsKilled;

    public void InitialiseData()
    {
        enemyIdsKilled = new Dictionary<string, bool>();
        foreach (string id in enemyIdsKilledArray)
            enemyIdsKilled.TryAdd(id, true);
    }

    public bool GetEnemyIDKilled(string id)
    {
        return doesDataExist && enemyIdsKilled.GetValueOrDefault(id, false);
    }

    public void SetEnemyIDKilled(string id, bool value)
    {
        if (enemyIdsKilled.TryAdd(id, value)) return;
        enemyIdsKilled[id] = value;
    }

    public void SaveEnemyIDsKilled()
    {
        enemyIdsKilledArray = enemyIdsKilled.Select(enemy => enemy.Key).ToArray();
    }
    
    public void ResetData()
    {
        doesDataExist = false;
        currentSceneIdx = 1;
        playerPosition = Vector3.zero;
        playerRotation = Vector3.zero;
        health = 100;
        enemyIdsKilledArray = Array.Empty<string>();
        InitialiseData();
    }
}