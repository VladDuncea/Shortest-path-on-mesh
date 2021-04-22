using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public enum TipAlgoritm
{
    Backtracking,
    Dijkstra,
    Astar
}

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

    // Culori
    Color culoareDefaultLinie;

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

        // Luam culoarea default a liniei
        culoareDefaultLinie = prefabLinie.GetComponent<LineRenderer>().startColor;
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
            this.AddPoint(new Vector3(float.Parse(coordonate[0]), float.Parse(coordonate[2]), float.Parse(coordonate[1])));
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

    public IEnumerator AplicaBacktracking(System.Action callback)
    {
        if (!GataDeAlgoritm())
            yield break;

        // Reseteaza culorile
        ResetCuloare();

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

        callback?.Invoke();
    }

    public IEnumerator AplicaDijkstra(System.Action callback)
    {
        if (!GataDeAlgoritm())
            yield break;

        // Reseteaza culorile
        ResetCuloare();

        // Date necesare pt algoritm
        List<int> parinte = Enumerable.Repeat(-1, points.Count).ToList();
        List<double> distanta = Enumerable.Repeat(double.PositiveInfinity, points.Count).ToList();
        List<bool> vizitat = Enumerable.Repeat(false, points.Count).ToList();

        // Introducem nodul de start
        // TODO posib sa mergi invers
        List<DateDinamica> priorityQueue = new List<DateDinamica>();
        priorityQueue.Add(new DateDinamica(startPoint, 0));
        // Setam distanta de la nodul de start cu 0
        distanta[startPoint] = 0;

        // Rulam cat avem noduri nedescoperite in lista
        while(priorityQueue.Count != 0)
        {
            // Extragem nodul din queue
            int nod = priorityQueue[0].nodCurent;
            double costNod = priorityQueue[0].cost;
            priorityQueue.RemoveAt(0);

            // Avand in vedere ca nu eliminam nodurile cu un cost mai bun verificam sa nu avem dubluri
            if(vizitat[nod])
            {
                // Ignoram
                continue;
            }

            // Marcam nodul ca vizitat
            vizitat[nod] = true;

            // Verificam daca nodul extras este nodul scop
            if(nod == endPoint)
            {
                // Oprim algoritmul
                break;
            }

            // Avansam toate nodurile vecine care nu au fost deja calculate
            int nrVecini = points[nod].vecini.Count;
            for (int i = 0; i < nrVecini; i++)
            {
                int vecin = points[nod].vecini[i];
                //verificam daca am mai fost in acest nod
                if (vizitat[vecin])
                {
                    //vecinul e deja in drum, il sarim
                    continue;
                }

                // Calculam costul
                double costVecin = costNod + Vector3.Distance(points[nod].coordonate, points[vecin].coordonate);

                // Verificam daca costul e mai bun decat ceva ce am gasit deja
                if(costVecin >= distanta[vecin])
                {
                    // Ignoram
                    continue;
                }

                // Actualizam noua distanta
                distanta[vecin] = costVecin;
                // Actualizam parintele vecinului
                parinte[vecin] = nod;

                // Afisam noua linie
                int indexVecin = points[nod].vecini.FindIndex(x => x == vecin);
                points[nod].linii[indexVecin].SetActive(true);
                // Asteptam durata unui pas
                yield return new WaitForSeconds(delayPasAlgoritm);

                // Construim obiectul cu datele
                DateDinamica dateDinamica = new DateDinamica(vecin, costVecin);

                // Inseram in lista ordonata la pozitia potrivita
                int poz = priorityQueue.BinarySearch(dateDinamica);
                // Nu exista un nod cu costul acesta, asa ca facem complementul binar pentru a afla pozitia pe care il inseram pentru a pastra lista sortata
                if (poz < 0)
                    poz = ~poz;
                priorityQueue.Insert(poz, dateDinamica);
            }

        }

        // Verificam daca am gasit drum
        if(distanta[endPoint] != double.PositiveInfinity)
        {
            // Construim drumul
            List<int> drum = new List<int>();
            int nodDrum = endPoint;
            drum.Add(nodDrum);
            while (parinte[nodDrum] != -1)
            {
                nodDrum = parinte[nodDrum];
                drum.Add(nodDrum);
            }
            drum.Reverse();

            // Memoram drumul
            drumMinimCalculat = drum;

            // Afisam drumul
            AfisareDrum(doarCuloare: true);
        }

        callback?.Invoke();
    }

    public IEnumerator AplicaAStar(System.Action callback, System.Func<Vector3,Vector3,float> aproximare)
    {
        if (!GataDeAlgoritm())
            yield break;

        // Reseteaza culorile
        ResetCuloare();

        // Introducem nodul de start
        List<DateAStar> priorityQueue = new List<DateAStar>();
        priorityQueue.Add(new DateAStar(startPoint, 0, 0));

        // Lista cu nodurile vizitate pentru a le ignora ulterior
        List<bool> noduriVizitate = Enumerable.Repeat(false, points.Count).ToList();

        // Nodul de final
        DateAStar final = null;

        // Rulam cat avem noduri nedescoperite in lista
        while (priorityQueue.Count != 0)
        {
            // Extragem nodul din queue
            DateAStar nodExtras = priorityQueue[0];
            priorityQueue.RemoveAt(0);

            // Verificam daca nodul extras este nodul scop
            if (nodExtras.nodCurent == endPoint)
            {
                // Am terminat
                final = nodExtras;
                break;
            }

            // Avand in vedere ca nu eliminam nodurile cu un cost mai bun verificam sa nu avem dubluri
            if (noduriVizitate[nodExtras.nodCurent])
            {
                // Ignoram
                continue;
            }

            // Marcam nodul ca vizitat
            noduriVizitate[nodExtras.nodCurent] = true;

            // Avansam toate nodurile vecine
            int nrVecini = points[nodExtras.nodCurent].vecini.Count;
            for (int i = 0; i < nrVecini; i++)
            {
                int vecin = points[nodExtras.nodCurent].vecini[i];

                // verificam daca nodul e in lista de vizitate
                if (noduriVizitate[vecin])
                {
                    //vecinul e deja in drum, il sarim
                    continue;
                }

                // Construim datele pentru vecin
                DateAStar dateVecin = new DateAStar(vecin, nodExtras.cost, -1);

                // Calculam costul
                dateVecin.cost += Vector3.Distance(points[nodExtras.nodCurent].coordonate, points[vecin].coordonate);

                // Calculam aproximarea
                dateVecin.aproximat = aproximare(points[vecin].coordonate, points[endPoint].coordonate);

                // Setam parintele nodului
                dateVecin.parinte = nodExtras;

                // Afisam noua linie
                int indexVecin = points[nodExtras.nodCurent].vecini.FindIndex(x => x == vecin);
                points[nodExtras.nodCurent].linii[indexVecin].SetActive(true);

                // Asteptam durata unui pas
                yield return new WaitForSeconds(delayPasAlgoritm);

                // Inseram in lista ordonata la pozitia potrivita
                int poz = priorityQueue.BinarySearch(dateVecin);
                // Nu exista un nod cu costul acesta, asa ca facem complementul binar pentru a afla pozitia pe care il inseram pentru a pastra lista sortata
                if (poz < 0)
                    poz = ~poz;
                priorityQueue.Insert(poz, dateVecin);
            }

        }

        // Verificam daca am gasit drum
        if (final != null)
        {
            // Construim drumul
            List<int> drum = new List<int>();

            drum.Add(final.nodCurent);
            while (final.parinte != null)
            {
                final = final.parinte;
                drum.Add(final.nodCurent);
            }
            drum.Reverse();

            // Memoram drumul
            drumMinimCalculat = drum;

            // Afisam drumul
            AfisareDrum(doarCuloare: true);
        }

        callback?.Invoke();
    }

    public void AfisareDrum(List<int> drum = null, bool doarCuloare = false)
    {
        // Daca nu avem inca un drum nu facem nimic
        if(drum == null && drumMinimCalculat == null)
        {
            return;
        }

        // Ascundem liniile deja desenate doar in cazul in care nu vrem culoare
        if(!doarCuloare)
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

            if (doarCuloare)
            {
                // Coloram linia
                points[drum[i]].linii[pozVecin].GetComponent<LineRenderer>().startColor = Color.green;
                points[drum[i]].linii[pozVecin].GetComponent<LineRenderer>().endColor = Color.green;
            }
            else
            {
                // Afisam linia
                points[drum[i]].linii[pozVecin].SetActive(true); 
            }
        }
    }

    private void ResetCuloare()
    {
        // Pentru fiecare punct
        foreach (Punct p in points)
        {
            // Pentru fiecare linie
            foreach (GameObject linie in p.linii)
            {
                // Coloram linia
                linie.GetComponent<LineRenderer>().startColor = culoareDefaultLinie;
                linie.GetComponent<LineRenderer>().endColor = culoareDefaultLinie;
            }
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

    public void UpdateDelay(float delay)
    {
        delayPasAlgoritm = delay;
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

#region Dijkstra

public class DateDinamica : System.IComparable<DateDinamica>
{
    public int nodCurent;
    public double cost;

    public DateDinamica(int nodCurent, double cost)
    {
        this.nodCurent = nodCurent;
        this.cost = cost;
    }

    public int CompareTo(DateDinamica other)
    {
        return this.cost.CompareTo(other.cost);
    }
}

#endregion

#region AStar

public class DateAStar : System.IComparable<DateAStar>
{
    public int nodCurent;
    public double cost,aproximat;
    public DateAStar parinte;

    public DateAStar(int nodCurent, double cost, double aproximat)
    {
        this.nodCurent = nodCurent;
        this.cost = cost;
        this.aproximat = aproximat;
        parinte = null;
    }

    public int CompareTo(DateAStar other)
    {
        double total, totalvs;
        total = cost + aproximat;
        totalvs = other.cost + other.aproximat;
        return total.CompareTo(totalvs);
    }
}

#endregion

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
        // La apasare esc afiseaza fereastra iesire
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            uiManager.AfiseazaFereastraIesire();
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

        // Aplicam Algoritm
        if (uiManager.AlgoritmAles() == TipAlgoritm.Backtracking)
        {
            StartCoroutine(data.AplicaBacktracking(FinalCorutina));
        }
        else if (uiManager.AlgoritmAles() == TipAlgoritm.Dijkstra)
        {
            StartCoroutine(data.AplicaDijkstra(FinalCorutina));
        }
        else if (uiManager.AlgoritmAles() == TipAlgoritm.Astar)
        {
            StartCoroutine(data.AplicaAStar(FinalCorutina,Vector3.Distance));
        }

        uiManager.UiRulare();
    }

    private void FinalCorutina()
    {
        uiManager.UiStandard();
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

    public void Stop()
    {
        StopAllCoroutines();
    }

    public void UpdatePasiPeSecunda(float pasi)
    {
        delayPasAlgoritm = 1 / pasi;
        data?.UpdateDelay(delayPasAlgoritm);
    }
}
