using Cysharp.Threading.Tasks.Triggers;
using System.Threading;
using UnityEngine;
using static UnitaskTokenContainer;

/**
 *  @author : hns17@naver.com
 *  @brief  : Monobehaviour �� CancellationToken ������ Ŭ����
 *              - UniTask�� �ڷ�ƾ ó�� ����ϱ� ���� �ۼ�
 *              - OnDisable, OnDistroy, Cancel �۾��� �����Ѵ�.
 */
[RequireComponent(typeof(AsyncDisableAndDestroyTrigger))]
public class MonoTask<T> : MonoBehaviour where T : MonoBehaviour
{
    private AsyncDisableAndDestroyTrigger trigger;

    /**
     *  @brief  : Token ����
     *  @return
     *      - true  : ���������� Cancel
     *      - false : Cancel Failed
     */
    public bool Cancel(int tokenID)
    {
        if(trigger == null) {
            return false;
        }

        return trigger.Cancel(tokenID);
    }

    public bool Cancel(CancellationTokenData tokenData)
    {
        return Cancel(tokenData.TokenID);
    }


    /**
     *  @brief  Token ����
     *  @return ����� Token ��ȯ
     */
    public CancellationTokenData CreateToken()
    {
        if(trigger == null) {
            this.TryGetComponent<AsyncDisableAndDestroyTrigger>(out trigger);
        }

        return trigger.CancellationToken;
    }
}
