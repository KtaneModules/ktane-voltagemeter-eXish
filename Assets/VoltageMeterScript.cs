using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class VoltageMeterScript : MonoBehaviour
{
    public GameObject squareModel;
    public GameObject circleModel;
    public GameObject pointer;
    public GameObject circlePointer;

    readonly private string QUERY_KEY = "volt";
    private bool activated;
    private bool solved;
    private bool _isCircular;
    private double[] possibleVoltages = new double[] { 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5, 10 };
    private static int chosenVoltage;

    void Awake()
    {
        GetComponent<KMWidget>().OnQueryRequest += GetQueryResponse;
        GetComponent<KMWidget>().OnWidgetActivate += Activate;
        GetComponent<KMBombInfo>().OnBombSolved += Solve;

        _isCircular = Random.Range(0, 2) == 0;
        Debug.LogFormat("[Voltage Meter] Is circular: {0}", _isCircular);
        if(_isCircular)
            squareModel.SetActive(false);
        else
            circleModel.SetActive(false);

        chosenVoltage = Random.Range(0, possibleVoltages.Length);
        Debug.LogFormat("[Voltage Meter] Voltage: {0}V", possibleVoltages[chosenVoltage]);
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
            if(!_isCircular)
            {
                if(startup)
                    pointer.transform.localPosition = Vector3.Lerp(new Vector3(-0.0525f, -0.0123f, -0.0061f), new Vector3(-0.0525f + (chosenVoltage + 2) * 0.00525f, -0.0123f, -0.0061f), t);
                else
                    pointer.transform.localPosition = Vector3.Lerp(new Vector3(-0.0525f + (chosenVoltage + 2) * 0.00525f, -0.0123f, -0.0061f), new Vector3(-0.0525f, -0.0123f, -0.0061f), t);
            }
            else
            {
                if(startup)
                    circlePointer.transform.localEulerAngles = Vector3.Lerp(new Vector3(0f, -76.5f, 0f), new Vector3(0f, ((float)possibleVoltages[chosenVoltage] - 5f) * 15.3f, 0f), t);
                else
                    circlePointer.transform.localEulerAngles = Vector3.Lerp(new Vector3(0f, ((float)possibleVoltages[chosenVoltage] - 5f) * 15.3f, 0f), new Vector3(0f, -76.5f, 0f), t);
            }
            t += Time.deltaTime * 2f;
            yield return null;
        }
        if(!_isCircular)
        {
            if(startup)
                pointer.transform.localPosition = new Vector3(-0.0525f + (chosenVoltage + 2) * 0.00525f, -0.0123f, -0.0061f);
            else
                pointer.transform.localPosition = new Vector3(-0.0525f, -0.0123f, -0.0061f);
        }
        else
        {
            if(startup)
                circlePointer.transform.localEulerAngles = new Vector3(0f, ((float)possibleVoltages[chosenVoltage] - 5f) * 15.3f, 0f);
            else
                circlePointer.transform.localEulerAngles = new Vector3(0f, -76.5f, 0f);
        }
    }
}
