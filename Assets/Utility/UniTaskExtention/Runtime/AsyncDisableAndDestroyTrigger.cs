using Cysharp.Threading.Tasks.Triggers;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnitaskTokenContainer;

namespace Cysharp.Threading.Tasks
{
    /**
     * @author  hns17@naver.com
     * @brief   Monobehaviour���� ����� CancellationTokenSource ������ ���� Extention Method
     *          OnDisable �� OnDestroy�� ȣ��Ǹ� Cancel ó���Ǵ� ��ū�� �����Ѵ�.
     */
    public static class UnityCancellationTask
    {
        /// <summary>This CancellationToken is canceled when the MonoBehaviour will be destroyed.</summary>
        public static CancellationTokenData GetCancellationTokenOnDisableAndDestroy(this GameObject gameObject)
        {
            return gameObject.GetAsyncDisableAndDestroyTrigger().CancellationToken;
        }

        /// <summary>This CancellationToken is canceled when the MonoBehaviour will be destroyed.</summary>
        public static CancellationTokenData GetCancellationTokenOnDisableAndDestroy(this Component component)
        {
            return component.GetAsyncDisableAndDestroyTrigger().CancellationToken;
        }
    }
}


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Cysharp.Threading.Tasks.Triggers
{
    /**
     * @author  hns17@naver.com
     * @brief   CancellationToken ������ ������Ʈ(AsyncDisableAndDestroyTrigger)�� ��ȯ�Ѵ�.
     *          ������Ʈ�� ������Ʈ�� ���� ��� �߰� �� ��ȯ
     */
    public static partial class AsyncTriggerExtensions
    {
        public static AsyncDisableAndDestroyTrigger GetAsyncDisableAndDestroyTrigger(this GameObject gameObject)
        {
            return GetOrAddComponent<AsyncDisableAndDestroyTrigger>(gameObject);
        }

        public static AsyncDisableAndDestroyTrigger GetAsyncDisableAndDestroyTrigger(this Component component)
        {
            return component.gameObject.GetAsyncDisableAndDestroyTrigger();
        }


        static T GetOrAddComponent<T>(GameObject gameObject)
    where T : Component
        {
#if UNITY_2019_2_OR_NEWER
            if(!gameObject.TryGetComponent<T>(out var component)) {
                component = gameObject.AddComponent<T>();
            }
#else
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
#endif

            return component;
        }
    }

    /**
     * @author  hns17@naver.com
     * @brief   CancellationToken ������ Ŭ����
     *          Monobehaviour�� �̺�Ʈ �Լ� OnDisable, OnDestroy �Լ��� ȣ��Ǹ� Cancel �Ǵ� Token ������ ������Ʈ
     */
    [DisallowMultipleComponent]
    public sealed class AsyncDisableAndDestroyTrigger : MonoBehaviour
    {
        private Dictionary<int, CancellationTokenData> tokenDatas = new Dictionary<int, CancellationTokenData>();

        public CancellationTokenData CancellationToken
        {
            get {
                if(!gameObject.activeSelf) {
                    var errMsg = string.Format("Couldn't be getted because the game object '{0}' is inactive!", this.gameObject.name);
                    throw new System.Exception(errMsg);
                }

                var newCancellationTokenData = UnitaskTokenContainer.GetObjectToken();
                tokenDatas.Add(newCancellationTokenData.TokenID, newCancellationTokenData);
                return newCancellationTokenData;
            }
        }

        public bool Cancel(int tokenID)
        {
            var res = false;
            if(tokenDatas.TryGetValue(tokenID, out var tokenData)) {
                res = UnitaskTokenContainer.Cancel(tokenID);
                tokenDatas.Remove(tokenID);
            }
            return res;
        }

        public bool Cancel(CancellationTokenData targetData)
        {
            return Cancel(targetData.TokenID);
        }

        public void Clear()
        {
            foreach(var tokenData in tokenDatas) {
                UnitaskTokenContainer.Cancel(tokenData.Value);
            }
            tokenDatas.Clear();
        }

        void OnDisable()
        {
            Clear();
        }

        void OnDestroy()
        {
            Clear();
        }
    }
}

