using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ClickPePunct(int indexPunct);

public class PointScript : MonoBehaviour
{

    public int index = -1;

    public event ClickPePunct clickPePunct;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        // Anuntam subscriberii ca punctul a fost apasat
        clickPePunct?.Invoke(index);
    }
}
