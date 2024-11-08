using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Book))]
public class AutoFlip : MonoBehaviour {
    public FlipMode Mode;
    public float PageFlipTime = 1;
    public float TimeBetweenPages = 1;
    public float DelayBeforeStarting = 0;
    public bool AutoStartFlip=true;
    public Book ControledBook;
    public int AnimationFramesCount = 10;
    
    [SerializeField] private AudioClip _flipSoundFX;
    bool isFlipping = false;
    // Use this for initialization
    void Start () {
        if (!ControledBook)
            ControledBook = GetComponent<Book>();
        if (AutoStartFlip)
            StartFlipping();
        ControledBook.OnFlip.AddListener(new UnityEngine.Events.UnityAction(PageFlipped));
	}
    void PageFlipped()
    {
        isFlipping = false;
    }
	public void StartFlipping()
    {
        StartCoroutine(FlipToEnd());
    }
    public void FlipRightPage()
    {
        if (isFlipping) return;
        if (ControledBook.currentPage >= ControledBook.TotalPageCount) return;
        isFlipping = true;
        ControledBook.ControlCameraAndCanvas(2);
        float frameTime = PageFlipTime / AnimationFramesCount;
        float xc = (ControledBook.EndBottomRight.x + ControledBook.EndBottomLeft.x) / 2;
        float xl = ((ControledBook.EndBottomRight.x - ControledBook.EndBottomLeft.x) / 2) * 0.9f;
        //float h =  ControledBook.Height * 0.5f;
        float h = Mathf.Abs(ControledBook.EndBottomRight.y) * 0.9f;
        float dx = (xl)*2 / AnimationFramesCount;
        SoundManagement.Instance.PlaySoundFXClip(_flipSoundFX, Enviroment.Instance.Player.transform, 1.0f);
        ControledBook.pageDragEnd = false;
        StartCoroutine(FlipRTL(xc, xl, h, frameTime, dx));
    }
    public void FlipLeftPage()
    {
        if (isFlipping) return;
        if (ControledBook.currentPage <= 0) return;
        isFlipping = true;
        ControledBook.ControlCameraAndCanvas(-2);
        float frameTime = PageFlipTime / AnimationFramesCount;
        float xc = (ControledBook.EndBottomRight.x + ControledBook.EndBottomLeft.x) / 2;
        float xl = ((ControledBook.EndBottomRight.x - ControledBook.EndBottomLeft.x) / 2) * 0.9f;
        //float h =  ControledBook.Height * 0.5f;
        float h = Mathf.Abs(ControledBook.EndBottomRight.y) * 0.9f;
        float dx = (xl) * 2 / AnimationFramesCount;
        SoundManagement.Instance.PlaySoundFXClip(_flipSoundFX, Enviroment.Instance.Player.transform, 1.0f);
        ControledBook.pageDragEnd = false;
        StartCoroutine(FlipLTR(xc, xl, h, frameTime, dx));
    }
    public void JumpPage()
    {
        //int normalPage = SubtitleManagement.Instance.EachPages[0] + 8;
        //int hintPage = SubtitleManagement.Instance.EachPages[1] + 8;
        //Debug.Log(normalPage);
        //Debug.Log(hintPage);
        //if (ControledBook.currentPage >= hintPage)
        //{
        //    int page = normalPage % 2 == 0 ? normalPage : normalPage - 1;
        //    // 代表在提示頁要切回劇情頁
        //    ControledBook.targetPage = page;
        //    FlipLeftPage();
        //}
        //else if (ControledBook.currentPage < normalPage)
        //{
        //    int page = normalPage % 2 == 0 ? normalPage : normalPage - 1;
        //    // 代表在劇情頁之前要切去劇情頁
        //    ControledBook.targetPage = page;
        //    FlipRightPage();
        //}
        //else if (ControledBook.currentPage >= normalPage && ControledBook.currentPage < hintPage)
        //{
        //    // 代表在劇情頁要切去提示頁
        //    ControledBook.targetPage = hintPage;
        //    FlipRightPage();
        //}
    }
    IEnumerator FlipToEnd()
    {
        yield return new WaitForSeconds(DelayBeforeStarting);
        float frameTime = PageFlipTime / AnimationFramesCount;
        float xc = (ControledBook.EndBottomRight.x + ControledBook.EndBottomLeft.x) / 2;
        float xl = ((ControledBook.EndBottomRight.x - ControledBook.EndBottomLeft.x) / 2)*0.9f;
        //float h =  ControledBook.Height * 0.5f;
        float h = Mathf.Abs(ControledBook.EndBottomRight.y)*0.9f;
        //y=-(h/(xl)^2)*(x-xc)^2          
        //               y         
        //               |          
        //               |          
        //               |          
        //_______________|_________________x         
        //              o|o             |
        //           o   |   o          |
        //         o     |     o        | h
        //        o      |      o       |
        //       o------xc-------o      -
        //               |<--xl-->
        //               |
        //               |
        float dx = (xl)*2 / AnimationFramesCount;
        switch (Mode)
        {
            case FlipMode.RightToLeft:
                while (ControledBook.currentPage < ControledBook.TotalPageCount)
                {
                    StartCoroutine(FlipRTL(xc, xl, h, frameTime, dx));
                    yield return new WaitForSeconds(TimeBetweenPages);
                }
                break;
            case FlipMode.LeftToRight:
                while (ControledBook.currentPage > 0)
                {
                    StartCoroutine(FlipLTR(xc, xl, h, frameTime, dx));
                    yield return new WaitForSeconds(TimeBetweenPages);
                }
                break;
        }
    }
    IEnumerator FlipRTL(float xc, float xl, float h, float frameTime, float dx)
    {
        float x = xc + xl;
        float y = (-h / (xl * xl)) * (x - xc) * (x - xc);
        Vector3 vec = new Vector3(x, y, 0).normalized;
        ControledBook.DragRightPageToPoint(vec * Time.deltaTime * 180);
        for (int i = 0; i < AnimationFramesCount; i++)
        {
            y = (-h / (xl * xl)) * (x - xc) * (x - xc);
            vec = new Vector3(x, y, 0).normalized;
            ControledBook.UpdateBookRTLToPoint(vec * Time.deltaTime * 60);
            yield return new WaitForSeconds(frameTime);
            x -= dx;
        }
        ControledBook.ReleasePage();
    }
    IEnumerator FlipLTR(float xc, float xl, float h, float frameTime, float dx)
    {
        float x = xc - xl;
        float y = (-h / (xl * xl)) * (x - xc) * (x - xc);
        Vector3 vec = new Vector3(x, y, 0).normalized;
        ControledBook.DragLeftPageToPoint(vec * Time.deltaTime * 180);
        for (int i = 0; i < AnimationFramesCount; i++)
        {
            y = (-h / (xl * xl)) * (x - xc) * (x - xc);
            vec = new Vector3(x, y, 0).normalized;
            ControledBook.UpdateBookLTRToPoint(vec * Time.deltaTime * 60);
            yield return new WaitForSeconds(frameTime);
            x += dx;
        }
        ControledBook.ReleasePage();
    }
}
