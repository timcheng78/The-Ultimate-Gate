using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSwitch : MonoBehaviour
{
    public Texture2D[] textures; // ��J�� Texture2D �}�C
    public float switchInterval = 1f; // �������j�]��^
    public bool toggleSwitch = false;

    private int currentIndex = 0; // ��e��ܪ�����
    private float timer = 0f; // �p�ɾ�
    private RawImage rawImage;

    private void Awake()
    {
        TryGetComponent<RawImage>(out rawImage);    
    }

    void Update()
    {
        if (!toggleSwitch) return;
        // �W�[�p�ɾ�
        timer += Time.deltaTime;

        // �ˬd�O�_��F�������ɶ�
        if (timer >= switchInterval)
        {
            // ������U�@�� Texture
            currentIndex = (currentIndex + 1) % textures.Length; // �i��`������
            rawImage.texture = textures[currentIndex]; // ��s RawImage �����z

            // ���m�p�ɾ�
            timer = 0f;
        }
    }
}
