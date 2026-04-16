using UnityEngine;

public class BillBoard : MonoBehaviour
{
    Camera mainCamera; //현재씬의카메라
    void Start()
    {
        mainCamera = Camera.main;
        
    }
    void LateUpdate() //update 중 마지막
    {
        transform.forward = mainCamera.transform.forward;
    }
}
