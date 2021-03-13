using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    private AppManager appManager;

    [SerializeField]
    private GameObject grupInitializare, grupAlgoritm, grupButoaneAlege;

    [SerializeField]
    private GameObject popupOk;
    [SerializeField]
    private TextMeshProUGUI textPopupOk;

    // Start is called before the first frame update
    void Start()
    {
        appManager = FindObjectOfType<AppManager>();
    }

    public void AlegePunctStartClick()
    {
        appManager.AlegeStart();
        AscundeUI();
    }

    public void AlegePunctStopClick()
    {
        appManager.AlegeStop();
        AscundeUI();
    }

    public void AscundeUI()
    {
        grupAlgoritm.SetActive(false);
        grupInitializare.SetActive(false);
    }

    public void AfiseazaUI()
    {
        grupAlgoritm.SetActive(true);
        grupInitializare.SetActive(true);
    }

    public void AfiseazaPopupOk(string text)
    {
        AscundeUI();
        textPopupOk.text = text;
        popupOk.SetActive(true);
    }

    public void PopupOkClick()
    {
        popupOk.SetActive(false);
        AfiseazaUI();
    }

    public void ClickAfiseaza()
    {
        appManager.GenereazaDate();
        grupButoaneAlege.SetActive(true);
        grupAlgoritm.SetActive(true);
    }
}
