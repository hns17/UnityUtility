using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnitaskTokenContainer;

public class TestMonoTask : MonoTask<TestMonoTask>
{
    private CancellationTokenData tokenData;

    private void Awake()
    {
        Debug.Log("Awake");
    }
    private void OnEnable()
    {
        TestTask().Forget();
    }

    async UniTaskVoid TestTask()
    {
        tokenData = new CancellationTokenData();
        while(true) {
            await UniTask.Delay(1000, false, PlayerLoopTiming.Update, tokenData.Token);
            Debug.Log("Test" + SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space)) {
            Cancel(tokenData);
        }
        else if(Input.GetKeyUp(KeyCode.Return)) {
            TestTask().Forget();
        }
    }
}
