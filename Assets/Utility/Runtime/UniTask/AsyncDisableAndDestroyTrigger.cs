using Cysharp.Threading.Tasks.Triggers;
using System.Threading;
using UnityEngine;


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
        public static CancellationToken GetCancellationTokenOnDisableAndDestroy(this GameObject gameObject)
        {
            return gameObject.GetAsyncDisableAndDestroyTrigger().CancellationToken;
        }

        /// <summary>This CancellationToken is canceled when the MonoBehaviour will be destroyed.</summary>
        public static CancellationToken GetCancellationTokenOnDisableAndDestroy(this Component component)
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
        bool awakeCalled = false;
        bool called = false;
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public CancellationToken CancellationToken
        {
            get {
                if(CancellationTokenSource == null) {
                    CancellationTokenSource = new CancellationTokenSource();
                }

                if(!awakeCalled) {
                    PlayerLoopHelper.AddAction(PlayerLoopTiming.Update, new AwakeMonitor(this));
                }

                called = false;
                return CancellationTokenSource.Token;
            }
        }

        public void Cancel()
        {
            if(!called) {
                called = true;

                CancellationTokenSource?.Cancel();
                CancellationTokenSource?.Dispose();

                CancellationTokenSource = null;
            }
        }

        void Awake()
        {
            awakeCalled = true;
        }

        void OnDisable()
        {
            Cancel();
        }

        void OnDestroy()
        {
            Cancel();
        }


        class AwakeMonitor : IPlayerLoopItem
        {
            readonly AsyncDisableAndDestroyTrigger trigger;

            public AwakeMonitor(AsyncDisableAndDestroyTrigger trigger)
            {
                this.trigger = trigger;
            }

            public bool MoveNext()
            {
                if(trigger.called) return false;
                if(trigger == null) {
                    trigger.OnDestroy();
                    return false;
                }
                return true;
            }
        }
    }
}

