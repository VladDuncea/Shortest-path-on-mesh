using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Proiectie
{
    public Vector3 p1, p2;

    public Proiectie(Vector3 a, Vector3 b)
    {
        p1 = a;
        p2 = b;
    }


    override public string ToString()
    {
        return p1.ToString() + " " + p2.ToString();
    }
}

public class Utils : MonoBehaviour
{

    // Functie care face intersectia dreptelor cu mai multe modificari
    // Fie o semidreapta A definita de punctele x1,x2 cu capatul finit in x1 si o dreapta B definita de punctele x3,x4.
    // Vrem sa gasim intersectia semidreptei A (finita la punctul x1) cu dreapta B.
    // In cazul in care intersectia nu are loc vom intoarce null
    public static Vector3? IntersectiaDreptelorModif(Vector3 x1, Vector3 x2, Vector3 x3, Vector3 x4)
    {
        Vector3 a, b, c;
        
        // Valori utilizate des
        a = x2 - x1;
        b = x4 - x3;
        c = x3 - x1;


        if (!Coplanare(x1,x2,x3,x4))
        {
            // Dreptele nu sunt coplanare!
            Debug.Log("Dreptele nu sunt coplanare!");
            return null;
        }

        float numarator = Vector3.Dot(Vector3.Cross(c, b), Vector3.Cross(a, b));
        float numitor = Vector3.Cross(a, b).sqrMagnitude;

        float s = numarator / numitor;

        // Verificare intersectie in afara semidreptei A
        if (s<0)
        {
            return null;
        }

        Vector3 intersectie = x1 + a * s;  


        return intersectie;
    }


    public static bool Coplanare(Vector3 x1, Vector3 x2, Vector3 x3, Vector3 x4)
    {
        Vector3 a, b, c;

        // Valori utilizate des
        a = x2 - x1;
        b = x4 - x3;
        c = x3 - x1;

        // Verificare coplanaritate
        float copl = Vector3.Dot(c, Vector3.Cross(a, b));

        return copl == 0;
    }

    // S este punctul start, p este proiectia initiala, l2 este punctul varful opus laturii pe care se afla proiectia
    public static Tuple<Proiectie,Proiectie> Proiectia(Vector3 S, Proiectie p, Vector3 l1, Vector3 l2, Vector3 l3)
    {
        Vector3 p1 = p.p1, p2 = p.p2;

        // Verificam daca l1,l3 are aceeasi directie cu p1,p2, altfel inversam p1,p2
        float val = Vector3.Dot(p2 - p1, l3 - l1);

        if(val <0)
        {
            // Inversam punctele proiectiei
            p1 = p.p2;
            p2 = p.p1;
        }

        // Calculam proiectia lui S prin segmentul p1,p2 pe dreapta formata de l1,l2
        Vector3? intersectiaStanga = IntersectiaDreptelorModif(S, p1, l1, l2);
        Vector3? intersectiaDreapta = IntersectiaDreptelorModif(S, p2, l2, l3);


        // Nu avem intersectii 
        if (intersectiaStanga == null || intersectiaDreapta == null)
        {
            Debug.LogError("Intotdeauna trebuie sa fie intersectii");
            return null;
        }

        // Verificam daca am avut intersectie in ambele parti, daca da putem deduce celelalte 2 intersectii
        if(intersectiaDreapta != null && intersectiaStanga != null)
        {
            return new Tuple<Proiectie, Proiectie>(new Proiectie((Vector3)intersectiaDreapta, l2), 
                new Proiectie((Vector3)intersectiaStanga, l2));
        }
            

        // Transformam la simplu vector3
        Vector3 i1 = (Vector3)intersectiaStanga, i2 = (Vector3)intersectiaDreapta;
        Vector3 a = l2 - l1;

        // Verificam daca intersectia este complet in interiorul segmeuntului l1,l2 sau daca trebuie sa taiam
        float pozReli1 = Vector3.Magnitude(i1-l1)*Mathf.Sign(Vector3.Dot(i1 - l1, a))/Vector3.Magnitude(a);
        float pozReli2 = Vector3.Magnitude(i2-l1)*Mathf.Sign(Vector3.Dot(i2 - l1, a))/Vector3.Magnitude(a);

        // Cazul in care ambele puncte sunt in afara segmentului l1,l2, ignoram
        if ((pozReli1 < 0 && pozReli2 < 0) || (pozReli1 > 1 && pozReli2 > 1))
        {
            return null;
        }

        // Cazul in care ambele puncte sunt in interior, pastram asa cum sunt
        if((pozReli1 >= 0 && pozReli2 >= 0) && (pozReli1 <= 1 && pozReli2 <= 1))
        {
            //return new Proiectie(i1, i2);
        }

        // Verificam daca trebuie sa limitam unele din puncte
        
        if(pozReli1 < 0)
        {
            // i1 e mai aproape de l1
            i1 = l1;
        }
        else if (pozReli1 > 1)
        {
            // i1 e mai aproape de l2
            i1 = l2;
        }

        if (pozReli2 < 0)
        {
            // i1 e mai aproape de l1
            i2 = l1;
        }
        else if (pozReli2 > 1)
        {
            // i1 e mai aproape de l2
            i2 = l2;
        }

        return null;
    }

    public static Vector3 RotesteDupaLinie(Vector3 punct, Vector3 l1, Vector3 l2, float unghi)
    {
        Vector3 V = l2 - l1;

        Vector4 col1, col2, col3, col4;
        Vector4 pct = new Vector4(punct.x, punct.y, punct.z, 1);

        Vector3 u = V.normalized;
        float d = Mathf.Sqrt(u.y * u.y + u.z * u.z);

        col1 = new Vector4(1,0,0,0);
        col2 = new Vector4(0, 1, 0, 0);
        col3 = new Vector4(0, 0, 1, 0);
        col4 = new Vector4(-l1.x, -l1.y, -l1.z, 1);
        Matrix4x4 T = new Matrix4x4(col1, col2, col3, col4);
        
        // Rx
        if(d != 0)
        {
            col2.y = u.z / d;
            col2.z = u.y / d;
            col3.y = -u.y / d;
            col3.z = u.z / d;
        }
        else
        {
            col1 = new Vector4(1, 0, 0, 0);
            col2 = new Vector4(0, 1, 0, 0);
            col3 = new Vector4(0, 0, 1, 0);
        }
        
        col4 = new Vector4(0, 0, 0, 1);
        Matrix4x4 Rx = new Matrix4x4(col1, col2, col3, col4);
         
        // Ry
        col1.x = d;
        col1.z = u.x;
        col2 = new Vector4(0, 1, 0, 0);
        col3 = new Vector4(-u.x,0,d,0);
        Matrix4x4 Ry = new Matrix4x4(col1, col2, col3, col4);

        // Rz
        col1.x = Mathf.Cos(unghi);
        col1.y = Mathf.Sin(unghi);
        col1.z = 0;
        col2.x = -Mathf.Sin(unghi);
        col2.y = Mathf.Cos(unghi);
        col3 = new Vector4(0, 0, 1, 0);
        Matrix4x4 Rz = new Matrix4x4(col1, col2, col3, col4);

        Vector4 pctNou = T.inverse*Rx.inverse*Ry.inverse*Rz*Ry*Rx*T*pct;

        return new Vector3(pctNou.x,pctNou.y,pctNou.z);
    }

    // p1,p2 sunt muchia dupa care masuram unghiul!
    public static float UnghiIntrePunctSiPlan(Vector3 punct, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Calculam proiectia punctului pe dreapta p1,p2

        Vector3 directie = (p2 - p1) / Vector3.Distance(p1, p2);
        Vector3 vect = punct - p1;
        float t = Vector3.Dot(vect, directie);

        // Punctul de proiectie
        Vector3 punctProiectie = p1 + t * directie;

        Debug.Log(punctProiectie);

        // Calculam norma planului
        Vector3 normaPlan = Vector3.Cross(p2-p1,p3-p1);

        // Vectorul format de punct si proiectie
        Vector3 vectorPePlan = punct - punctProiectie;

        // Calculam unghiul dintre norma si vectorul construit de punctul nostru si punctul de proiectie
        float unghi = Mathf.Acos(Vector3.Dot(vectorPePlan, normaPlan)/ (vectorPePlan.magnitude * normaPlan.magnitude));

        Debug.Log(unghi-Mathf.PI/2);

        // Scadem 90 de grade pt ca norma face 90 de grade pe plan
        return unghi - Mathf.PI / 2;
    }

}
