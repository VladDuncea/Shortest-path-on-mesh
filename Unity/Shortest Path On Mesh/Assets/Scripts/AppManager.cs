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
    DijkstraDual,
    Astar
}

public class Fata
{
    public int nod1, nod2, nod3;

    public Fata(int nod1,int nod2,int nod3)
    {
        this.nod1 = nod1;
        this.nod2 = nod2;
        this.nod3 = nod3;
    }

    public Fata(Punct nod1, Punct nod2, Punct nod3)
    {
        this.nod1 = nod1.index;
        this.nod2 = nod2.index;
        this.nod3 = nod3.index;
    }
}

public class Punct
{
    public int index;
    public Vector3 coordonate;
    public List<int> vecini;
    public List<GameObject> linii;
    public List<Fata> fete;
    public GameObject scenePoint;

    public Punct(int index, Vector3 coordonate, GameObject scenePoint)
    {
        this.index = index;
        this.coordonate = coordonate;
        vecini = new List<int>();
        linii = new List<GameObject>();
        fete = new List<Fata>();
        this.scenePoint = scenePoint;
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

    public void scalarePunct(float scara)
    {
        // Scalare punct
        scenePoint.transform.localScale = Vector3.one * scara;
        // Scalare linii
        foreach(GameObject linie in linii)
        {
            linie.GetComponent<LineRenderer>().startWidth = scara/3;
        }
    }
}

// Statistici rulare
public class Statistici
{
    public int numarMuchii, numarNoduri;
    public List<int> drum;
    public double distanta;
    public float durataRulare;

    public Statistici()
    {
        numarMuchii = numarNoduri = 0;
        drum = null;
        distanta = -1;
        durataRulare = 0;
    }

    public Statistici(Statistici s)
    {
        numarMuchii = s.numarMuchii;
        numarNoduri = s.numarNoduri;
        if(s.drum != null)
            drum = new List<int>(s.drum);
        distanta = s.distanta;
        durataRulare = s.durataRulare;
    }
};

public class PointsData
{
    private int nrPoints = 0;
    private List<Punct> points;
    private List<GameObject> scenePoints;
    private List<int> drumMinimCalculat = null;

    private int startPoint = -1, endPoint = -1;

    public float pasiPeSecunda = 0.1f;

    // Variabile pentru alegere punct start/stop
    private bool alegePunctStart = false, alegePunctStop = false;

    // Prefab-uri
    GameObject prefabPunct, prefabLinie;

    // Culori
    Color culoareDefaultLinie;

    // Materiale 
    private Material defaultPointMaterial, startPointMaterial, endPointMaterial;

    // Statistici rulare
    private Statistici statistici;

    // Evenimente
    public event ClickPePunct eventPunctAles;

    // Referinta la ui
    UiManager UI;

    // Frecventa update vizual(in secunde)
    float frecventaUpdate = 0.25f;

    public PointsData(GameObject prefabPunct, GameObject prefabLinie, Material startPointMaterial, Material endPointMaterial, UiManager UI)
    {
        points = new List<Punct>();
        scenePoints = new List<GameObject>();

        this.startPointMaterial = startPointMaterial;
        this.endPointMaterial = endPointMaterial;

        this.prefabPunct = prefabPunct;
        this.prefabLinie = prefabLinie;

        // Luam culoarea default a liniei
        culoareDefaultLinie = prefabLinie.GetComponent<LineRenderer>().startColor;

        this.UI = UI;
    }

    private Punct AddPoint(Vector3 punct)
    {
        // Construim punctul vizual
        GameObject scenePoint = Object.Instantiate(prefabPunct, punct, prefabPunct.transform.rotation);

        // Construim clasa punct
        Punct p = new Punct(nrPoints, punct, scenePoint);
        points.Add(p);

        // Subscriem punctul la scalare
        UI.scalarePunct += p.scalarePunct;

        // Daca nu avem materialul default, il memoram
        if (defaultPointMaterial == null)
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

        return p;
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

            // Scoatem subscrierea de la eveniment
            UI.scalarePunct -= punct.scalarePunct;

            // Distrugem obiectul vizual
            Object.Destroy(punct.scenePoint);
        }
    }

    private void AddLineBetweenPoints(int p1, int p2)
    {
        // Adaugam in lista lui p1 daca nu e deja
        if (!points[p1].vecini.Contains(p2))
        {
            points[p1].vecini.Add(p2);
            points[p1].linii.Add(null);
        }
        // Adaugam in lista lui p2 daca nu e deja
        if (!points[p2].vecini.Contains(p1))
        {
            points[p2].vecini.Add(p1);
            points[p2].linii.Add(null);
        }
    }

    private void AddLineBetweenPoints(Punct p1, Punct p2)
    {
        AddLineBetweenPoints(p1.index, p2.index);
    }

    public void ReadPointsFromFile(string path)
    {
        // Deschidem un StreamReader cu ajutorul caruia vom citi rand cu rand datele despre puncte
        StreamReader reader = new StreamReader(path);

        string extensie = Path.GetExtension(path);
        if (extensie == ".txt")
            ReadPointsFromTxt(reader);
        else if (extensie == ".obj")
            ReadPointsFromObj(reader);

        // Inchidem fisierul
        reader.Close();
    }

    private void ReadPointsFromTxt(StreamReader reader)
    {
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

                // Trasam linia intre puncte
                AddLineBetweenPoints(pointIndex, indexVecin);
            }
        }

        // Afisare linii intre puncte
        AfisareLinii();
    }

    private void ReadPointsFromObj(StreamReader reader)
    {
        // Parcurgem tot fisierul si luam doar liniile pentru puncte (v ...)
        while (!reader.EndOfStream)
        {
            // Citim linia
            string linie = reader.ReadLine();
            // Spargem linia dupa spatii
            string[] sep = { " " };
            string[] bucati = linie.Split(sep, System.StringSplitOptions.RemoveEmptyEntries);

            if(bucati.Length == 0 || bucati[0] != "v")
            {
                // Sarim peste liniile care nu descriu un punct
                continue;
            }

            // Adaugam punctul
            // Vom pune in ordinea 1,3,2 pt ca y este aici axa verticala
            this.AddPoint(new Vector3(float.Parse(bucati[1]), float.Parse(bucati[2]), float.Parse(bucati[3])));
        }

        // Ne intoarcem la inceputul fisierului
        reader.BaseStream.Position = 0;

        // Parcurgem tot fisierul si luam doar liniile pentru fete (f ...)
        while (!reader.EndOfStream)
        {
            // Citim linia
            string linie = reader.ReadLine();
            // Spargem linia dupa spatii
            string[] sep = { " " };
            string[] bucati = linie.Split(sep, System.StringSplitOptions.RemoveEmptyEntries);


            if (bucati.Length == 0 || bucati[0] != "f")
            {
                // Sarim peste liniile care nu descriu o fata
                continue;
            }

            // Verificam ca fata sa fie formata din 3 puncte(acceptam doar triangulari)
            if(bucati.Length != 4)
            {
                continue;
            }

            // Separam dupa '/' si pastram doar prima valoare din fiecare set
            // Scadem 1 pentru ca in .obj indexarea punctelor incepe de la 1
            int punct1 = int.Parse(bucati[1].Split('/')[0]) - 1;
            int punct2 = int.Parse(bucati[2].Split('/')[0]) - 1;
            int punct3 = int.Parse(bucati[3].Split('/')[0]) - 1;

            // Construim o fata cu punctele acestea
            Fata f = new Fata(punct1, punct2, punct3);

            // Marcam faptul ca punctele apartin fetei
            points[punct1].fete.Add(f);
            points[punct2].fete.Add(f);
            points[punct3].fete.Add(f);

            // Construim cele 3 legaturi dintre puncte
            AddLineBetweenPoints(punct1, punct2);
            AddLineBetweenPoints(punct2, punct3);
            AddLineBetweenPoints(punct3, punct1);
        }

        // Afisare linii intre puncte
        AfisareLinii();
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

        // Memoram timpul de start
        float startTime = Time.realtimeSinceStartup;

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

        // Variabila stocare statistici rulare
        Statistici stats = new Statistici();

        int pasiFaraPauza = 0;

        while (stivaBacktracking.Count != 0)
        {
            // Numaram pasii la statistici
            stats.numarNoduri++;

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

                // Asteptam pentru 'frecventaSecunde' la numarul necesar de pasi(aici vom seta o limita maxima la 50)
                pasiFaraPauza++;
                if (pasiPeSecunda * frecventaUpdate < pasiFaraPauza || pasiFaraPauza > 50)
                {
                    pasiFaraPauza = 0;
                    yield return new WaitForSeconds(frecventaUpdate);
                }

                //continuam cautarea
                stivaBacktracking.Push(new DateBacktrck(vecin, cost + Vector3.Distance(points[nodCurent].coordonate, points[vecin].coordonate), drumNou));
            }
        }

        // Calculare timp rulare
        stats.durataRulare = Time.realtimeSinceStartup - startTime;

        // Actualizam statisticile global
        this.statistici = stats;

        drumMinimCalculat = drumMinim;
        AfisareDrum();

        callback?.Invoke();
    }

    public IEnumerator AplicaDijkstra(System.Action callback)
    {
        if (!GataDeAlgoritm())
            yield break;

        // Memoram timpul de start
        float startTime = Time.realtimeSinceStartup;

        // Reseteaza culorile
        ResetCuloare();

        // Date necesare pt algoritm
        List<int> parinte = Enumerable.Repeat(-1, points.Count).ToList();
        List<double> distanta = Enumerable.Repeat(double.PositiveInfinity, points.Count).ToList();
        List<bool> vizitat = Enumerable.Repeat(false, points.Count).ToList();

        // Introducem nodul de start
        List<DateDinamica> priorityQueue = new List<DateDinamica>();
        priorityQueue.Add(new DateDinamica(startPoint, 0));
        // Setam distanta de la nodul de start cu 0
        distanta[startPoint] = 0;

        // Variabila stocare statistici rulare
        Statistici stats = new Statistici();

        // Variabila pentru pauze
        int pasiFaraPauza = 0;

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

            // Numaram nodul la statistici
            stats.numarNoduri++;

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

                // Numaram muchia la statistici
                stats.numarMuchii++;

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

                // Asteptam pentru 'frecventaSecunde' la numarul necesar de pasi
                pasiFaraPauza++;
                if(pasiPeSecunda*frecventaUpdate < pasiFaraPauza)
                {
                    pasiFaraPauza = 0;
                    yield return new WaitForSeconds(frecventaUpdate);
                }

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

            // Memoram drumul si distanta in statistici
            stats.drum = drum;
            stats.distanta = distanta[endPoint];

            // Calculare timp rulare
            stats.durataRulare = Time.realtimeSinceStartup - startTime;

            // Memoram statisticile global
            this.statistici = stats;

            // Afisam drumul
            AfisareDrum(doarCuloare: true);
        }

        callback?.Invoke();
    }

    public IEnumerator AplicaDijkstraDual(System.Action callback)
    {
        if (!GataDeAlgoritm())
            yield break;

        // Memoram timpul de start
        float startTime = Time.realtimeSinceStartup;

        // Reseteaza culorile
        ResetCuloare();

        // Date necesare pt algoritm
        List<List<int>> parinte = new List<List<int>>();
        // Vectorul de parinti va fi dublat aici ( un vector pentru parintele de pe partea start, unul pt parintele de pe partea scop)
        parinte.Add(Enumerable.Repeat(-1, points.Count).ToList());
        parinte.Add(Enumerable.Repeat(-1, points.Count).ToList());
        List<List<double>> distanta = new List<List<double>>();
        distanta.Add(Enumerable.Repeat(double.PositiveInfinity, points.Count).ToList());
        distanta.Add(Enumerable.Repeat(double.PositiveInfinity, points.Count).ToList());
        List<int> vizitat = Enumerable.Repeat(0, points.Count).ToList(); // 0 - nevizitat | 1- vizitat de start | 2- vizitat de scop

        // Introducem nodul de start
        List<DateDinamicaDual> priorityQueue = new List<DateDinamicaDual>();
        priorityQueue.Add(new DateDinamicaDual(startPoint, 0, 1));
        priorityQueue.Add(new DateDinamicaDual(endPoint, 0, 2));
        // Setam distanta de la nodul de start cu 0
        distanta[0][startPoint] = 0;
        distanta[1][endPoint] = 0;

        // Variabila pentru a memora nodul de intersectie
        int nodMijloc = -1;

        // Variabila stocare statistici rulare
        Statistici stats = new Statistici();

        int pasiFaraPauza = 0;

        // Rulam cat avem noduri nedescoperite in lista
        while (priorityQueue.Count != 0)
        {
            // Extragem nodul din queue
            int nod = priorityQueue[0].nodCurent;
            double costNod = priorityQueue[0].cost;
            int parte = priorityQueue[0].parte;
            priorityQueue.RemoveAt(0);

            // Verificam daca nodul extras a fost vizitat de cealalta parte
            if (vizitat[nod] != 0 && vizitat[nod] != parte)
            {
                // Oprim algoritmul
                nodMijloc = nod;
                break;
            }

            // Ignoram nodurile vizitate deja de partea noastra (putem sa avem un cost mai prost in queue)
            if (vizitat[nod] ==  parte)
            {
                // Ignoram
                continue;
            }

            // Numaram nodul la statistici
            stats.numarNoduri++;

            // Marcam nodul ca vizitat
            vizitat[nod] = parte;

            // Avansam toate nodurile vecine care nu au fost deja calculate
            int nrVecini = points[nod].vecini.Count;
            for (int i = 0; i < nrVecini; i++)
            {
                int vecin = points[nod].vecini[i];
                // verificam daca am mai fost in acest nod din partea noastra
                if (vizitat[vecin] == parte)
                {
                    //vecinul e deja vizitat, il sarim
                    continue;
                }

                // Calculam costul
                double costVecin = costNod + Vector3.Distance(points[nod].coordonate, points[vecin].coordonate);

                // Numaram muchia la statistici
                stats.numarMuchii++;

                // Verificam daca costul e mai bun decat ceva ce am gasit deja
                if (costVecin >= distanta[parte-1][vecin])
                {
                    // Daca nu il ignoram
                    continue;
                }

                // Actualizam noua distanta
                distanta[parte-1][vecin] = costVecin;
                // Actualizam parintele vecinului
                parinte[parte-1][vecin] = nod;

                // Afisam noua linie
                int indexVecin = points[nod].vecini.FindIndex(x => x == vecin);
                points[nod].linii[indexVecin].SetActive(true);

                // Asteptam pentru 'frecventaSecunde' la numarul necesar de pasi
                pasiFaraPauza++;
                if (pasiPeSecunda * frecventaUpdate < pasiFaraPauza)
                {
                    pasiFaraPauza = 0;
                    yield return new WaitForSeconds(frecventaUpdate);
                }

                // Construim obiectul cu datele
                DateDinamicaDual dateDinamica = new DateDinamicaDual(vecin, costVecin, parte);

                // Inseram in lista ordonata la pozitia potrivita
                int poz = priorityQueue.BinarySearch(dateDinamica);
                // Nu exista un nod cu costul acesta, asa ca facem complementul binar pentru a afla pozitia pe care il inseram pentru a pastra lista sortata
                if (poz < 0)
                    poz = ~poz;
                priorityQueue.Insert(poz, dateDinamica);
            }

        }

        // Verificam daca am gasit drum
        if (nodMijloc != -1)
        {
            // Construim drumul
            List<int> drum = new List<int>();

            // Incepem din nodul de mijloc spre final
            int nodDrum = nodMijloc;
            drum.Add(nodDrum);
            while (parinte[1][nodDrum] != -1)
            {
                nodDrum = parinte[1][nodDrum];
                drum.Add(nodDrum);
            }
            // Invesam ordinea(nodul de sfarsit va fi primul in vector)
            drum.Reverse();

            // Mergem din mijloc spre inceput
            nodDrum = nodMijloc;
            while (parinte[0][nodDrum] != -1)
            {
                nodDrum = parinte[0][nodDrum];
                drum.Add(nodDrum);
            }
            // Invesam ordinea (nodul de inceput va fi primul in vector)
            drum.Reverse();

            // Memoram drumul
            drumMinimCalculat = drum;

            // Memoram drumul si distanta in statistici
            stats.drum = drum;
            stats.distanta = distanta[0][nodMijloc] + distanta[1][nodMijloc];

            // Calculare timp rulare
            stats.durataRulare = Time.realtimeSinceStartup - startTime;

            // Memoram statisticile global
            this.statistici = stats;

            // Afisam drumul
            AfisareDrum(doarCuloare: true);
        }

        callback?.Invoke();
    }

    public IEnumerator AplicaAStar(System.Action callback, System.Func<Vector3,Vector3,float> aproximare)
    {
        if (!GataDeAlgoritm())
            yield break;

        // Memoram timpul de start
        float startTime = Time.realtimeSinceStartup;

        // Reseteaza culorile
        ResetCuloare();

        // Introducem nodul de start
        List<DateAStar> priorityQueue = new List<DateAStar>();
        priorityQueue.Add(new DateAStar(startPoint, 0, 0));

        // Lista cu nodurile vizitate pentru a le ignora ulterior
        List<bool> noduriVizitate = Enumerable.Repeat(false, points.Count).ToList();

        // Nodul de final
        DateAStar final = null;

        // Variabila stocare statistici rulare
        Statistici stats = new Statistici();

        int pasiFaraPauza = 0;

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

            // Numaram nodul la statistici
            stats.numarNoduri++;

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

                // Numaram muchia la statistici
                stats.numarMuchii++;

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

                // Asteptam pentru 'frecventaSecunde' la numarul necesar de pasi
                pasiFaraPauza++;
                if (pasiPeSecunda * frecventaUpdate < pasiFaraPauza)
                {
                    pasiFaraPauza = 0;
                    yield return new WaitForSeconds(frecventaUpdate);
                }

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
            // Memoram distanta
            stats.distanta = final.cost;

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

            // Memoram drumul in statistici
            stats.drum = drum;

            // Calculare timp rulare
            stats.durataRulare = Time.realtimeSinceStartup - startTime;

            // Memoram statisticile global
            this.statistici = stats;

            // Afisam drumul
            AfisareDrum(doarCuloare: true);
        }

        callback?.Invoke();
    }

    public IEnumerator AplicaDijkstraDualCuRafinare(System.Action callback)
    {
        if (!GataDeAlgoritm())
            yield break;

        // Memoram timpul de start
        float startTime = Time.realtimeSinceStartup;

        // Numarul de iteratii de repetare
        int iteratii = 5;

        // Reseteaza culorile
        ResetCuloare();

        while(iteratii > 0)
        {
            // Facem un pas
            iteratii--;

            // Date necesare pt algoritm
            List<List<int>> parinte = new List<List<int>>();
            // Vectorul de parinti va fi dublat aici ( un vector pentru parintele de pe partea start, unul pt parintele de pe partea scop)
            parinte.Add(Enumerable.Repeat(-1, points.Count).ToList());
            parinte.Add(Enumerable.Repeat(-1, points.Count).ToList());
            List<List<double>> distanta = new List<List<double>>();
            distanta.Add(Enumerable.Repeat(double.PositiveInfinity, points.Count).ToList());
            distanta.Add(Enumerable.Repeat(double.PositiveInfinity, points.Count).ToList());
            List<int> vizitat = Enumerable.Repeat(0, points.Count).ToList(); // 0 - nevizitat | 1- vizitat de start | 2- vizitat de scop

            // Introducem nodul de start
            List<DateDinamicaDual> priorityQueue = new List<DateDinamicaDual>();
            priorityQueue.Add(new DateDinamicaDual(startPoint, 0, 1));
            priorityQueue.Add(new DateDinamicaDual(endPoint, 0, 2));
            // Setam distanta de la nodul de start cu 0
            distanta[0][startPoint] = 0;
            distanta[1][endPoint] = 0;

            // Variabila pentru a memora nodul de intersectie
            int nodMijloc = -1;

            // Variabila stocare statistici rulare
            Statistici stats = new Statistici();

            int pasiFaraPauza = 0;

            // Rulam cat avem noduri nedescoperite in lista
            while (priorityQueue.Count != 0)
            {
                // Extragem nodul din queue
                int nod = priorityQueue[0].nodCurent;
                double costNod = priorityQueue[0].cost;
                int parte = priorityQueue[0].parte;
                priorityQueue.RemoveAt(0);

                // Verificam daca nodul extras a fost vizitat de cealalta parte
                if (vizitat[nod] != 0 && vizitat[nod] != parte)
                {
                    // Oprim algoritmul
                    nodMijloc = nod;
                    break;
                }

                // Ignoram nodurile vizitate deja de partea noastra (putem sa avem un cost mai prost in queue)
                if (vizitat[nod] == parte)
                {
                    // Ignoram
                    continue;
                }

                // Numaram nodul la statistici
                stats.numarNoduri++;

                // Marcam nodul ca vizitat
                vizitat[nod] = parte;

                // Avansam toate nodurile vecine care nu au fost deja calculate
                int nrVecini = points[nod].vecini.Count;
                for (int i = 0; i < nrVecini; i++)
                {
                    int vecin = points[nod].vecini[i];
                    // verificam daca am mai fost in acest nod din partea noastra
                    if (vizitat[vecin] == parte)
                    {
                        //vecinul e deja vizitat, il sarim
                        continue;
                    }

                    // Calculam costul
                    double costVecin = costNod + Vector3.Distance(points[nod].coordonate, points[vecin].coordonate);

                    // Numaram muchia la statistici
                    stats.numarMuchii++;

                    // Verificam daca costul e mai bun decat ceva ce am gasit deja
                    if (costVecin >= distanta[parte - 1][vecin])
                    {
                        // Daca nu il ignoram
                        continue;
                    }

                    // Actualizam noua distanta
                    distanta[parte - 1][vecin] = costVecin;
                    // Actualizam parintele vecinului
                    parinte[parte - 1][vecin] = nod;

                    // Afisam noua linie
                    int indexVecin = points[nod].vecini.FindIndex(x => x == vecin);
                    points[nod].linii[indexVecin].SetActive(true);

                    // Asteptam pentru 'frecventaSecunde' la numarul necesar de pasi
                    pasiFaraPauza++;
                    if (pasiPeSecunda * frecventaUpdate < pasiFaraPauza)
                    {
                        pasiFaraPauza = 0;
                        yield return new WaitForSeconds(frecventaUpdate);
                    }

                    // Construim obiectul cu datele
                    DateDinamicaDual dateDinamica = new DateDinamicaDual(vecin, costVecin, parte);

                    // Inseram in lista ordonata la pozitia potrivita
                    int poz = priorityQueue.BinarySearch(dateDinamica);
                    // Nu exista un nod cu costul acesta, asa ca facem complementul binar pentru a afla pozitia pe care il inseram pentru a pastra lista sortata
                    if (poz < 0)
                        poz = ~poz;
                    priorityQueue.Insert(poz, dateDinamica);
                }
            }

            if (nodMijloc == -1)
            {
                // Nu avem drum -> iesim
                callback?.Invoke();
                yield break;
            }

            // La ultima iteratie afisam si memoram drumul
            if (nodMijloc != -1)
            {
                // Construim drumul
                List<int> drum = new List<int>();

                // Incepem din nodul de mijloc spre final
                int nodDrum = nodMijloc;
                drum.Add(nodDrum);
                while (parinte[1][nodDrum] != -1)
                {
                    nodDrum = parinte[1][nodDrum];
                    drum.Add(nodDrum);
                }
                // Invesam ordinea(nodul de sfarsit va fi primul in vector)
                drum.Reverse();

                // Mergem din mijloc spre inceput
                nodDrum = nodMijloc;
                while (parinte[0][nodDrum] != -1)
                {
                    nodDrum = parinte[0][nodDrum];
                    drum.Add(nodDrum);
                }
                // Invesam ordinea (nodul de inceput va fi primul in vector)
                drum.Reverse();

                // Memoram drumul
                drumMinimCalculat = drum;

                // Memoram drumul si distanta in statistici
                stats.drum = drum;
                stats.distanta = distanta[0][nodMijloc] + distanta[1][nodMijloc];

                // Calculare timp rulare
                stats.durataRulare = Time.realtimeSinceStartup - startTime;

                // Memoram statisticile global
                this.statistici = stats;

                
            }

            // Generam noi noduri pentru a rafina 
            // Luam fiecare nod din drum si pentru fiecare fata din care apartine adaugam un nod la mijloc si il legam de nodurile fetei
            HashSet<Fata> feteSparte = new HashSet<Fata>();

            // Pastram in un dictionar fetele deja parcurse
            foreach(int nod in drumMinimCalculat)
            {
                Punct p = points[nod];

                // Lista noua(lista nodului se va schimba pe masura ce spargem fetele
                List<Fata> listaInterna = new List<Fata>(p.fete);

                foreach(Fata f in listaInterna)
                {
                    // Verificam sa nu fie o fata sparta deja
                    if(feteSparte.Contains(f))
                    {
                        continue;
                    }

                    Punct p1 = points[f.nod1];
                    Punct p2 = points[f.nod2];
                    Punct p3 = points[f.nod3];

                    // Calculam coordonatele noului punct
                    Vector3 coordPunctMijloc = (p1.coordonate + p2.coordonate + p3.coordonate)/3;

                    // Adaugam punctul in scena si in date
                    Punct punctMijloc = AddPoint(coordPunctMijloc);

                    // Construim noile legaturi
                    AddLineBetweenPoints(punctMijloc, p1);
                    AddLineBetweenPoints(punctMijloc, p2);
                    AddLineBetweenPoints(punctMijloc, p3);

                    // Construim cele 3 fete noi
                    Fata f1 = new Fata(p1, p2, punctMijloc);
                    Fata f2 = new Fata(p1, p3, punctMijloc);
                    Fata f3 = new Fata(p2, p3, punctMijloc);

                    // Marcam faptul ca fetele sunt construite in iteratia curenta
                    feteSparte.Add(f1);
                    feteSparte.Add(f2);
                    feteSparte.Add(f3);

                    // Marcam faptul ca punctul de mijloc apartine celor 3 fete
                    punctMijloc.fete.Add(f1);
                    punctMijloc.fete.Add(f2);
                    punctMijloc.fete.Add(f3);

                    // Eliminam fata veche din nodurile p1..p3 si le adaugam noile fete
                    p1.fete.Remove(f);
                    p1.fete.Add(f1);
                    p1.fete.Add(f2);

                    p2.fete.Remove(f);
                    p2.fete.Add(f1);
                    p2.fete.Add(f3);

                    p3.fete.Remove(f);
                    p3.fete.Add(f2);
                    p3.fete.Add(f3);
                }
            }

            // Apelam afisare linii pentru a ne genera noile linii
            AfisareLinii();
        }

        // Afisam drumul
        AfisareDrum(doarCuloare: true);

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

    public void UpdatePasiPeSecunda(int pasi)
    {
        pasiPeSecunda = pasi;
    }

    public Statistici GetStatistici()
    {
        return new Statistici(statistici);
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

public class DateDinamicaDual : System.IComparable<DateDinamicaDual>
{
    public int nodCurent;
    public double cost;
    public int parte;

    public DateDinamicaDual(int nodCurent, double cost, int parte)
    {
        this.nodCurent = nodCurent;
        this.cost = cost;
        this.parte = parte;
    }

    public int CompareTo(DateDinamicaDual other)
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
        List<string> numeFisiere = Directory.GetFiles(basePath + "/Input","*.txt").ToList();
        numeFisiere.AddRange(Directory.GetFiles(basePath + "/Input","*.obj").ToList());
        // Aratam utilizatorului doar denumirea, nu si calea fisierului
        for (int i = 0;i < numeFisiere.Count;i++)
        {
            numeFisiere[i] = Path.GetFileName(numeFisiere[i]);
        }

        TMP_Dropdown dropdown = dropdownFisiere.GetComponent<TMP_Dropdown>();

        dropdown.AddOptions(numeFisiere);

    }

    public void GenereazaDate()
    {
        if (data != null)
            data.EliminaElementeDesenate();

        // Construim obiectul care va tine datele
        data =  new PointsData(prefabPunct, prefabLinie, startPointMaterial, endPointMaterial, uiManager);

        // setare delay pas algoritm
        data.pasiPeSecunda = delayPasAlgoritm;

        // Inscriere la evenimentul de click pe punct
        data.eventPunctAles += eventPunctAles;

        // Folosim dropdown-ul pentru a alege fisierul
        TMP_Dropdown dropdown = dropdownFisiere.GetComponent<TMP_Dropdown>();
        string numeFisier = dropdown.options[dropdown.value].text;
        // Cale de baza
        string basePath = Application.streamingAssetsPath;

        // Citim datele
        data.ReadPointsFromFile(basePath + "/Input/" + numeFisier);

        // Fortam scalarea initiala
        uiManager.OnSliderValueChanged();
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

        // Punem ui-ul de rulare
        uiManager.UiRulare();

        // Aplicam Algoritm
        if (uiManager.AlgoritmAles() == TipAlgoritm.Backtracking)
        {
            StartCoroutine(data.AplicaBacktracking(FinalCorutina));
        }
        else if (uiManager.AlgoritmAles() == TipAlgoritm.Dijkstra)
        {
            StartCoroutine(data.AplicaDijkstra(FinalCorutina));
        }
        else if (uiManager.AlgoritmAles() == TipAlgoritm.DijkstraDual)
        {
            StartCoroutine(data.AplicaDijkstraDual(FinalCorutina));
        }
        else if (uiManager.AlgoritmAles() == TipAlgoritm.Astar)
        {
            StartCoroutine(data.AplicaAStar(FinalCorutina,Vector3.Distance));
        }
    }

    private void FinalCorutina()
    {
        // Ne intoarcem la ui Standard
        uiManager.UiStandard();

        // Afisam statisticile
        Statistici stats = data.GetStatistici();
        if(stats != null)
            uiManager.AfiseazaStatistici(stats);
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

    public void UpdatePasiPeSecunda(int pasi)
    {
        data?.UpdatePasiPeSecunda(pasi);
    }
}
