using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private float vitezaMiscare = 5.0f;
    [SerializeField]
    private float vitezaRotatie = 50.0f;

    private bool modRotire = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    void Update()
    {
        // La fiecare frame verificam pe ce taste apasa utilizatorul
        float inputVertical = Input.GetAxis("Vertical");// folosita pt fata/spate
        float inputOrizontal = Input.GetAxis("Horizontal"); // folosita pt stanga/dreapta
        float inputZbor = Input.GetAxis("Float Axis"); // a treia axa pt a merge sus/jos

        transform.Translate(new Vector3(inputOrizontal, inputZbor, inputVertical) * vitezaMiscare * Time.deltaTime);

        // Rotim camera in functe de inputul utilizatorului
        if(modRotire)
        {
            float rotatieOrizontala = Input.GetAxis("Mouse X");
            float rotatieVerticala = Input.GetAxis("Mouse Y");

            // Privire sus jos
            transform.RotateAround(transform.position, Vector3.up , rotatieOrizontala * vitezaRotatie * Time.deltaTime);

            // Privire stanga dreapta
            transform.Rotate(Vector3.left, rotatieVerticala * vitezaRotatie * Time.deltaTime, Space.Self);
        }
        
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            modRotire = !modRotire;
            if(modRotire)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

        }
    }
}
