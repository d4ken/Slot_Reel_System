using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SlotMark
{
    Ichigo = 0,
    Bell = 1,
    Seven = 2,
}

public class SlotReel : MonoBehaviour
{
    [SerializeField] private RectTransform leftReel;
    [SerializeField] private RectTransform centerReel;
    [SerializeField] private RectTransform rightReel;
    private const int DefaultReelPositionY = 252;
    private const int MarkHeight = 84;
    private CancellationToken _ct;
    private bool _isReelStopped;
    private int _stockCounter;
    
    private void Start()
    {
        _stockCounter = 0;
        _isReelStopped = true;
        _ct = this.GetCancellationTokenOnDestroy();
    }

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_isReelStopped && _stockCounter == 0)
            {
                await PlaySlot();
            }
            else if (!_isReelStopped)
            {
                _stockCounter++;
            }

        }
        
        if (_isReelStopped && _stockCounter > 0)
        {
            _stockCounter--;
            await PlaySlot();
            Debug.Log(_stockCounter);
        }
        
    }

    private async UniTask PlaySlot()
    {
        _isReelStopped = false;
        var leftReelTask = ReelRotateTween(leftReel,leftReel.localPosition.y, 3);
        var centerReelTask = ReelRotateTween(centerReel,centerReel.localPosition.y, 4);
        var rightReelTask = ReelRotateTween(rightReel,rightReel.localPosition.y, 5);
        await UniTask.WhenAll(leftReelTask, centerReelTask, rightReelTask);
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: _ct);
        _isReelStopped = true;

    }

    private async UniTask ReelRotateTween(RectTransform reelTransform, float currentPositionY , int loopCount, float loopTime = 0.5f)
    {
        // 初回のリセット
        await reelTransform.DOLocalMoveY(0, currentPositionY/DefaultReelPositionY * loopTime)
            .SetEase(Ease.Linear)
            .SetLink(reelTransform.gameObject)
            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, _ct);
        
        // 高さをデフォルトに持っていく
        reelTransform.localPosition = new Vector3(reelTransform.localPosition.x, DefaultReelPositionY,
            reelTransform.localPosition.z);
        
        // リールのループ演出
        await reelTransform.DOLocalMoveY(0, loopTime)
            .SetLoops(loopCount, LoopType.Restart)
            .SetEase(Ease.Linear)
            .SetLink(reelTransform.gameObject)
            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, _ct);
        
        // 高さをデフォルトに持っていく
        reelTransform.localPosition = new Vector3(reelTransform.localPosition.x, DefaultReelPositionY,
            reelTransform.localPosition.z);

        int randomMark = Random.Range(0, 3);
        int targetPositionY = randomMark * MarkHeight;
        // ターゲットの位置に止まる
        await reelTransform.DOLocalMoveY(targetPositionY, (DefaultReelPositionY - (float)targetPositionY) / DefaultReelPositionY * loopTime)
            .SetEase(Ease.Linear)
            .SetLink(reelTransform.gameObject)
            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, _ct);
        
        // リールが一番上にある時
        if (reelTransform.localPosition.y <= 0)
        {
            // 高さを0に下げる
            reelTransform.localPosition = new Vector3(reelTransform.localPosition.x, DefaultReelPositionY,
                reelTransform.localPosition.z);
        }
    }
}
