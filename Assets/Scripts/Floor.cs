using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public enum TileType
{
    Empty = 0, // Empty tile.

    // 1-8.

    Open = 9,
    Bomb = 10,
    Flag = 11,
    Master = 12,
}

public class Floor : MonoBehaviour
{
    public TileType type;
    public TileType visibleType;
    public int matrixX;
    public int matrixY;
    public bool inShadow = false;
    public bool catchMaster = false;

    public Renderer buttomTexture;
    public Renderer tileTexture;
    public Renderer eggTexture;
    public Texture[] typeTextures;
    public Texture[] highlightNumbers;  //反白數字

    Color normalColor = new Color(1, 1, 1);
    Color shadowColor = new Color(0.5f, 0.5f, 0.5f);

    public GameObject egg;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    private void OnMouseUpAsButton()
    {
#if UNITY_EDITOR
        if (EventSystem.current.IsPointerOverGameObject())
#elif UNITY_IOS || UNITY_ANDROID
        if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
#else
        if (EventSystem.current.IsPointerOverGameObject())
#endif
        {
            //Debug.Log("on UI, return " + EventSystem.current.currentSelectedGameObject.name);
            return;
        }
        if (GameManager.instance.isPutFlag == false)
        {
            //如果game over了就不做事
            if (GameManager.instance.isGameOver)
            {
                return;
            }
            
            //Debug.Log("初始化前 type = " + type);
            //如果是第一格點擊，初始化炸彈和師傅
            if (GameManager.instance.isFirstStep)
            {
                GameManager.instance.SetMaster();
                GameManager.instance.SetBomb(matrixX, matrixY);

                GameManager.instance.isFirstStep = false;
            }
            //Debug.Log("初始化後 type = " + type);

            if(visibleType == TileType.Flag)
            {
                return;
            }

            //如果這格是師傅，開啟
            if (type == TileType.Master)
            {
                if(visibleType == TileType.Empty)
                {
                    catchMaster = true;
                    //翻開顯示師傅
                    GameManager.instance.getMasterCount++;
                    UIManager.instance.SetMasterText(GameManager.instance.getMasterCount, GameManager.instance.masterCount);
                    SoundManager.instance.PlayFoundMaster();
                    ShowThisFloor();
                    UIManager.instance.OpenMasterJump();
                    GameManager.instance.CheckGameOver();
                }
                
                return;
            }

            if(visibleType == TileType.Open)
            {
                return;
            }

            //如果點擊的不是陰影，GameOver
            if (inShadow == false)
            {
                GameManager.instance.GetBomb();
                return;
            }

            //如果這格是炸彈，就GameOver
            if (type == TileType.Bomb)
            {
                ShowThisFloor();
                //呼叫GameManager的GameOver
                GameManager.instance.GetBomb();
                return;
            }
            

            //翻開
            GameManager.instance.Reveal(matrixX, matrixY);
        }
        else
        {
            //插旗
            if(visibleType == TileType.Empty)
            {
                visibleType = TileType.Flag;
                if (typeTextures[(int)type] != null)
                {
                    //tileTexture.material.mainTexture = typeTextures[(int)TileType.Flag];
                }
                else
                {
                    //tileTexture.gameObject.SetActive(false);
                }

                egg.SetActive(true);

                if (type == TileType.Bomb)
                {
                    //這格是炸彈，插旗炸彈數增加
                    GameManager.instance.flaggedBombCount++;
                }

                GameManager.instance.UIBombCount++;
                UIManager.instance.SetFlaggedText(GameManager.instance.UIBombCount, GameManager.instance.bombCount);
                SoundManager.instance.PlaySetFlag();
                GameManager.instance.CheckGameOver();
            }
            else if(visibleType == TileType.Flag)
            {
                visibleType = TileType.Empty;
                if (typeTextures[(int)type] != null)
                {
                    //tileTexture.material.mainTexture = typeTextures[(int)TileType.Empty];
                }
                else
                {
                    //tileTexture.gameObject.SetActive(false);
                }

                egg.SetActive(false);

                if (type == TileType.Bomb)
                {
                    //這格是炸彈，拔旗要把插旗炸彈數扣回來
                    GameManager.instance.flaggedBombCount--;
                }
                GameManager.instance.UIBombCount--;
                UIManager.instance.SetFlaggedText(GameManager.instance.UIBombCount, GameManager.instance.bombCount);
            }
            
        }

        
    }

    //重置格子
    public void ResetFloor()
    {
        type = TileType.Empty;
        visibleType = TileType.Empty;
        tileTexture.gameObject.SetActive(true);
        tileTexture.material.mainTexture = typeTextures[(int)TileType.Empty];
        egg.SetActive(false);
        catchMaster = false;
    }

    //顯示這格是什麼
    public void ShowThisFloor()
    {
        visibleType = TileType.Open;
        //Debug.Log("typeTextures[(int)type] = " + typeTextures[(int)type]);
        if(visibleType == TileType.Open && type == TileType.Empty)
        {
            tileTexture.gameObject.SetActive(false);
        }
        else if (typeTextures[(int)type] != null)
        {
            egg.SetActive(false);
            tileTexture.material.mainTexture = typeTextures[(int)type];
        }
        else
        {
            Debug.Log("typeTextures[(int)type] = " + typeTextures[(int)type]);
            tileTexture.gameObject.SetActive(false);
        }

    }

    //換成反白數字
    public void ChangeToHighlightNumber()
    {
        if (highlightNumbers[(int)type] != null)
        {
            tileTexture.material.mainTexture = highlightNumbers[(int)type];
        }
            
    }

    //設定陰影
    public void SetShadowColor()
    {
        if(inShadow)
        {
            tileTexture.material.color = shadowColor;
            buttomTexture.material.color = shadowColor;
            eggTexture.material.color = shadowColor;
        }
        else
        {
            tileTexture.material.color = normalColor;
            buttomTexture.material.color = normalColor;
            eggTexture.material.color = normalColor;
        }
    }

}
