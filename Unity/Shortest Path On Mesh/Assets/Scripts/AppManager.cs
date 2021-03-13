using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public class Punct
{
    public int index;
    public Vector3 coordonate;
    public List<int> vecini;
    public List<GameObject> linii;

    public Punct(int index, Vector3 coordonate)
    {
        this.index = index;
        this.coordonate = coordonate;
        vecini = new List<int>();
        linii = new List<GameObject>();
    }

    public void EliminaLinii()
    {
        // Daca nu avem linii nu facem nimic
        if (linii == null)
            return;

        foreach (GameObject linie in linii)
            Object.Destroy(linie);
    }

    public void AscundeLinii()
    {
        // Daca nu avem linii nu facem nimic
        if (linii == null)
            return;

        foreach (GameObject linie in linii)
            linie.SetActive(false);
    }
}

public class PointsData
{
    private int nrPoints = 0;
    private List<Punct> points;
    private List<GameObject> scenePoints;
    private List<int> drumMinimCalculat = null;

    private int startPoint = -1, endPoint = -1;

    public float delayPasAlgoritm = 0.1f;

    // Variabile pentru alegere punct start/stop
    private bool alegePunctStart = false, alegePunctStop = false;

    // Prefab-uri
    GameObject prefabPunct, prefabLinie;

    // Materiale 
    private Material defaultPointMaterial, startPointMaterial, endPointMaterial;

    // Evenimente
    public event ClickPePunct eventPunctAles;

    public PointsData(GameObject prefabPunct, GameObject prefabLinie, Material startPointMaterial, Material endPointMaterial)
    {
        points = new List<Punct>();
        scenePoints = new List<GameObject>();

        this.startPointMaterial = startPointMaterial;
        this.endPointMaterial = endPointMaterial;

        this.prefabPunct = prefabPunct;
        this.prefabLinie = prefabLinie;
    }

    private void AddPoint(Vector3 punct)
    {
        points.Add(new Punct(nrPoints,punct));
        
        // Construim punctul
        GameObject scenePoint = Object.Instantiate(prefabPunct, punct, prefabPunct.transform.rotation);

        // Daca nu avem materialul default, il memoram
        if(defaultPointMaterial == null)
        {
            defaultPointMaterial = scenePoint.GetComponent<MeshRenderer>().material;
        }

        // Setam indexul punctului
        scenePoint.GetComponent<PointScript>().index = nrPoints;

        // Ne inscriem la evenimentul de click pe punct
        scenePoint.GetComponent<PointScript>().clickPePunct += clickPePunct;

        // Adaugam punctul in scena
        scenePoints.Add(scenePoint);

        // Crestem counterul pentru numarul de puncte
        nrPoints++;
    }

    private void clickPePunct(int indexPunct)
    {
        if(alegePunctStart)
        {
            SetStartPoint(indexPunct);
            alegePunctStart = false;

            // Notifica interfata ca s-a ales punctul
            eventPunctAles?.Invoke(indexPunct);
        }
        else if(alegePunctStop)
        {
            SetEndPoint(indexPunct);
            alegePunctStop = false;
            eventPunctAles?.Invoke(indexPunct);
        }
    }

    public List<Punct> GetPoints()
    {
        return points;
    }

    public void SetStartPoint(int pointIndex)
    {
        // verificam daca exista inainte un punct de start
        if(startPoint != -1)
        {
            scenePoints[startPoint].GetComponent<MeshRenderer>().material = defaultPointMaterial;
        }

        startPoint = pointIndex;
        scenePoints[startPoint].GetComponent<MeshRenderer>().material = startPointMaterial;
    }

    public void SetEndPoint(int pointIndex)
    {
        if (endPoint != -1)
        {
            scenePoints[endPoint].GetComponent<MeshRenderer>().material = defaultPointMaterial;
        }

        endPoint = pointIndex;
        scenePoints[endPoint].GetComponent<MeshRenderer>().material = endPointMaterial;
    }

    public int GetEndPoint()
    {
        return endPoint;
    }

    public void EliminaElementeDesenate()
    {
        // Sterge liniile
        foreach (Punct punct in points)
        {
            punct.EliminaLinii();
        }

        // Sterge punctele
        foreach ( GameObject punctScena in scenePoints)
        {
            Object.Destroy(punctScena);
        }
    }

    public void ReadPointsFromFile(string path)
    {
        // Deschidem un StreamReader cu ajutorul caruia vom citi rand cu rand datele despre puncte
        StreamReader reader = new StreamReader(path);

        // Citim numarul de puncte
        int nrPoints = int.Parse(reader.ReadLine());

        // Pentru fiecare punct ii citim coordonatele
        for (int i = 0; i < nrPoints; i++)
        {
            // Citim linia
            string linie = reader.ReadLine();
            // Spargem linia dupa spatii
            string[] coordonate = linie.Split(' ');

            // Adaugam punctul
            // Vom pune in ordinea 0,2,1 pt ca y este aici axa verticala
            this.AddPoint(new Vector3(int.Parse(coordonate[0]), int.Parse(coordonate[2]), int.Parse(coordonate[1])));
        }

        // Ultimele linii sunt vecinii fiecarui punct
        // Index punct | nr vecini | index vecini
        while (!reader.EndOfStream)
        {
            string linie = reader.ReadLine();
            string[] liniaSparta = linie.Split(' ');
            int pointIndex = int.Parse(liniaSparta[0]);
            int nrVecini = int.Parse(liniaSparta[1]);

            for (int i = 0; i < nrVecini; i++)
            {
                int indexVecin = int.Parse(liniaSparta[2 + i]);
                // Adaugam in lista noastra daca nu e deja
                if (!points[pointIndex].vecini.Contains(indexVecin))
                {
                    points[pointIndex].vecini.Add(indexVecin);
                    points[pointIndex].linii.Add(null);
                }
                // Adaugam in lista vecinului daca nu e deja
                if (!points[indexVecin].vecini.Contains(pointIndex))
                {
                    points[indexVecin].vecini.Add(pointIndex);
                    points[indexVecin].linii.Add(null);

                }
            }
        }

        // Afisare linii intre puncte
        AfisareLinii();

        // Inchidem fisierul
        reader.Close();
    }

    private void AfisareLinii()
    {
        // Mergem peste fiecare punct
        foreach(Punct punct in points)
        {
            // Verificare punct fara vecini
            if (punct.vecini == null)
                continue;

            // Mergem peste fiecare vecin
            int nrVecini = punct.vecini.Count;
            for (int i =0; i< nrVecini; i++)
            {
                // Verificam ca linia sa nu fie deja trasata
                if (punct.linii[i] != null)
                    continue;

                // Memoram vecinul
                Punct vecin = points[punct.vecini[i]];

                // Construim linia
                GameObject linie = Object.Instantiate(prefabLinie);
                // Memoram linia la noi
                punct.linii[i] = linie;
                // Memoram linia la vecin
                int indexVecin = vecin.vecini.FindIndex(x => x == punct.index);
                vecin.linii[indexVecin] = linie;
                // Plasam linia intre cele doua puncte
                linie.GetComponent<LineRenderer>().SetPosition(0, punct.coordonate);
                linie.GetComponent<LineRenderer>().SetPosition(1, points[punct.vecini[i]].coordonate);
            }
        }
    }

    public void AscundeLiniile()
    {
        // Sterge liniile
        foreach (Punct punct in points)
        {
            punct.AscundeLinii();
        }
    }

    public IEnumerator AplicaBacktracking()
    {
        if (!GataDeAlgoritm())
            yield break;

        // Stergem fostul drum
        drumMinimCalculat = null;

        int[] drumInitial = { startPoint };
        
        Stack<DateBacktrck> stivaBacktracking = new Stack<DateBacktrck>();

        // Adaugam starea initiala in stiva
        stivaBacktracking.Push(new DateBacktrck(startPoint,0,drumInitial.ToList()));

        // Date necesare pentru pastrarea drumului minim
        double costMinim = Mathf.Infinity;
        List<int> drumMinim = null;

        while(stivaBacktracking.Count != 0)
        {
            
            // Scoatem din stiva datele necesare
            DateBacktrck dateCurente = stivaBacktracking.Pop();
            int nodCurent = dateCurente.nodCurent;
            double cost = dateCurente.cost;
            List<int> drum = dateCurente.drum;


            if (nodCurent == endPoint)
            {
                // am ajuns la destinatie, verificam daca drumul este mai bun
                if(cost < costMinim)
                {
                    costMinim = cost;
                    drumMinim = new List<int>(drum);
                }
                continue;
            }

            //incercam sa avansam pe toti vecinii disponibli(dar nu pe cei pe care am fost deja)
            int nrVecini = points[nodCurent].vecini.Count;
            for (int i = 0; i < nrVecini; i++)
            {
                int vecin = points[nodCurent].vecini[i];
                //verificam daca am mai fost in acest nod
                if (drum.Contains(vecin))
                {
                    //vecinul e deja in drum, il sarim
                    continue;
                }

                List<int> drumNou = new List<int>(drum);
                // marcam ca am fost in nodul curent
                drumNou.Add(vecin);

                //TODO asteapta inainte sa afisezi linie
                AfisareDrum(drumNou);
                yield return new WaitForSeconds(delayPasAlgoritm);

                //continuam cautarea
                stivaBacktracking.Push(new DateBacktrck(vecin, cost + Vector3.Distance(points[nodCurent].coordonate, points[vecin].coordonate), drumNou));
            }
        }

        drumMinimCalculat = drumMinim;
        AfisareDrum();
    }

    public void AfisareDrum(List<int> drum = null)
    {
        // Daca nu avem inca un drum nu facem nimic
        if(drum == null && drumMinimCalculat == null)
        {
            return;
        }

        AscundeLiniile();

        // Daca userul nu seteaza un drum il alegem noi
        if(drum == null)
        {
            drum = drumMinimCalculat;
        }

        int lungimeDrum = drum.Count;

        // Afisam linia de la nodul curent la nodul urmator
        for (int i = 0; i < lungimeDrum - 1; i++)
        {
            // Cautam pozitia nodului viitor in vecinii nodului curent
            int pozVecin = points[drum[i]].vecini.FindIndex(x => x == drum[i + 1]);

            // Afisam linia
            points[drum[i]].linii[pozVecin].SetActive(true);
        }
    }

    public void AlegeStart()
    {
        alegePunctStart = true;
    }

    public void AlegeStop()
    {
        alegePunctStop = true;
    }

    public bool GataDeAlgoritm()
    {
        if(startPoint == -1 || endPoint == -1)
        {
            return false;
        }

        return true;
    }
}

#region Backtracking
public class DateBacktrck
{
    public int nodCurent;
    public double cost;
    public List<int> drum;

    public DateBacktrck(int nodCurent, double cost, List<int> drum)
    {
        this.nodCurent = nodCurent;
        this.cost = cost;
        this.drum = drum;
    }

    public void printDrum()
    {
        if (cost == -1)
        {
            Debug.Log("Nu exista drum intre aceste noduri!");
            return;
        }
        Debug.Log("Costul drumului: " + cost);
        foreach (int nod in drum)
        {
            Debug.Log(nod);
        }
    }
}

#endregion Backtracking


public class AppManager : MonoBehaviour
{

    #region VariabileGlobale

    public GameObject prefabPunct;
    public GameObject prefabLinie;
    public GameObject dropdownFisiere;

    public Material startPointMaterial;
    public Material endPointMaterial;

    private PointsData data = null;

    [SerializeField]
    private float delayPasAlgoritm = 0.5f;

    // Interfata
    private UiManager uiManager;

    #endregion

    void Start()
    {
        uiManager = FindObjectOfType<UiManager>();
        // Populeaza cu date dropdown fisiere
        PopulareDropdown();
    }

    // Update is called once per frame
    void Update()
    {
        // La apasare esc iesi
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void PopulareDropdown()
    {
        // Cale de baza
        string basePath = Application.streamingAssetsPath;
        // Cautam toate fisierele .txt din Input
        string[] numeFisiere = Directory.GetFiles(basePath + "/Input","*.txt");
        // Aratam utilizatorului doar denumirea, nu si calea fisierului
        for(int i = 0;i < numeFisiere.Length;i++)
        {
            numeFisiere[i] = Path.GetFileName(numeFisiere[i]);
        }

        TMP_Dropdown dropdown = dropdownFisiere.GetComponent<TMP_Dropdown>();

        dropdown.AddOptions(numeFisiere.ToList());

    }

    public void GenereazaDate()
    {
        if (data != null)
            data.EliminaElementeDesenate();

        // Construim obiectul care va tine datele
        data =  new PointsData(prefabPunct, prefabLinie, startPointMaterial, endPointMaterial);

        // setare delay pas algoritm
        data.delayPasAlgoritm = delayPasAlgoritm;

        // Inscriere la evenimentul de click pe punct
        data.eventPunctAles += eventPunctAles;

        // Folosim dropdown-ul pentru a alege fisierul
        TMP_Dropdown dropdown = dropdownFisiere.GetComponent<TMP_Dropdown>();
        string numeFisier = dropdown.options[dropdown.value].text;
        // Cale de baza
        string basePath = Application.streamingAssetsPath;

        // Citim datele
        data.ReadPointsFromFile(basePath + "/Input/" + numeFisier);
    }

    private void eventPunctAles(int indexPunct)
    {
        // Anunta interfata ca s-a ales un punct
        uiManager.AfiseazaUI();
    }

    public void BacktrackingClick()
    {
        if (!data.GataDeAlgoritm())
        {
            // Notifica utilizatorul ca nu e gata de algoritm
            uiManager.AfiseazaPopupOk("Nu ati ales punctul de start si/sau de final");
            return;
        }

        // Ascundem liniile
        data.AscundeLiniile();

        // Aplicam Backtracking
        StartCoroutine(data.AplicaBacktracking());
    }

    public void AfiseazaDrum()
    {
        data?.AfisareDrum();
    }

    public void AlegeStart()
    {
        data?.AlegeStart();
    }

    public void AlegeStop()
    {
        data?.AlegeStop();
    }
}
