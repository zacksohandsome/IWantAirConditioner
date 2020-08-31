using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    public Camera camera;
    public GameObject cameraRoot;

    public Vector3 cameraOriginPosition = new Vector3(5, 10, -5);
    public float cameraNowX = 0;
    public float cameraMinX = -5;
    public float cameraMaxX = 5;
    public float cameraMoveDistance = 1;
    public float cameraOriginSize = 10;
    public float cameraNowSize = 10;
    public float cameraMinSize = 2;
    public float cameraMaxSize = 15;
    public float cameraZoomDistance = 0.5f;
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
    public void CameraReset()
    {
        camera.transform.localPosition = cameraOriginPosition;
        camera.orthographicSize = cameraOriginSize;
    }

    //左移
    public void CameraMoveLeft()
    {
        cameraNowX -= cameraMoveDistance;
        if(cameraNowX < cameraMinX)
        {
            cameraNowX = cameraMinX;
            return;
        }
        if(cameraNowX > cameraMaxX)
        {
            cameraNowX = cameraMaxX;
            return;
        }
        camera.transform.localPosition = new Vector3(cameraNowX, camera.transform.localPosition.y, camera.transform.localPosition.z - cameraMoveDistance);
    }

    //右移
    public void CameraMoveRight()
    {
        cameraNowX += cameraMoveDistance;
        if (cameraNowX < cameraMinX)
        {
            cameraNowX = cameraMinX;
            return;
        }
        if (cameraNowX > cameraMaxX)
        {
            cameraNowX = cameraMaxX;
            return;
        }
        camera.transform.localPosition = new Vector3(cameraNowX, camera.transform.localPosition.y, camera.transform.localPosition.z + cameraMoveDistance);
    }

    //放大
    public void CameraZoomIn()
    {
        cameraNowSize -= cameraZoomDistance;
        if (cameraNowSize < cameraMinSize)
        {
            cameraNowSize = cameraMinSize;
        }
        if (cameraNowSize > cameraMaxSize)
        {
            cameraNowSize = cameraMaxSize;
        }
        camera.orthographicSize = cameraNowSize;
    }

    //縮小
    public void CameraZoomOut()
    {
        cameraNowSize += cameraZoomDistance;
        if (cameraNowSize < cameraMinSize)
        {
            cameraNowSize = cameraMinSize;
        }
        if (cameraNowSize > cameraMaxSize)
        {
            cameraNowSize = cameraMaxSize;
        }
        camera.orthographicSize = cameraNowSize;
    }
}
