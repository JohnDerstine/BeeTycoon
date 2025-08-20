using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveSystem
{
    private static SaveData _saveData = new SaveData();

    [System.Serializable]
    public struct SaveData
    {
        public PlayerSaveData PlayerData;
        public MapSaveData MapData;
        public MarketSaveData MarketData;
        public GameSaveData GameData;
    }

    public static string SaveFileName()
    {
        string _saveFile = Application.persistentDataPath + "/save" + ".save";
        return _saveFile;
    }

    public static void Save()
    {
        HandleSaveData();

        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(_saveData, true));
    }

    private static void HandleSaveData()
    {
        GameObject.Find("GameController").GetComponent<GameController>().Save(ref _saveData.GameData);
        GameObject.Find("PlayerController").GetComponent<PlayerController>().Save(ref _saveData.PlayerData);
        GameObject.Find("MapLoader").GetComponent<MapLoader>().Save(ref _saveData.MapData);
        GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>().Save(ref _saveData.MarketData);
    }

    public static void Load()
    {
        string saveContent = File.ReadAllText(SaveFileName());

        _saveData = JsonUtility.FromJson<SaveData>(saveContent);
        HandleLoadData();
    }

    private static void HandleLoadData()
    {
        GameObject.Find("GameController").GetComponent<GameController>().Load(_saveData.GameData);
        GameObject.Find("MapLoader").GetComponent<MapLoader>().Load(_saveData.MapData);
        GameObject.Find("PlayerController").GetComponent<PlayerController>().Load(_saveData.PlayerData);
        GameObject.Find("HoneyMarket").GetComponent<HoneyMarket>().Load(_saveData.MarketData);
    }

    public static bool CheckSaveFile()
    {
        return File.Exists(SaveFileName());
    }

    public static void DeleteSave()
    {
        File.Delete(SaveFileName());
    }
}
