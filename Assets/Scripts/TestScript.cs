using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public TestDisableAndDestroyToken testToken;

    private void Awake()
    {
        testToken.GetCancellationTokenOnDisableAndDestroy();
    }
}
