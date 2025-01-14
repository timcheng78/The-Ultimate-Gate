using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private string playerSettingName = "player_setting.json";
    private string normalEnd = "normal";
    private bool useEncryption = false;
    private readonly string encryptionCodeWord = "word";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    public GameData Load()
    {
        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                // Load the seralizer data from the file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // optionally dencrypt the data
                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                // deserialize the data from Json back into the c# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            } 
            catch (Exception e)
            {
                Debug.LogError("Error occured then tring to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public PlayerSettingData LoadPlayerSetting()
    {
        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, playerSettingName);
        PlayerSettingData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                // Load the seralizer data from the file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // deserialize the data from Json back into the c# object
                loadedData = JsonUtility.FromJson<PlayerSettingData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured then tring to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(GameData data)
    {
        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            // create the directory the file be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the c# game data obejct into Json
            string dataToStore = JsonUtility.ToJson(data, true);

            // optionally encrypt the data
            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // write the serialized data to the file
            using FileStream stream = new (fullPath, FileMode.Create);
            using StreamWriter writer = new (stream);
            writer.Write(dataToStore);
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public void SavePlayerSetting(PlayerSettingData data)
    {
        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, playerSettingName);
        try
        {
            // create the directory the file be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the c# game data obejct into Json
            string dataToStore = JsonUtility.ToJson(data, true);

            // write the serialized data to the file
            using FileStream stream = new(fullPath, FileMode.Create);
            using StreamWriter writer = new(stream);
            writer.Write(dataToStore);
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public void SaveNormalEndFile()
    {
        // use Path.Combine to account for different OS's having different path separators
        string fullPath = Path.Combine(dataDirPath, normalEnd);
        try
        {
            // create the directory the file be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the c# game data obejct into Json
            string dataToStore = JsonUtility.ToJson("", true);

            // write the serialized data to the file
            using FileStream stream = new(fullPath, FileMode.Create);
            using StreamWriter writer = new(stream);
            writer.Write(dataToStore);
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public static bool CheckFileExist(string dataDirPath, string name)
    {
        string fullPath = Path.Combine(dataDirPath, name);
        if (File.Exists(fullPath))
        {
            return true;
        }
        return false;
    }

    public void Delete()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            // delete file
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                Debug.Log("File deleted: " + fullPath);
            }
            else
            {
                Debug.Log("File does not exist: " + fullPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to delete data to file: " + fullPath + "\n" + e);
        }
    }

    // the below is a simple implementation of XOR encryption
    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";
        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    }

}
