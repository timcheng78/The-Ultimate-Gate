using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RecordData
{
    public SerializableDictionary<int, SerialzableList<string>> records;
    public List<string> usedSentences;
    public List<string> hintSentences;

    public List<List<string>> RecordsToList()
    {
        List<List<string>> result = new();
        for (int i = 0, len = records.Count; i < len; ++i)
        {
            result.Add(new());
            foreach (string sentences in records[i])
            {
                result[i].Add(sentences);
            }
        }

        return result;
    }

    public SerializableDictionary<int, SerialzableList<string>> ListToRecords(List<List<string>> list)
    {
        SerializableDictionary<int, SerialzableList<string>> tRecords = new();
        for (int i = 0, len = list.Count; i < len; ++i)
        {
            tRecords[i] = new();
            foreach (string sentence in list[i])
            {
                tRecords[i].Add(sentence);
            }
        }
        return tRecords;
    }
}
