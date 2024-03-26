using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager m_Instance { get; private set; }

    public OptionsSaveData m_OptionsSaveData;
    public GameSaveData m_GameSaveData;

    private static string m_OptionsSaveDataPath = "OptionsSaveData";
    private static string m_GameSaveDataPath = "GameSaveData";

    private void Awake()
    {
        // check if an instance of this singleton does not exist
        if(m_Instance == null)
        {
            // Assign this intance to the static varible
            m_Instance = this;
            // Optionally, make this object persistent across scenes
            DontDestroyOnLoad(gameObject);
        }
        else if(m_Instance!=this) 
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveOptionsSaveData();
            SaveGameSaveData();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadFile();
        }
    }

    private static void SaveFile(object thisData, string thisPath)
    {
        string json = Encode(JsonUtility.ToJson(thisData));
        //We set the path to the persistent data path, which is a folder that Unity creates to store data
        string path = Path.Combine(Application.persistentDataPath, thisPath);
        
        File.WriteAllText(path, json);
        Debug.Log("File Saved");
    }

    private static void LoadFile() 
    {
        string path = Path.Combine(Application.persistentDataPath, m_OptionsSaveDataPath);
        
        //We read the file and decode it
        string json = Decode(File.ReadAllText(path));

        m_OptionsSaveData = JsonUtility.FromJson<OptionsSaveData>(json);
    }

    public static void SaveOptionsSaveData()
    {
        SaveFile(m_Instance.m_OptionsSaveData,m_OptionsSaveDataPath);
    }

    public static void SaveGameSaveData()
    {
        SaveFile(m_Instance.m_GameSaveData, m_GameSaveDataPath);
    }

    private static string Encode(string toEncode)
    {
        byte[] bytesToEncode = System.Text.Encoding.UTF8.GetBytes(toEncode);
        string encodedText = System.Convert.ToBase64String(bytesToEncode);
        return encodedText;
    }

    private static string Decode(string toDecode)
    {
        byte[] bytesToDecode = System.Convert.FromBase64String(toDecode);
        string decodedText = System.Text.Encoding.UTF8.GetString(bytesToDecode);
        return decodedText;
    }


}

[Serializable]
public class OptionsSaveData
{
    [SerializeField]
    public float m_MasterVolume;
    [SerializeField]
    public bool m_IsFullScreen;
}

[Serializable]
public class GameSaveData
{
    [SerializeField]
    public Vector3 m_PlayerPosition;
    [SerializeField]
    public int m_Score;
}

