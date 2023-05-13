using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnitaskTokenContainer;

public class TestTokenContainer : MonoBehaviour
{
    private CancellationTokenData global;
    private CancellationTokenData scene;
    private CancellationTokenData group;
    private CancellationTokenData obj;

    void Start()
    {
        global = UnitaskTokenContainer.GetGlobalToken("GlobalToken");
        scene = UnitaskTokenContainer.GetSceneToken();
        group = UnitaskTokenContainer.GetGroupToken("GroupToken");
        obj = UnitaskTokenContainer.GetObjectToken();

        TestTask("global", global).Forget();
        TestTask("scene", scene).Forget();
        TestTask("group_task0", group).Forget();
        TestTask("group_task1", group).Forget();
        TestTask("obj0", obj).Forget();
        TestTask("obj1", UnitaskTokenContainer.GetObjectToken()).Forget();
    }


    private async UniTaskVoid TestTask(string name, CancellationTokenData tokenData)
    {
        while(true) {
            await UniTask.Delay(1500, false, PlayerLoopTiming.Update, tokenData.Token);
            Debug.LogFormat("{0}", name);
        }
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Alpha1)) {
            UnitaskTokenContainer.Cancel("GlobalToken");
        }
        else if(Input.GetKeyUp(KeyCode.Alpha2)) {
            UnitaskTokenContainer.Cancel(scene);
        }
        else if(Input.GetKeyUp(KeyCode.Alpha3)) {
            UnitaskTokenContainer.Cancel(group.TokenID);
        }
        else if(Input.GetKeyUp(KeyCode.Alpha4)) {
            UnitaskTokenContainer.Cancel(obj);
        }
    }
}
