using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour {

    [SerializeField]
    private Button left;
    [SerializeField]
    private Button right;
    [SerializeField]
    private Button up;
    [SerializeField]
    private Button down;
    [SerializeField]
    private Button zoomIn;
    [SerializeField]
    private Button zoomOut;

    private Transform cam;

    //Delete later
    [SerializeField]
    private Button CheatButton;
    private int nextHouse;

    private void Awake()
    {
        cam = Camera.main.gameObject.transform;

        left.onClick.AddListener(MoveLeft);
        right.onClick.AddListener(MoveRight);
        up.onClick.AddListener(MoveUp);
        down.onClick.AddListener(MoveDown);
        zoomIn.onClick.AddListener(ZoomIn);
        zoomOut.onClick.AddListener(ZoomOut);

        //Delete later
        CheatButton.onClick.AddListener(MoveCameraToNextHouse);
        nextHouse = 0;
    }

    private void MoveLeft()
    {
        if (CheckForMapLimit("left"))
        {
            return;
        }

        Camera.main.gameObject.transform.position = new Vector3(cam.position.x - 1, cam.position.y, cam.position.z);
        GameManager.Instance.OnCameraMove();
    }
    private void MoveRight()
    {
        if (CheckForMapLimit("right"))
        {
            return;
        }

        Camera.main.gameObject.transform.position = new Vector3(cam.position.x + 1, cam.position.y, cam.position.z);
        GameManager.Instance.OnCameraMove();
    }
    private void MoveUp()
    {
        if (CheckForMapLimit("top"))
        {
            return;
        }

        Camera.main.gameObject.transform.position = new Vector3(cam.position.x, cam.position.y + 1, cam.position.z);
        GameManager.Instance.OnCameraMove();
    }
    private void MoveDown()
    {
        if (CheckForMapLimit("bottom"))
        {
            return;
        }

        Camera.main.gameObject.transform.position = new Vector3(cam.position.x, cam.position.y - 1, cam.position.z);
        GameManager.Instance.OnCameraMove();
    }
    private void ZoomIn()
    {
        if (Camera.main.orthographicSize > 3)
        {
            Camera.main.orthographicSize -= 1;
        }
    }
    private void ZoomOut()
    {
        if (Camera.main.orthographicSize < 8)
        {
            Camera.main.orthographicSize += 1;
        }
    }
    private bool CheckForMapLimit(string side)
    {
        float camX = Camera.main.gameObject.transform.position.x;
        float camY = Camera.main.gameObject.transform.position.y;
        int camSize = (int)Camera.main.orthographicSize;

        switch (side)
        {
            case "left":
                if (camX < camSize - 1) return true;
                break;
            case "right":
                if (camX > BuildManager.Instance.GetMapWidth() - camSize) return true;
                break;
            case "top":
                if (camY > BuildManager.Instance.GetMapHeight() - camSize) return true;
                break;
            case "bottom":
                if (camY < camSize - 1) return true;
                break;
        }
        return false;
    }

    //Delete Later
    private void MoveCameraToNextHouse()
    {
        if (nextHouse < 5)
        {
            int x = BuildManager.Instance.GetAllXpos()[nextHouse];
            int y = BuildManager.Instance.GetAllYpos()[nextHouse];
            Camera.main.gameObject.transform.position = new Vector3(x, y, cam.position.z);
            nextHouse++;
            GameManager.Instance.OnCameraMove();
        }
        else
        {
            CheatButton.gameObject.SetActive(false);
        }
    }
}
