using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager<T>
{
    private string filePath;

    public DataManager(string fileName)
    {
        string directory = Path.Combine(Application.persistentDataPath, "SaveData");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        filePath = Path.Combine(directory, fileName);
    }

    public void SaveData(T data)
    {
        try
        {
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
            Debug.Log($"Data saved to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

    public T LoadData()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"File not found: {filePath}");
            return Activator.CreateInstance<T>();
        }

        try
        {
            string jsonData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(jsonData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data: {e.Message}");
            return Activator.CreateInstance<T>();
        }
    }
}
