// Assets/Scripts/QodanaTest.cs
using System;
using UnityEngine;

public class QodanaTest : MonoBehaviour
{
    // 1) Public field (style warning)
    public int badField;

    void Start()
    {
        // 2) Unused variable
        int unused = 42;

        // 3) Possible null dereference
        string s = null;
        Debug.Log(s.Length);

        // 4) Division by zero
        int x = 0;
        int y = 10 / x;
        Debug.Log(y);

        // 5) Empty catch block
        try
        {
            ThrowSomething();
        }
        catch (Exception)
        {
        }

        // 6) Always-true condition
        if (true)
        {
            Debug.Log("Always true");
        }

        // 7) Async void (bad practice)
        BadAsync();
    }

    async void BadAsync()
    {
        await System.Threading.Tasks.Task.Delay(100);
    }

    void ThrowSomething()
    {
        throw new Exception("Test");
    }
}