using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Linq;

[RequireComponent(typeof(KMService))]
public class VoltageMeterService : MonoBehaviour
{
    private Harmony _harmony;

    private void Awake()
    {
        if(_harmony != null)
            return;
        _harmony = new Harmony("voltageMeter.harmony");

        System.Type type = Ex.ByName("WidgetGenerator");
        if(type == null)
            return;

        Debug.LogFormat("[Voltage Meter] Patching GenerateWidgets()...");
        _harmony.Patch(type.GetMethod("GenerateWidgets", Ex.FLAGS), new HarmonyMethod(GetType().GetMethod("GenerateWidgetsPrefix", Ex.FLAGS)));
    }

    private static bool GenerateWidgetsPrefix(object widgetManager, int optionalWidgetsToGenerate, object __instance)
    {
        Debug.LogFormat("[Voltage Meter] Begin Patched GenerateWidgets()");
        foreach(object requiredWidget in __instance.Get<IList>("RequiredWidgets"))
        {
            object widget = __instance.Call<object>("CreateWidget", requiredWidget);
            if(widget != null)
            {
                widgetManager.Call("AddWidget", widget);
                widget.Call("InitWidget", widgetManager);
            }
        }
        bool _isVoltage = false;
        Debug.LogFormat("[Voltage Meter] Required Widgets Loaded");
        for(int index = 0; index < optionalWidgetsToGenerate && __instance.Get<IList>("Widgets").Count != 0; ++index)
        {
            List<object> l = __instance.Get<IList>("Widgets").Cast<object>().ToList();
            if(_isVoltage)
                l.RemoveAll(o => ((MonoBehaviour)o).GetComponents<Component>().Any(c => c is VoltageMeterScript));

            if(!_isVoltage && widgetManager.Get<IEnumerable>("widgets").Cast<object>().Any(o => ((MonoBehaviour)o).GetComponents<Component>().Any(c => c is VoltageMeterScript)))
            {
                _isVoltage = true;

                l.RemoveAll(o => ((MonoBehaviour)o).GetComponents<Component>().Any(c => c is VoltageMeterScript));
                if(l.Count <= 0)
                    break;
            }

            object widget = __instance.Call<object>("CreateWidget", l[Random.Range(0, l.Count)]);
            if(widget != null)
            {
                widgetManager.Call("AddWidget", widget);
                widget.Call("InitWidget", widgetManager);
            }
        }
        Debug.LogFormat("[Voltage Meter] End Patched GenerateWidgets()");
        return false;
    }
}

internal static class Ex
{
    public const System.Reflection.BindingFlags FLAGS = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance;

    public static T Get<T>(this object o, string name)
    {
        return (T)o.GetType().GetField(name, FLAGS).GetValue(o);
    }

    public static void Set<T>(this object o, string name, T v)
    {
        o.GetType().GetField(name, FLAGS).SetValue(o, v);
    }

    public static T Call<T>(this object o, string name, params object[] args)
    {
        return (T)o.GetType().GetMethod(name, FLAGS).Invoke(o, args);
    }

    public static void Call(this object o, string name, params object[] args)
    {
        o.GetType().GetMethod(name, FLAGS).Invoke(o, args);
    }

    public static System.Type ByName(string name)
    {
        foreach(var assembly in System.AppDomain.CurrentDomain.GetAssemblies().Reverse())
        {
            var tt = assembly.GetType(name);
            if(tt != null)
            {
                return tt;
            }
        }

        return null;
    }
}