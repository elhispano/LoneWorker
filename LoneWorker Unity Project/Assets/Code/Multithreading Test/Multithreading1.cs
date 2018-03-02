using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

public class Multithreading1 : MonoBehaviour
{
    public Transform[] testTransforms;

    static bool done;
    static readonly object locker = new object();

    string outputConsole;

    List<Vector3> positions = new List<Vector3>(); 

    void OnGUI()
    {
        if (GUILayout.Button("Test 1"))
        {
            Method1();
        }

        GUILayout.Label(outputConsole);
    }

    void Method1()
    {
        Debug.Log("METHOD 1");  

        done = false;
        outputConsole = "";

        positions.Clear();
        for (int i = 0; i < testTransforms.Length; i++)
        {
            positions.Add(testTransforms [i].position);
        }

        Thread t = new Thread(ThreadMethod);
        t.Start();
    }

    void ThreadMethod()
    {
        lock (locker)
        {
            if (!done)
            {
                done = true;

                for (int i = 0; i < positions.Count; i++)
                {
                    outputConsole += positions[i].ToString();
                }
            }
        }
    }
}
