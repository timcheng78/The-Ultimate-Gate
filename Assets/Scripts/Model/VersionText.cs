using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionText : MonoBehaviour
{
    public float updateInterval = 0.5f;  //每幾秒算一次
    private float lastInterval;
    private float fps;
    public TMP_Text FPS_text; //讓UITEXT放進來

    // Start is called before the first frame update
    void Start()
    {
        // TryGetComponent<TMP_Text>(out FPS_text);
        GetComponent<TMP_Text>().text = Application.version;
    }

    //private void Update()
    //{
        //frames++;
        //float timeNow = Time.realtimeSinceStartup;
        //if (timeNow >= lastInterval + updateInterval)  //每0.5秒更新一次
        //{
        //    fps = frames / (timeNow - lastInterval); //幀數= 每幀/每幀間隔毫秒 
        //    frames = 0;
        //    lastInterval = timeNow;
        //    FPS_text.text = fps.ToString(); //在UI上顯示幀數
        //}
    //}
}
