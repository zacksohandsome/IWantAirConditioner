using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartPage : MonoBehaviour
{
    public GameObject tellStoryPage;
    public GameObject staffPage;
    public GameObject loadingImage;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenTellStoryPage()
    {
        tellStoryPage.SetActive(true);
    }

    public void ChangeToGame()
    {
        loadingImage.SetActive(true);
        SceneManager.LoadScene("Game");
    }

    public void OpenStaffPage()
    {
        if(staffPage.activeSelf)
        {
            CloseStaffPage();
        }
        else
        {
            staffPage.SetActive(true);
        }
    }

    public void CloseStaffPage()
    {
        staffPage.SetActive(false);
    }
}
