#define USE_BINARY_FORMATTER
//Codigo de Eduardo
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

    public Transform m_Player;

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

    private void Start()
    {
        m_Player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            m_GameSaveData.m_PlayerPosition = m_Player.position;
            SaveOptionsSaveData();
            SaveGameSaveData();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadOptionsSaveData();
            LoadGameSaveData();
            m_Player.position = m_GameSaveData.m_PlayerPosition;
        }
    }

    private static void SaveFile(object thisData, string thisPath)
    {
        //We convert the OptionsSaveData object to a JSON string and encode it
        string json = Encode(JsonUtility.ToJson(thisData));
        //We set the path to the persistent data path, which is a folder that Unity creates to store data
        string path = Path.Combine(Application.persistentDataPath, thisPath);

#if USE_BINARY_FORMATTER
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, json);
        stream.Close();
#else        
        File.WriteAllText(path, json);
#endif
        Debug.Log("File Saved");
    }

    private static T LoadFile<T>(string fileName) where T : new() 
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if(File.Exists(path))
        {
#if USE_BINARY_FORMATTER
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path,FileMode.Open);
            string data = formatter.Deserialize(stream) as string;
            stream.Close();
            string json = Decode(data);
#else
            //We read the file and decode it
            string json = Decode(File.ReadAllText(path));
#endif
            // We convert the JSON string to an OptionsSaveData object
            return JsonUtility.FromJson<T>(json);
        }
        else
        {
            Debug.Log("File does not exist");
            return new T();
        }

    }

    public static void SaveOptionsSaveData()
    {
        SaveFile(m_Instance.m_OptionsSaveData,m_OptionsSaveDataPath);
    }

    public static void SaveGameSaveData()
    {
        SaveFile(m_Instance.m_GameSaveData, m_GameSaveDataPath);
    }

    public static void LoadOptionsSaveData()
    {
        m_Instance.m_OptionsSaveData = LoadFile<OptionsSaveData>(m_OptionsSaveDataPath);
    }

    public static void LoadGameSaveData()
    {
        m_Instance.m_GameSaveData = LoadFile<GameSaveData>(m_GameSaveDataPath);
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

