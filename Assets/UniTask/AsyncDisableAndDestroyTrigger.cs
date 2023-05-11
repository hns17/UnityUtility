using Cysharp.Threading.Tasks.Triggers;
using System.Threading;
using UnityEngine;


namespace Cysharp.Threading.Tasks
{
    /**
     * @author  hns17@naver.com
     * @brief   Monobehaviour에서 사용할 CancellationTokenSource 관리를 위한 Extention Method
     *          OnDisable 및 OnDestroy가 호출되면 Cancel 처리되는 토큰을 리턴한다.
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
     * @brief   CancellationToken 관리용 컴포넌트(AsyncDisableAndDestroyTrigger)를 반환한다.
     *          오브젝트에 컴포넌트가 없는 경우 추가 후 반환
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
     * @brief   CancellationToken 관리용 클래스
     *          Monobehaviour의 이벤트 함수 OnDisable, OnDestroy 함수가 호출되면 Cancel 되는 Token 관리용 컴포넌트
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

