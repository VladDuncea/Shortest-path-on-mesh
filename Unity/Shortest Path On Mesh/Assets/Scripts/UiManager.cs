using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    private AppManager appManager;

    [SerializeField]
    private GameObject grupInitializare, grupAlgoritm, grupButoaneAlege, fereastraIesire, dropdownAlgoritm, butonStop;

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

    public void AfiseazaFereastraIesire()
    {
        fereastraIesire.SetActive(true);
    }

    public void ClickMeniu()
    {
        SceneManager.LoadScene("Meniu");
    }

    public void ClickInapoi()
    {
        fereastraIesire.SetActive(false);
    }

    public TipAlgoritm AlgoritmAles()
    {
        int optiune = dropdownAlgoritm.GetComponent<TMP_Dropdown>().value;

        switch(optiune)
        {
            case 0:
                return TipAlgoritm.Backtracking;
            case 1:
                return TipAlgoritm.Dijkstra;
            case 2:
                return TipAlgoritm.Astar;
        }

        // In caz de eroare intoarce backtracking
        return TipAlgoritm.Backtracking;
    }

    public void UiRulare()
    {
        // Ascunde Ui
        grupInitializare.SetActive(false);

        // Ascunde doar o parte din obiecte
        for(int i = 0; i < grupAlgoritm.transform.childCount; ++i) {
            Transform child = grupAlgoritm.transform.GetChild(i);
            if (child.name != "LabelPasi" && child.name != "PasiPeSecunda")
                child.gameObject.SetActive(false);
        }

        // Afiseaza buton stop
        butonStop.SetActive(true);
    }

    public void UiStandard()
    {
        // Afiseaza Ui
        grupInitializare.SetActive(true);

        // Afisam toate obiectele copil
        for (int i = 0; i < grupAlgoritm.transform.childCount; ++i)
        {
            grupAlgoritm.transform.GetChild(i).gameObject.SetActive(true);
        }

        // Ascunde buton stop
        butonStop.SetActive(false);
    }

    public void ClickButonStop()
    {
        // Anunta manager ca e apasat stop
        appManager.Stop();

        // Afiseaza ui standard
        UiStandard();

        // Ascunde buton stop
        butonStop.SetActive(false);
    }

    public void PasiPeSecundaOnValueChanged(string valoare)
    {
        int pasi;
        if(int.TryParse(valoare, out pasi) && pasi > 0)
        {
            appManager.UpdatePasiPeSecunda(pasi);
        }
    }
}
