using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CoroutineManager : Singleton<CoroutineManager>
{
    private Dictionary<string, Coroutine> _coroutines = new Dictionary<string, Coroutine>();

    public string BeginCoroutine(IEnumerator coroutine)
    {
        var cor = StartCoroutine(coroutine);
        var key = GenerateKey(8);
        _coroutines.Add(key, cor);
        return key;
    }

    public void EndCoroutine(string key)
    {
        var cor = _coroutines[key];
        if (cor == null) return;
        StopCoroutine(cor);
    }

    private string GenerateKey(uint length)
    {
        StringBuilder sb = new StringBuilder();
        for (uint i = 0; i < length; ++i)
        {
            sb.Append((char) Random.Range(0, 255));
        }
        string key = sb.ToString(); 
        if (_coroutines.ContainsKey(key)) return GenerateKey(length);
        return key;
    }
}
