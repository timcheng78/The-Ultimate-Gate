using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class StartVideoScript : MonoBehaviour
{
    private VideoPlayer _videoPlayer;
    // Start is called before the first frame update
    void Start()
    {
        TryGetComponent<VideoPlayer>(out _videoPlayer);
        _videoPlayer.loopPointReached += EnddingFrame;
    }

    private void EnddingFrame(UnityEngine.Video.VideoPlayer vp)
    {
        _videoPlayer.enabled = false;
        StartCoroutine(ChangeScene());
    }

    IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(1);
    }
}
