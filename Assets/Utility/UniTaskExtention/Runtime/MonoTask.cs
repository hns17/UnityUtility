using Cysharp.Threading.Tasks.Triggers;
using System.Threading;
using UnityEngine;


/**
 *  @author : hns17@naver.com
 *  @brief  : Monobehaviour �� CancellationToken ������ Ŭ����
 *              - UniTask�� �ڷ�ƾ ó�� ����ϱ� ���� �ۼ�
 *              - OnDisable, OnDistroy, Cancel �۾��� �����Ѵ�.
 */
[RequireComponent(typeof(AsyncDisableAndDestroyTrigger))]
public class MonoTask : MonoBehaviour
{
    private AsyncDisableAndDestroyTrigger trigger;

    /**
     *  @brief  : Token ����
     *  @return
     *      - true  : ���������� Cancel
     *      - false : Cancel Failed
     */
    public bool Cancel()
    {
        if(trigger == null) {
            return false;
        }

        trigger.Cancel();
        return true;
    }


    /**
     *  @brief  Token ����
     *  @return ����� Token ��ȯ
     */
    public CancellationToken Token
    {
        get {
            if(trigger == null) {
                this.TryGetComponent<AsyncDisableAndDestroyTrigger>(out trigger);
            }

            return trigger.CancellationToken;
        }
    }
}
