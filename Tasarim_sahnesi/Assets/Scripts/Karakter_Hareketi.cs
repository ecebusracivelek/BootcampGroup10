using JetBrains.Annotations;
using UnityEngine;

public class Karakter_Hareketi : MonoBehaviour
{
    GameObject İtem;
    public bool nesne_bulundu = false;
    public bool ziplama_kontrol = false;
    [SerializeField] GameObject cube;
    [SerializeField] int çarpım = 2;
    Rigidbody rigidBody;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {

        rigidBody.useGravity = false;





        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * çarpım);

        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(Vector3.back * Time.deltaTime * çarpım);

        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(Vector3.left * Time.deltaTime * çarpım);

        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(Vector3.right * Time.deltaTime * çarpım);

        }
        if (Input.GetKeyDown(KeyCode.L) && !ziplama_kontrol)
        {

            rigidBody.useGravity = false;

            if ((Vector3.up * çarpım).magnitude < 9)
            {
                rigidBody.AddForce(Vector3.up * çarpım, ForceMode.Impulse);
            }
            ziplama_kontrol = true;
        }
        if (Input.GetMouseButtonDown(1) && nesne_bulundu)
        {

            if (İtem != null)
            {
                İtem.SetActive(false);
                nesne_bulundu = false;
            }
          
            
        }
         if (Input.GetMouseButtonDown(0) && !nesne_bulundu)
        {
            if (İtem != null)
            {
                 İtem.SetActive(true);
                
            }
           
            
            
        }


        rigidBody.useGravity = true;


    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Nesne"))
        {
            nesne_bulundu = true;
            İtem = other.gameObject;
        }
    }
    void OnTriggerExit(Collider other)
    {
        İtem = null;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Zemin"))
    {
        ziplama_kontrol = false; 
    }
        
    }


}
    

  
