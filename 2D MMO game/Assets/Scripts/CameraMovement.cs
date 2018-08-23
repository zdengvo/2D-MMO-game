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
        if (CheckForMapLimit("Left"))
        {
            return;
        }

        GameManager.Instance.OnCameraMove("Left");
        Camera.main.gameObject.transform.position = new Vector3(cam.position.x - 1, cam.position.y, cam.position.z);
        GameManager.Instance.OnCameraMoveClear("Right");
    }
    private void MoveRight()
    {
        if (CheckForMapLimit("Right"))
        {
            return;
        }

        GameManager.Instance.OnCameraMove("Right");
        Camera.main.gameObject.transform.position = new Vector3(cam.position.x + 1, cam.position.y, cam.position.z);
        GameManager.Instance.OnCameraMoveClear("Left");
    }
    private void MoveUp()
    {
        if (CheckForMapLimit("Top"))
        {
            return;
        }

        GameManager.Instance.OnCameraMove("Up");
        Camera.main.gameObject.transform.position = new Vector3(cam.position.x, cam.position.y + 1, cam.position.z);
        GameManager.Instance.OnCameraMoveClear("Down");
    }
    private void MoveDown()
    {
        if (CheckForMapLimit("Bottom"))
        {
            return;
        }

        GameManager.Instance.OnCameraMove("Down");
        Camera.main.gameObject.transform.position = new Vector3(cam.position.x, cam.position.y - 1, cam.position.z);
        GameManager.Instance.OnCameraMoveClear("Up");
    }
    private void ZoomIn()
    {
        if (Camera.main.orthographicSize > 3)
        {
            Camera.main.orthographicSize -= 2;
            GameManager.Instance.OnCameraMove("ZoomIn");
        }
    }
    private void ZoomOut()
    {
        if (Camera.main.orthographicSize < 9)
        {
            Camera.main.orthographicSize += 2;
            GameManager.Instance.OnCameraMove("ZoomOut");
        }
    }
    private bool CheckForMapLimit(string side)
    {
        float camX = Camera.main.gameObject.transform.position.x;
        float camY = Camera.main.gameObject.transform.position.y;
        int camSize = (int)Camera.main.orthographicSize;

        switch (side)
        {
            case "Left":
                if (camX < camSize - 1) return true;
                break;
            case "Right":
                if (camX > BuildManager.Instance.GetMapWidth() - camSize) return true;
                break;
            case "Top":
                if (camY > BuildManager.Instance.GetMapHeight() - camSize) return true;
                break;
            case "Bottom":
                if (camY < camSize - 1) return true;
                break;
        }
        return false;
    }

    //Delete Later
    private void MoveCameraToNextHouse()
    {
        if (nextHouse < BuildManager.Instance.GetAllXpos().Length)
        {
            float x = BuildManager.Instance.GetAllXpos()[nextHouse];
            float y = BuildManager.Instance.GetAllYpos()[nextHouse];
            Camera.main.gameObject.transform.position = new Vector3(x-0.5f, y-0.5f, cam.position.z);
            nextHouse++;
            GameManager.Instance.OnCameraMove("default");
        }
        else
        {
            CheatButton.gameObject.SetActive(false);
        }
    }
}
