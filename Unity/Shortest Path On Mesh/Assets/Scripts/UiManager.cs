using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public delegate void OneFloatDelegate(float scara);

public class UiManager : MonoBehaviour
{
    private AppManager appManager;

    [SerializeField]
    private GameObject grupInitializare, grupAlgoritm, grupButoaneAlege, fereastraIesire, dropdownAlgoritm, butonStop;

    [SerializeField]
    private GameObject grupStatistici;
    [SerializeField]
    private TextMeshProUGUI textMuchii, textNoduri, textDistanta, textDurata;

    [SerializeField]
    private GameObject popupOk;
    [SerializeField]
    private TextMeshProUGUI textPopupOk;

    [SerializeField]
    private Slider scaleSlider;

    // Evenimente
    public event OneFloatDelegate scalarePunct;

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
        AscundeStatistici();
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
                return TipAlgoritm.DijkstraDual;
            case 3:
                return TipAlgoritm.Astar;
            case 4:
                return TipAlgoritm.RafinareDijkstraDual;
        }

        // In caz de eroare intoarce backtracking
        return TipAlgoritm.Backtracking;
    }

    public void UiRulare()
    {
        // Ascunde Ui
        grupInitializare.SetActive(false);
        AscundeStatistici();

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

    public void ClickButonAfisareLinii()
    {

    }

    public void PasiPeSecundaOnValueChanged(string valoare)
    {
        int pasi;
        if(int.TryParse(valoare, out pasi) && pasi > 0)
        {
            appManager.UpdatePasiPeSecunda(pasi);
        }
    }

    public void AscundeStatistici()
    {
        grupStatistici.SetActive(false);
    }

    public void AfiseazaStatistici(Statistici stats)
    {
        textNoduri.text = stats.numarNoduri.ToString();
        textMuchii.text = stats.numarMuchii.ToString();
        textDistanta.text = stats.distanta.ToString("N2");
        textDurata.text = stats.durataRulare.ToString("f6");
        grupStatistici.SetActive(true);
    }

    // Invoked when the value of the slider changes.
    public void OnSliderValueChanged()
    {
        scalarePunct?.Invoke(scaleSlider.value);
    }
}
