using UnityEngine;

public class Mouse_Kontrolü:MonoBehaviour
{
    public Camera cam;
    public float kontrol = 1f;
    public float döndür=0f;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        float dönüş_takibi = Input.GetAxis("Mouse X") * kontrol;
        döndür = döndür + dönüş_takibi;
        cam.transform.rotation = Quaternion.Euler(0, döndür, 0);
    }
    
}
