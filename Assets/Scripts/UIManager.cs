using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public Toggle putFlagSwitch;
    public Text flaggedText;
    public Text masterText;

    //各個選單畫面
    public GameObject optionPage;
    public GameObject gameOverPage;
    public GameObject gameClearPage;
    public GameObject gameInfoPage;
    public Toggle backgroundMusicSwitch;
    public GameObject AskQuitPage;
    public GameObject masterJump;


    //選擇關卡畫面們
    public int selectLevelNumber = 1;
    public GameObject selectLevelPage;
    public GameObject selectLevel1Open;
    public GameObject selectLevel2Open;
    public GameObject selectLevel3Open;
    public GameObject selectLevel2Close;
    public GameObject selectLevel3Close;

    //說明頁面們
    int infoPageIndex = 0;
    [SerializeField]
    Image gameInfoImage;
    [SerializeField]
    Sprite[] gameInfoPages;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //重置
    public void ResetUI()
    {
        putFlagSwitch.isOn = false;
        GameManager.instance.SwitchPutFlag(putFlagSwitch.isOn);
    }
    
    //切換一般點擊和插旗
    public void SetPuttingFlagBool()
    {
        GameManager.instance.SwitchPutFlag(putFlagSwitch.isOn);
    }

    //呼叫攝影機左移
    public void CallCameraMoveLeft()
    {
        CameraController.instance.CameraMoveLeft();
    }

    //呼叫攝影機右移
    public void CallCameraMoveRight()
    {
        CameraController.instance.CameraMoveRight();
    }

    //呼叫攝影機放大
    public void CallCameraZoomIn()
    {
        CameraController.instance.CameraZoomIn();
    }

    //呼叫攝影機縮小
    public void CallCameraZoomOut()
    {
        CameraController.instance.CameraZoomOut();
    }

    //呼叫旋轉棋盤
    public void CallBoardRotate()
    {
        GameManager.instance.RotateChessBoardButton();
    }

    //設定插旗數量
    public void SetFlaggedText(int left, int right)
    {
        flaggedText.text = "" + left + "/" + right;
    }

    //設定找到師傅數量
    public void SetMasterText(int left, int right)
    {
        masterText.text = "" + left + "/" + right;
    }

    //開啟option畫面
    public void OpenOptionPage()
    {
        optionPage.SetActive(true);
    }

    public void CloseOptionPage()
    {
        optionPage.SetActive(false);
    }
    
    //開啟遊戲說明頁
    public void OpenGameInfoPage()
    {
        gameInfoPage.SetActive(true);
        optionPage.SetActive(false);
        infoPageIndex = 0;
        gameInfoImage.sprite = gameInfoPages[infoPageIndex];
    }

    public void ChangeGameInfoPage()
    {
        infoPageIndex++;
        if(infoPageIndex < gameInfoPages.Length)
        {
            gameInfoImage.sprite = gameInfoPages[infoPageIndex];
        }
        else
        {
            //超出陣列，關閉說明
            CloseGameInfoPage();
        }
    }

    public void CloseGameInfoPage()
    {
        gameInfoPage.SetActive(false);
        if (GameManager.instance.isFirstTime)
        {
            
            GameManager.instance.isFirstTime = false;
        }
        else
        {
            
            optionPage.SetActive(true);
        }
        
    }

    //開啟過關畫面
    public void OpenGameClearPage()
    {
        gameClearPage.SetActive(true);
    }

    //下一關按鈕
    public void NextLevelButton()
    {
        GameManager.instance.LevelUp();
        GameManager.instance.SetLevel(GameManager.instance.nowLevel, true);
        CameraController.instance.CameraReset();
        SoundManager.instance.StopAllSoundEffect();
        ResetUI();
        gameClearPage.SetActive(false);
    }

    //開啟失敗畫面
    public void OpenGameOverPage()
    {
        gameOverPage.SetActive(true);
    }

    //失敗畫面重新開始按鈕
    public void RestartButton()
    {
        GameManager.instance.SetLevel(GameManager.instance.nowLevel, false);
        CameraController.instance.CameraReset();
        ResetUI();
        SoundManager.instance.StopAllSoundEffect();
        gameOverPage.SetActive(false);
    }

    //選單 選關按鈕
    public void OpenSelectLevelButton()
    {
        optionPage.SetActive(false);
        selectLevelPage.SetActive(true);
        selectLevelNumber = 1;
        OpenWhichLevelPage();
    }

    //選單 選關返回
    public void ReturnOptionPage()
    {
        optionPage.SetActive(true);
        selectLevelPage.SetActive(false);
    }

    //選關確定
    public void SelectLevelConfirm()
    {
        GameManager.instance.SetLevel(selectLevelNumber, true);
        selectLevelPage.SetActive(false);
        
    }

    //選關按鈕左
    public void SelectLastLevel()
    {
        selectLevelNumber--;
        if(selectLevelNumber < 1)
        {
            selectLevelNumber = 1;
        }
        OpenWhichLevelPage();
    }

    //選關按鈕右
    public void SelectNextLevel()
    {
        selectLevelNumber++;
        if (selectLevelNumber > 3)
        {
            selectLevelNumber = 3;
        }
        OpenWhichLevelPage();
    }

    //根據選關變數開啟選關畫面
    public void OpenWhichLevelPage()
    {
        selectLevel1Open.SetActive(false);
        selectLevel2Open.SetActive(false);
        selectLevel3Open.SetActive(false);
        selectLevel2Close.SetActive(false);
        selectLevel3Close.SetActive(false);
        if (selectLevelNumber == 1)
        {
            selectLevel1Open.SetActive(true);
        }
        else if(selectLevelNumber == 2)
        {
            if(selectLevelNumber <= GameManager.instance.canPlayLevel)
            {
                selectLevel2Open.SetActive(true);
            }
            else
            {
                selectLevel2Close.SetActive(true);
            }
        }
        else if (selectLevelNumber == 3)
        {
            if (selectLevelNumber <= GameManager.instance.canPlayLevel)
            {
                selectLevel3Open.SetActive(true);
            }
            else
            {
                selectLevel3Close.SetActive(true);
            }
        }
    }

    //設定背景音樂
    public void SetBackgroundMusic(Toggle toggle)
    {
        SoundManager.instance.SetBackgroundPlayer(!toggle.isOn);
    }

    //開啟詢問離開
    public void OpenAskQuitPage()
    {
        optionPage.SetActive(false);
        AskQuitPage.SetActive(true);
    }

    public void CloseAskQuitPage()
    {
        optionPage.SetActive(true);
        AskQuitPage.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    //開啟並撥放師傅動畫，1.5秒後關閉
    public void OpenMasterJump()
    {
        masterJump.SetActive(true);
        StartCoroutine(CloseMasterJump());
    }

    IEnumerator CloseMasterJump()
    {
        yield return new WaitForSeconds(1.5f);
        masterJump.SetActive(false);
    }
}
