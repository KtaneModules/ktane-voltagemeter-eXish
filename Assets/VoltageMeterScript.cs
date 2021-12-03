using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json;
using HarmonyLib;

public class VoltageMeterScript : MonoBehaviour
{

    public GameObject pointer;

    readonly private string QUERY_KEY = "volt";
    private bool activated;
    private bool solved;
    private double[] possibleVoltages = new double[] { 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5, 10 };
    private static int chosenVoltage;

    static int widgetIdCounter = 1;
    int widgetId;

    void Awake()
    {
        widgetId = widgetIdCounter++;
        GetComponent<KMWidget>().OnQueryRequest += GetQueryResponse;
        GetComponent<KMWidget>().OnWidgetActivate += Activate;
        GetComponent<KMBombInfo>().OnBombSolved += Solve;
        GetComponent<KMBombInfo>().OnBombExploded += Explode;
        if(widgetId == 1)
        {
            chosenVoltage = UnityEngine.Random.Range(0, possibleVoltages.Length);
            Debug.LogFormat("[Voltage Meter] Voltage: {0}V", possibleVoltages[chosenVoltage]);
        }
    }

    void Activate()
    {
        if(!solved)
        {
            StartCoroutine(MovePointer(true));
            activated = true;
        }
    }

    void Solve()
    {
        if(activated)
            StartCoroutine(MovePointer(false));
        solved = true;
        widgetIdCounter = 1;
    }

    void Explode()
    {
        widgetIdCounter = 1;
    }

    public string GetQueryResponse(string queryKey, string queryInfo)
    {
        if(queryKey == QUERY_KEY)
        {
            Dictionary<string, string> response = new Dictionary<string, string>
            {
                { "voltage", possibleVoltages[chosenVoltage].ToString() }
            };
            string responseStr = JsonConvert.SerializeObject(response);
            return responseStr;
        }
        return "";
    }

    private IEnumerator MovePointer(bool startup)
    {
        float t = 0f;
        while(t < 1f)
        {
            if(startup)
                pointer.transform.localPosition = Vector3.Lerp(new Vector3(-0.0525f, -0.0123f, -0.0061f), new Vector3(-0.0525f + (chosenVoltage + 2) * 0.00525f, -0.0123f, -0.0061f), t);
            else
                pointer.transform.localPosition = Vector3.Lerp(new Vector3(-0.0525f + (chosenVoltage + 2) * 0.00525f, -0.0123f, -0.0061f), new Vector3(-0.0525f, -0.0123f, -0.0061f), t);
            t += Time.deltaTime * 2f;
            yield return null;
        }
        if(startup)
            pointer.transform.localPosition = new Vector3(-0.0525f + (chosenVoltage + 2) * 0.00525f, -0.0123f, -0.0061f);
        else
            pointer.transform.localPosition = new Vector3(-0.0525f, -0.0123f, -0.0061f);
    }
}
