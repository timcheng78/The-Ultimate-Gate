using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionText : MonoBehaviour
{
    public float updateInterval = 0.5f;  //�C�X���@��
    private float lastInterval;
    private float fps;
    public TMP_Text FPS_text; //��UITEXT��i��

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
        //if (timeNow >= lastInterval + updateInterval)  //�C0.5���s�@��
        //{
        //    fps = frames / (timeNow - lastInterval); //�V��= �C�V/�C�V���j�@�� 
        //    frames = 0;
        //    lastInterval = timeNow;
        //    FPS_text.text = fps.ToString(); //�bUI�W��ܴV��
        //}
    //}
}
