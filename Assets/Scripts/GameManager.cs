using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//關卡資訊的class
[System.Serializable]
public class LevelInfo
{
    public int bombCount = 1;
    public int masterCount = 1;
}

//遮蔽物資訊的class
[System.Serializable]
public class ObstacleInfo
{
    public GameObject[] infoObstacleArray1;
    public GameObject[] infoObstacleArray2;
    public GameObject[] infoObstacleArray3;
    public GameObject[] infoObstacleArray4;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int canPlayLevel = 1;
    public int nowLevel = 1;
    public int boardSize = 8;
    public int bombCount = 5;
    public int masterCount = 3;
    public int flaggedBombCount = 0;
    public int UIBombCount = 0;
    public int getMasterCount = 0;

    public GameObject chessBoardRoot;
    public GameObject cameraRoot;
    public GameObject boardRoot;

    public GameObject floorPrefab;
    public Floor[,] floorMatrix;

    public int obstacleArrayNumber = 1;
    public GameObject obstacleRoot1;
    public GameObject obstacleRoot2;
    public GameObject obstacleRoot3;
    public GameObject obstacleRoot4;
    public GameObject[] obstacleArray1;
    public GameObject[] obstacleArray2;
    public GameObject[] obstacleArray3;
    public GameObject[] obstacleArray4;
    Obstacle[] obstacleScriptArray1 = new Obstacle[0];
    Obstacle[] obstacleScriptArray2 = new Obstacle[0];
    Obstacle[] obstacleScriptArray3 = new Obstacle[0];
    Obstacle[] obstacleScriptArray4 = new Obstacle[0];

    public Texture[] FloorTextures;  //0是外圍，1是淺的，2是深的

    public bool isPutFlag = false;
    public bool isFirstStep = true;
    public bool isGameOver = false;
    bool isRotating = false;

    //設定用關卡資訊
    public LevelInfo[] levelInfo;

    //設定用遮蔽物資訊
    public ObstacleInfo[] obstacleInfo;



    //是否第一次玩
    public bool isFirstTime;

    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }

        InitData();
        InitBoard();

        if(isFirstTime)
        {
            UIManager.instance.OpenGameInfoPage();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.R))
        {
            PlayerPrefs.SetInt("CanPlayLevel", 0);
        }
    }

    //初始化資料
    public void InitData()
    {
        //讀取可遊玩關卡資料
        Debug.Log("PlayerPrefs.GetInt(CanPlayLevel, 0) = " + PlayerPrefs.GetInt("CanPlayLevel", 0));
        if(PlayerPrefs.GetInt("CanPlayLevel", 0) == 0)
        {
            //第一次玩，設定等級1
            PlayerPrefs.SetInt("CanPlayLevel", 1);
            canPlayLevel = 1;
            nowLevel = canPlayLevel;

            isFirstTime = true;
        }
        else
        {
            canPlayLevel = PlayerPrefs.GetInt("CanPlayLevel", 1);
            nowLevel = canPlayLevel;
        }



        UIBombCount = 0;
        flaggedBombCount = 0;
        getMasterCount = 0;
        obstacleArrayNumber = 1;

        //根據level設定資料
        bombCount = levelInfo[nowLevel].bombCount;
        masterCount = levelInfo[nowLevel].masterCount;

        //設定遮蔽物資料
        CleanObstacleObject();
        int randomIndex = Random.Range(0, obstacleInfo.Length);
        obstacleArray1 = obstacleInfo[randomIndex].infoObstacleArray1;
        obstacleArray2 = obstacleInfo[randomIndex].infoObstacleArray2;
        obstacleArray3 = obstacleInfo[randomIndex].infoObstacleArray3;
        obstacleArray4 = obstacleInfo[randomIndex].infoObstacleArray4;
    }

    //設定關卡，已經存在棋盤的情況下
    public void SetLevel(int level, bool resetObstacle)
    {
        nowLevel = level;

        UIBombCount = 0;
        flaggedBombCount = 0;
        getMasterCount = 0;
        obstacleArrayNumber = 1;

        bombCount = levelInfo[nowLevel].bombCount;
        masterCount = levelInfo[nowLevel].masterCount;

        //重置棋盤成全部未開
        ResetBoard();

        

        //設定UI
        //設定炸彈的部分
        UIManager.instance.SetFlaggedText(UIBombCount, bombCount);
        //設定師傅的部分
        UIManager.instance.SetMasterText(getMasterCount, masterCount);
        UIManager.instance.ResetUI();

        isFirstStep = true;
        isGameOver = false;

        //棋盤歸位
        chessBoardRoot.transform.rotation = Quaternion.Euler(0, 0, 0);

        if (resetObstacle)
        {
            //設定遮蔽物資料
            CleanObstacleObject();
            int randomIndex = Random.Range(0, obstacleInfo.Length);
            obstacleArray1 = obstacleInfo[randomIndex].infoObstacleArray1;
            obstacleArray2 = obstacleInfo[randomIndex].infoObstacleArray2;
            obstacleArray3 = obstacleInfo[randomIndex].infoObstacleArray3;
            obstacleArray4 = obstacleInfo[randomIndex].infoObstacleArray4;

            //設置障礙物1，不旋轉
            SetObstacle(obstacleRoot1, obstacleArray1, 0, ref obstacleScriptArray1);
            //設置障礙物2，旋轉90度
            SetObstacle(obstacleRoot2, obstacleArray2, -90, ref obstacleScriptArray2);
            //設置障礙物3，旋轉180度
            SetObstacle(obstacleRoot3, obstacleArray3, -180, ref obstacleScriptArray3);
            //設置障礙物4，旋轉270度
            SetObstacle(obstacleRoot4, obstacleArray4, -270, ref obstacleScriptArray4);
        }

        //設定陰影
        SetShadowByObstacle();
        //設定遮蔽物透明度
        ResetObstacleAlpha();
        SetObstacleAlpha();

        //淡入BGM
        SoundManager.instance.CallFadeInBGM();
    }

    public void LevelUp()
    {
        //看有沒有超過可遊玩關卡
        if(nowLevel >= canPlayLevel && canPlayLevel == 3)
        {
            return;
        }
        //升級並記錄可遊玩關卡
        nowLevel++;
        if (nowLevel > 3)
        {
            nowLevel = 3;
        }
        canPlayLevel++;
        if(canPlayLevel > 3)
        {
            canPlayLevel = 3;
        }
        PlayerPrefs.SetInt("CanPlayLevel", canPlayLevel);
    }

    //清空遮蔽物
    public void CleanObstacleObject()
    {
        if(obstacleScriptArray1.Length == 0)
        {
            return;
        }
        for(int i = 0; i < obstacleScriptArray1.Length; i++)
        {
            if(obstacleScriptArray1[i] != null)
            {
                Destroy(obstacleScriptArray1[i].gameObject);
                obstacleScriptArray1[i] = null;
            }
            if (obstacleScriptArray2[i] != null)
            {
                Destroy(obstacleScriptArray2[i].gameObject);
                obstacleScriptArray2[i] = null;
            }
            if (obstacleScriptArray3[i] != null)
            {
                Destroy(obstacleScriptArray3[i].gameObject);
                obstacleScriptArray3[i] = null;
            }
            if (obstacleScriptArray4[i] != null)
            {
                Destroy(obstacleScriptArray4[i].gameObject);
                obstacleScriptArray4[i] = null;
            }
        }
    }

    //重置棋盤成未開
    public void ResetBoard()
    {
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                
                floorMatrix[x, y].ResetFloor();

            }
        }
    }

    //初始化棋盤
    public void InitBoard()
    {
        //設定棋盤資料
        floorMatrix = new Floor[boardSize, boardSize];

        //設定地板
        float originX = 0 - boardSize / 2 + 0.5f;
        float originY = boardSize / 2 - 0.5f;
        GameObject copyFloorObj;
        Floor getFloor;
        for (int y = 0; y < boardSize; y++)
        {
            for(int x = 0; x < boardSize; x++)
            {
                copyFloorObj = Instantiate(floorPrefab);
                copyFloorObj.transform.parent = boardRoot.transform;
                copyFloorObj.transform.rotation = Quaternion.identity;
                copyFloorObj.gameObject.name = "floor_" + x + "_" + y;

                float positionX = originX + x;
                float positionY = originY - y;
                copyFloorObj.transform.position = new Vector3(positionX, 0, positionY);

                getFloor = copyFloorObj.GetComponent<Floor>();
                getFloor.matrixX = x;
                getFloor.matrixY = y;
                getFloor.type = TileType.Empty;
                getFloor.visibleType = TileType.Empty;

                floorMatrix[x, y] = getFloor;

                //設定buttomFloor
                /*
                if(y == 0 || y == boardSize-1)
                {
                    //最上面和最下面一條，外圍圖
                    getFloor.buttomTexture.material.mainTexture = FloorTextures[2];
                }
                else if(x == 0 || x == boardSize - 1)
                {
                    //最左邊和最右邊一條，外圍圖
                    getFloor.buttomTexture.material.mainTexture = FloorTextures[2];
                }
                else
                */
                {
                    //中間的
                    if(x % 2 == 1 && y % 2 == 0)
                    {
                        //淺色
                        getFloor.buttomTexture.material.mainTexture = FloorTextures[1];
                    }
                    else if (x % 2 == 0 && y % 2 == 1)
                    {
                        //淺色
                        getFloor.buttomTexture.material.mainTexture = FloorTextures[1];
                    }
                    else
                    {
                        //深色
                        getFloor.buttomTexture.material.mainTexture = FloorTextures[2];
                    }
                }
            }
        }

        //設置障礙物1，不旋轉
        SetObstacle(obstacleRoot1, obstacleArray1, 0, ref obstacleScriptArray1);
        //設置障礙物2，旋轉90度
        SetObstacle(obstacleRoot2, obstacleArray2, -90, ref obstacleScriptArray2);
        //設置障礙物3，旋轉180度
        SetObstacle(obstacleRoot3, obstacleArray3, -180, ref obstacleScriptArray3);
        //設置障礙物4，旋轉270度
        SetObstacle(obstacleRoot4, obstacleArray4, -270, ref obstacleScriptArray4);

        //設定陰影
        SetShadowByObstacle();
        //設定遮蔽物透明度
        ResetObstacleAlpha();
        SetObstacleAlpha();

        //設定UI
        //設定炸彈的部分
        UIManager.instance.SetFlaggedText(UIBombCount, bombCount);
        //設定師傅的部分
        UIManager.instance.SetMasterText(getMasterCount, masterCount);

        isFirstStep = true;
        isGameOver = false;
    }

    //放炸彈
    public void SetBomb(int x, int y)
    {
        //x,y是點擊位置，不要放到這個位置上
        for (int count = 0; count < this.bombCount;)
        {
            int bombX = UnityEngine.Random.Range(0, boardSize);
            int bombY = UnityEngine.Random.Range(0, boardSize);

            if(bombX == x && bombY == y)
            {
                continue;
            }

            

            //隨機到不是炸彈才設定成炸彈
            if (floorMatrix[bombX, bombY].type != TileType.Bomb && floorMatrix[bombX, bombY].type != TileType.Master)
            {
                floorMatrix[bombX, bombY].type = TileType.Bomb;

                //附近八格不是炸彈也不是師傅的話就數字+1
                if (bombX + 1 < boardSize && floorMatrix[bombX + 1, bombY].type != TileType.Bomb && floorMatrix[bombX + 1, bombY].type != TileType.Master)
                {
                    floorMatrix[bombX + 1, bombY].type = (TileType)((int)floorMatrix[bombX + 1, bombY].type + 1);
                }
                if (bombX + 1 < boardSize && bombY + 1 < boardSize && floorMatrix[bombX + 1, bombY + 1].type != TileType.Bomb && floorMatrix[bombX + 1, bombY + 1].type != TileType.Master)
                {
                    floorMatrix[bombX + 1, bombY + 1].type = (TileType)((int)floorMatrix[bombX + 1, bombY + 1].type + 1);
                }
                if (bombY + 1 < boardSize && floorMatrix[bombX, bombY + 1].type != TileType.Bomb && floorMatrix[bombX, bombY + 1].type != TileType.Master)
                {
                    floorMatrix[bombX, bombY + 1].type = (TileType)((int)floorMatrix[bombX, bombY + 1].type + 1);
                }
                if (bombX - 1 >= 0 && bombY + 1 < boardSize && floorMatrix[bombX - 1, bombY + 1].type != TileType.Bomb && floorMatrix[bombX - 1, bombY + 1].type != TileType.Master)
                {
                    floorMatrix[bombX - 1, bombY + 1].type = (TileType)((int)floorMatrix[bombX - 1, bombY + 1].type + 1);
                }

                if (bombX - 1 >= 0 && floorMatrix[bombX - 1, bombY].type != TileType.Bomb && floorMatrix[bombX - 1, bombY].type != TileType.Master)
                {
                    floorMatrix[bombX - 1, bombY].type = (TileType)((int)floorMatrix[bombX - 1, bombY].type + 1);
                }
                if (bombX - 1 >= 0 && bombY - 1 >= 0 && floorMatrix[bombX - 1, bombY - 1].type != TileType.Bomb && floorMatrix[bombX - 1, bombY - 1].type != TileType.Master)
                {
                    floorMatrix[bombX - 1, bombY - 1].type = (TileType)((int)floorMatrix[bombX - 1, bombY - 1].type + 1);
                }
                if (bombY - 1 >= 0 && floorMatrix[bombX, bombY - 1].type != TileType.Bomb && floorMatrix[bombX, bombY - 1].type != TileType.Master)
                {
                    floorMatrix[bombX, bombY - 1].type = (TileType)((int)floorMatrix[bombX, bombY - 1].type + 1);
                }
                if (bombX + 1 < boardSize && bombY - 1 >= 0 && floorMatrix[bombX + 1, bombY - 1].type != TileType.Bomb && floorMatrix[bombX + 1, bombY - 1].type != TileType.Master)
                {
                    floorMatrix[bombX + 1, bombY - 1].type = (TileType)((int)floorMatrix[bombX + 1, bombY - 1].type + 1);
                }

                count++;
            }
        }
    }

    //放師傅
    public void SetMaster()
    {
        for(int count = 0; count < masterCount;)
        {
            int masterX = UnityEngine.Random.Range(0, boardSize);
            int masterY = UnityEngine.Random.Range(0, boardSize);

            floorMatrix[masterX, masterY].type = TileType.Master;
            count++;
        }
    }

    //點擊翻開地板
    public void Reveal(int x, int y)
    {
        
        //翻開(x, y)位置
        //超過棋盤範圍，不處理
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
        {
            return;
        }

        //已插旗，不處理
        if (floorMatrix[x, y].visibleType == TileType.Flag)
        {
            return;
        }

        //已經打開的，不處理
        if (floorMatrix[x, y].visibleType == TileType.Open)
        {
            return;
        }

        //該格是炸彈，不翻開，這邊會是點擊格子擴散的時候判斷
        if (floorMatrix[x, y].type == TileType.Bomb)
        {
            return;
        }
        
        //該格是數字或空白，打開並顯示
        if ((int)floorMatrix[x, y].type <= 8)
        {
            floorMatrix[x, y].visibleType = TileType.Open;
            floorMatrix[x, y].ShowThisFloor();
            CheckGameOver();

            //如果是數字的話就不繼續往下做
            if (floorMatrix[x, y].type != TileType.Empty)
            {
                //檢查周圍是否有師傅，有的話換成反白的數字
                if(CheckMasterAround(x, y))
                {
                    floorMatrix[x, y].ChangeToHighlightNumber();
                }
                return;
            }
        }

        //該格是師傅，不翻開
        if (floorMatrix[x, y].type == TileType.Master)
        {
            return;
        }

        //擴散翻開其他格子，遞迴去做
        // Recursive reveal.
        Reveal(x + 1, y);
        Reveal(x + 1, y + 1);
        Reveal(x, y + 1);
        Reveal(x - 1, y + 1);

        Reveal(x - 1, y);
        Reveal(x - 1, y - 1);
        Reveal(x, y - 1);
        Reveal(x + 1, y - 1);

        
    }

    //確認遊戲結束
    public void CheckGameOver()
    {
        //確認是否過關
        if(flaggedBombCount == bombCount && getMasterCount == masterCount && CheckAllOpen())
        {
            Debug.Log("Game Clear");
            isGameOver = true;
            UIManager.instance.OpenGameClearPage();
            SoundManager.instance.PauseBGM();
            SoundManager.instance.PlayWin();
        }
    }

    //是否全部格子都打開
    public bool CheckAllOpen()
    {
        for(int x = 0; x < boardSize; x++)
        {
            for(int y = 0; y < boardSize; y++)
            {
                if((int)floorMatrix[x,y].type < 9 && floorMatrix[x, y].visibleType == TileType.Empty ||
                    (int)floorMatrix[x, y].type < 9 && floorMatrix[x, y].visibleType == TileType.Flag)
                {
                    return false;
                }
            }
        }

        return true;
    }

    //踩到炸彈了
    public void GetBomb()
    {
        SoundManager.instance.PlayAhs();
        isGameOver = true;
        //全部翻開
        for (int x = 0; x < boardSize; x++)
        {
            for(int y = 0; y < boardSize; y++)
            {
                if (floorMatrix[x, y].type != TileType.Empty && (int)floorMatrix[x, y].type < 9)
                {
                    //檢查周圍是否有師傅，有的話換成反白的數字
                    if (CheckMasterAround(x, y))
                    {
                        floorMatrix[x, y].ChangeToHighlightNumber();
                    }
                    else
                    {
                        floorMatrix[x, y].ShowThisFloor();
                    }
                    
                }
                else
                {
                    floorMatrix[x, y].ShowThisFloor();
                }
                
            }
        }

        StartCoroutine(ShowGameOver());
    }

    IEnumerator ShowGameOver()
    {
        SoundManager.instance.PauseBGM();
        yield return new WaitForSeconds(3);

        Debug.Log("Game Over");
        UIManager.instance.OpenGameOverPage();
        
        SoundManager.instance.PlayLose();
    }

    public void SwitchPutFlag(bool flagSwitch)
    {
        isPutFlag = flagSwitch;
    }

    //檢查周圍師傅
    public bool CheckMasterAround(int x, int y)
    {
        //檢查X型範圍兩格
        //左上
        if(x - 1 >= 0 && y - 1 >= 0 && floorMatrix[x - 1, y - 1].type == TileType.Master)
        {
            return true;
        }
        if (x - 2 >= 0 && y - 2 >= 0 && floorMatrix[x - 2, y - 2].type == TileType.Master)
        {
            return true;
        }

        //左下
        if (x - 1 >= 0 && y + 1 < boardSize && floorMatrix[x - 1, y + 1].type == TileType.Master)
        {
            return true;
        }
        if (x - 2 >= 0 && y + 2 < boardSize && floorMatrix[x - 2, y + 2].type == TileType.Master)
        {
            return true;
        }

        //右上
        if (x + 1 < boardSize && y - 1 >= 0 && floorMatrix[x + 1, y - 1].type == TileType.Master)
        {
            return true;
        }
        if (x + 2 < boardSize && y - 2 >= 0 && floorMatrix[x + 2, y - 2].type == TileType.Master)
        {
            return true;
        }

        //右下
        if (x + 1 < boardSize && y + 1 < boardSize && floorMatrix[x + 1, y + 1].type == TileType.Master)
        {
            return true;
        }
        if (x + 2 < boardSize && y + 2 < boardSize && floorMatrix[x + 2, y + 2].type == TileType.Master)
        {
            return true;
        }



        return false;
    }

    //擺放遮蔽物
    public void SetObstacle(GameObject root, GameObject[] getObstacles, float angle, ref Obstacle[] scriptArray)
    {
        float originX = 0 - boardSize / 2 + 0.5f;
        float originY = boardSize / 2;
        scriptArray = new Obstacle[getObstacles.Length];
        for (int i = 0; i < getObstacles.Length; i++)
        {
            //陣列沒設定東西就跳過
            if(getObstacles[i] == null)
            {
                continue;
            }
            float positionX = originX + i;
            GameObject createObs = Instantiate(getObstacles[i], new Vector3(positionX, 0, originY), Quaternion.identity, root.transform);
            //取得遮蔽物腳本
            scriptArray[i] = createObs.GetComponent<Obstacle>();
        }
        root.transform.Rotate(new Vector3(0, angle, 0));
    }


    //逆時針旋轉矩陣
    static Floor[,] RotateMatrixCounterClockwise(Floor[,] oldMatrix)
    {
        Floor[,] newMatrix = new Floor[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];
        int newColumn, newRow = 0;
        for (int oldColumn = oldMatrix.GetLength(1) - 1; oldColumn >= 0; oldColumn--)
        {
            newColumn = 0;
            for (int oldRow = 0; oldRow < oldMatrix.GetLength(0); oldRow++)
            {
                newMatrix[newRow, newColumn] = oldMatrix[oldRow, oldColumn];
                newColumn++;
            }
            newRow++;
        }
        return newMatrix;
    }

    //順時針旋轉矩陣
    static Floor[,] RotateMatrixClockwise(Floor[,] oldMatrix)
    {
        Floor[,] newMatrix = new Floor[oldMatrix.GetLength(1), oldMatrix.GetLength(0)];
        int newColumn, newRow = 0;
        for (int oldColumn = 0; oldColumn < oldMatrix.GetLength(1); oldColumn++)
        {
            newColumn = 0;
            for (int oldRow = oldMatrix.GetLength(0) - 1; oldRow >= 0; oldRow--)
            {
                newMatrix[newRow, newColumn] = oldMatrix[oldRow, oldColumn];
                newColumn++;
            }
            newRow++;
        }
        return newMatrix;
    }

    //讀取矩陣資料
    void ReadArray(int[,] getArray)
    {
        string showString = "";
        for (int i = 0; i < getArray.GetLength(0); i++)
        {
            showString = "";
            for (int j = 0; j < getArray.GetLength(1); j++)
            {
                showString += getArray[i, j] + " ";
            }
            Debug.Log(showString);
        }
    }


    //根據障礙物設定陰影
    public void SetShadowByObstacle()
    {
        Obstacle[] getObstacles = new Obstacle[boardSize];
        switch(obstacleArrayNumber)
        {
            case 1:
                getObstacles = obstacleScriptArray1;
                break;
            case 2:
                getObstacles = obstacleScriptArray2;
                break;
            case 3:
                getObstacles = obstacleScriptArray3;
                break;
            case 4:
                getObstacles = obstacleScriptArray4;
                break;
            default:
                getObstacles = obstacleScriptArray1;
                break;

        }

        ClearShadowFloor();

        for(int i = 1; i <= 4; i++)
        {
            //設定陰影
            if(i == obstacleArrayNumber)
            {
                SetShadowFloor(getObstacles);
            }

            //旋轉矩陣
            floorMatrix = RotateMatrixCounterClockwise(floorMatrix);
        }

    }

    public void SetShadowFloor(Obstacle[] obs)
    {
        int startIndex;
        int shadowLength;
        for(int x = 0; x < obs.Length; x++)
        {
            if(obs[x] == null)
            {
                continue;
            }
            startIndex = obs[x].startIndex;
            shadowLength = obs[x].shadowLength;

            for(int y = 0; y < shadowLength; y++)
            {
                floorMatrix[x, startIndex + y].inShadow = true;
                
            }
            
        }

        for (int x = 0; x < boardSize; x++)
        {
            
            for (int y = 0; y < boardSize; y++)
            {
                
                floorMatrix[x, y].SetShadowColor();
            }

        }
    }

    public void ClearShadowFloor()
    {
        for(int x = 0; x < floorMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < floorMatrix.GetLength(0); y++)
            {
                floorMatrix[x, y].inShadow = false;
                floorMatrix[x, y].SetShadowColor();
            }
        }
    }

    //轉棋盤
    public void RotateChessBoardButton()
    {
        //如果正在轉就不能連續轉
        if(isRotating)
        {
            return;
        }
        //順時針旋轉棋盤
        isRotating = true;
        StartCoroutine(RotateChessBoard());
    }

    IEnumerator RotateChessBoard()
    {
        float nowAngle = chessBoardRoot.transform.localEulerAngles.y;
        float targetAngle = nowAngle + 90;
        //Debug.Log("nowAngle = " + nowAngle);
        //Debug.Log("targetAngle = " + targetAngle);

        while (nowAngle < targetAngle)
        {
            nowAngle++;
            if(nowAngle >= targetAngle)
            {
                nowAngle = targetAngle;
            }
            //Debug.Log("in while nowAngle = " + nowAngle);


            chessBoardRoot.transform.rotation = Quaternion.Euler(0, nowAngle, 0);
            yield return 0;
        }

        isRotating = false;

        //設定陰影
        obstacleArrayNumber++;
        if(obstacleArrayNumber > 4)
        {
            obstacleArrayNumber = 1;
        }
        SetShadowByObstacle();
        ResetObstacleAlpha();
        SetObstacleAlpha();

        yield return 0;
    }

    //設定障礙物透明度
    public void SetObstacleAlpha()
    {
        int minusOneWallNumber = obstacleArrayNumber - 1;
        if(minusOneWallNumber <= 0)
        {
            minusOneWallNumber += 4;
        }
        int minusTwoWallNumber = obstacleArrayNumber - 2;
        if (minusTwoWallNumber <= 0)
        {
            minusTwoWallNumber += 4;
        }
        Debug.Log("minusOneWallNumber = " + minusOneWallNumber);
        Debug.Log("minusTwoWallNumber = " + minusTwoWallNumber);

        Obstacle[] minusOneWallArray = GetObstacleScriptArray(minusOneWallNumber);
        Obstacle[] minusTwoWallArray = GetObstacleScriptArray(minusTwoWallNumber);

        for(int i = 0; i < minusOneWallArray.Length; i++)
        {
            if(minusOneWallArray[i] != null)
            {
                minusOneWallArray[i].SetImageTranslucent();
                minusOneWallArray[i].isTranslucent = true;
            }
            if (minusTwoWallArray[i] != null)
            {
                minusTwoWallArray[i].SetImageTranslucent();
                minusTwoWallArray[i].isTranslucent = true;
            }
        }


    }

    //重置所有障礙物的透明度
    public void ResetObstacleAlpha()
    {
        for(int i = 0; i < obstacleScriptArray1.Length; i++)
        {
            if(obstacleScriptArray1[i] != null)
            {
                obstacleScriptArray1[i].SetImageOpaque();
                obstacleScriptArray1[i].isTranslucent = false;
            }
            if (obstacleScriptArray2[i] != null)
            {
                obstacleScriptArray2[i].SetImageOpaque();
                obstacleScriptArray2[i].isTranslucent = false;
            }
            if (obstacleScriptArray3[i] != null)
            {
                obstacleScriptArray3[i].SetImageOpaque();
                obstacleScriptArray3[i].isTranslucent = false;
            }
            if (obstacleScriptArray4[i] != null)
            {
                obstacleScriptArray4[i].SetImageOpaque();
                obstacleScriptArray4[i].isTranslucent = false;
            }
        }
    }

    public Obstacle[] GetObstacleScriptArray(int num)
    {
        switch(num)
        {
            case 1:
                return obstacleScriptArray1;
            case 2:
                return obstacleScriptArray2;
            case 3:
                return obstacleScriptArray3;
            case 4:
                return obstacleScriptArray4;
            default:
                return obstacleScriptArray1;
        }
    }
}
