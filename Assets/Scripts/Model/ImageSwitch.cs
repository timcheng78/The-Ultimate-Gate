using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSwitch : MonoBehaviour
{
    public Texture2D[] textures; // 輸入的 Texture2D 陣列
    public float switchInterval = 1f; // 切換間隔（秒）
    public bool toggleSwitch = false;

    private int currentIndex = 0; // 當前顯示的索引
    private float timer = 0f; // 計時器
    private RawImage rawImage;

    private void Awake()
    {
        TryGetComponent<RawImage>(out rawImage);    
    }

    void Update()
    {
        if (!toggleSwitch) return;
        // 增加計時器
        timer += Time.deltaTime;

        // 檢查是否到了切換的時間
        if (timer >= switchInterval)
        {
            // 切換到下一個 Texture
            currentIndex = (currentIndex + 1) % textures.Length; // 進行循環切換
            rawImage.texture = textures[currentIndex]; // 更新 RawImage 的紋理

            // 重置計時器
            timer = 0f;
        }
    }
}
