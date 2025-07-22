using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float MouseSensitivity = 100f;
    public float topClamp = 90f;
    public float bottomClamp = -90f;

    [Header("References")]
    public Transform playerBody;

    private float xRotation = 0f;
    private float yRotation = 0f;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        // Parent'ı otomatik bul
        if (playerBody == null)
        {
            playerBody = transform.parent;
        }
        
        // Başlangıç rotasyonunu sıfırla
        transform.localRotation = Quaternion.identity;
            
        xRotation = 0f;
        yRotation = 0f;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, bottomClamp, topClamp);

        // Camera dikey hareket
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // Player yatay hareket
        if (playerBody != null)
            playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}