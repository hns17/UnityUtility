using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestMonoTask : MonoTask
{
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
        while(true) {
            await UniTask.Delay(1000, false, PlayerLoopTiming.Update, Token);
            Debug.Log("Test" + SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space)) {
            Cancel();
        }
        else if(Input.GetKeyUp(KeyCode.Return)) {
            TestTask().Forget();
        }
    }
}
