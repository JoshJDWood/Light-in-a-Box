using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    public static int unsolvedVal = 1000;
    public static int solvedEasy = 999;

    public static void SaveFile(int[] solvedValues, bool hardMode, string fileName)
    {
        SaveData data = new SaveData();
        data.hardMode = hardMode;
        data.solvedValues = solvedValues;

        string contents = JsonUtility.ToJson(data);
        string Path = Application.persistentDataPath + "/" + fileName;

        File.WriteAllText(Path, contents);
        Debug.Log("filepath is: " + Path);
    }

    public static SaveData LoadFile(string fileName)
    {
        string path = Application.persistentDataPath + "/" + fileName;
        if (File.Exists(path))
        {
            string contents = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(contents);
        }
        Debug.Log("save file not found");
        return null;
    }
    
    [System.Serializable]
    public class SaveData
    {
        public bool hardMode;
        public int[] solvedValues;
    }
}
