using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TextMeteorEffect : MonoBehaviour
{
    [Header("預覽設定")]
    [SerializeField] public string previewText = "測試文字";
    [SerializeField] private bool autoPlayOnStart = true;

    [Header("文字設定")]
    [SerializeField] private GameObject creditStar;
    [SerializeField, Range(0.01f, 1f)] private float characterDelay = 0.1f;
    [SerializeField] private float startDistance = 1f;

    [Header("動畫設定")]
    [SerializeField, Range(1f, 50f)] private float moveSpeed = 10f;
    [SerializeField, Range(0f, 90f)] private float fallAngle = 45f;
    [SerializeField, Range(0.1f, 2f)] private float fadeInDuration = 0.5f;
    [SerializeField, Range(0.1f, 2f)] private float fadeOutDuration = 0.5f;

    [Header("流星效果")]
    [SerializeField] private Color trailColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField, Range(0.1f, 2f)] private float trailLength = 0.5f;
    [SerializeField, Range(0.1f, 2f)] private float trailWidth = 0.2f;

    [Header("效能優化")]
    [SerializeField, Range(5, 50)] private int poolSize = 10;

    private Queue<MeteorCharacter> meteorPool;
    private List<MeteorCharacter> activeMeteors;
    private Vector3 startPosition;
    private Vector3 moveDirection;
    private WaitForSeconds waitDelay;
    private bool isPlaying = false;

    private class MeteorCharacter
    {
        public Vector3 startPosition;
        public GameObject gameObject;
        public TextMeshPro text;
        public TrailRenderer trail;
        public ParticleSystem particles;
    }

    private void Awake()
    {
        CacheCalculations();
        InitializePool();
    }

    private void Start()
    {
        if (autoPlayOnStart)
        {
            StartEffect(previewText);
        }
    }

    private void CacheCalculations()
    {
        startPosition = transform.position;
        moveDirection = Quaternion.Euler(0, 0, -fallAngle) * Vector3.right;
        waitDelay = new WaitForSeconds(characterDelay);
        activeMeteors = new List<MeteorCharacter>();
    }

    private void InitializePool()
    {
        if (meteorPool != null)
        {
            foreach (var meteor in meteorPool)
            {
                if (meteor.gameObject != null)
                {
                    DestroyImmediate(meteor.gameObject);
                }
            }
        }

        meteorPool = new Queue<MeteorCharacter>();

        for (int i = 0; i < poolSize; i++)
        {
            CreatePooledMeteor(i);
        }
    }

    private void CreatePooledMeteor(int index)
    {
        GameObject meteorObj = Instantiate(creditStar, transform);
       
        meteorObj.SetActive(false);

        MeteorCharacter meteor = new MeteorCharacter
        {
            startPosition = startPosition - new Vector3(0, index * startDistance, 0),
            gameObject = meteorObj,
            text = meteorObj.GetComponent<TextMeshPro>(),
            trail = meteorObj.transform.GetChild(0).GetComponent<TrailRenderer>(),
            //particles = particles
        };
        meteorPool.Enqueue(meteor);
    }

    public void StartEffect(string text)
    {
        if (isPlaying)
        {
            StopAllCoroutines();
            foreach (var meteor in activeMeteors.ToArray())
            {
                ReturnToPool(meteor);
            }
        }

        isPlaying = true;
        StartCoroutine(AnimateText(text));
    }

    private IEnumerator AnimateText(string fullText)
    {
        string[] texts = fullText.Split(" ");
        for (int i = 0; i < texts.Length; i++)
        {
            MeteorCharacter meteor = GetMeteorFromPool();
            meteor.text.text = texts[i].ToString();
            meteor.gameObject.transform.position = meteor.startPosition;
            meteor.text.alpha = 0f;
            meteor.trail.Clear(); // 清除之前的拖尾
            if (meteor.particles) meteor.particles.Play();

            StartCoroutine(AnimateMeteor(meteor));

            yield return waitDelay;
        }

        isPlaying = false;
    }

    private IEnumerator AnimateMeteor(MeteorCharacter meteor)
    {
        Vector3 currentPos = meteor.startPosition;
        float elapsedTime = -1f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, fadeInDuration);
            meteor.text.alpha = alpha;

            currentPos += moveDirection * moveSpeed * Time.deltaTime;
            meteor.gameObject.transform.position = currentPos;

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (meteor.particles) meteor.particles.Stop();

        elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            meteor.text.alpha = alpha;
            yield return null;
        }

        ReturnToPool(meteor);
    }

    private MeteorCharacter GetMeteorFromPool()
    {
        if (meteorPool.Count > 0)
        {
            MeteorCharacter meteor = meteorPool.Dequeue();
            meteor.gameObject.SetActive(true);
            activeMeteors.Add(meteor);
            return meteor;
        }

        CreatePooledMeteor(poolSize++);
        return GetMeteorFromPool();
    }

    private void ReturnToPool(MeteorCharacter meteor)
    {
        meteor.gameObject.SetActive(false);
        activeMeteors.Remove(meteor);
        meteorPool.Enqueue(meteor);
    }

    private void OnDestroy()
    {
        if (meteorPool != null)
        {
            foreach (var meteor in meteorPool)
            {
                if (meteor.gameObject != null)
                {
                    DestroyImmediate(meteor.gameObject);
                }
            }
        }

        foreach (var meteor in activeMeteors)
        {
            if (meteor.gameObject != null)
            {
                DestroyImmediate(meteor.gameObject);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TextMeteorEffect))]
public class TextMeteorEffectEditor : Editor
{
    private TextMeteorEffect effect;

    private void OnEnable()
    {
        effect = (TextMeteorEffect)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("播放預覽效果"))
        {
            effect.StartEffect(effect.GetComponent<TextMeteorEffect>().previewText);
        }

        if (GUILayout.Button("重置對象池"))
        {
            effect.SendMessage("InitializePool");
        }
    }
}
#endif