using System;
using System.Numerics;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class OfflineMgr : MonoSingletonAuto<OfflineMgr>
{
    private BigInteger curGoldRewardAmount;

    private void Start()
    {
        CalculateOfflineTimeAsync().Forget();
    }

    private async UniTaskVoid CalculateOfflineTimeAsync()
    {
        await UniTask.WaitUntil(() =>
            DataMgr.Inst != null &&
            DataMgr.Inst.IsLoadComplete_GameData &&
            TimerMgr.Inst != null);

        DebugMgr.Log("CalculateOfflineTime", Consts.eLogType.InGame);

        if (DataMgr.Inst.GameData.totalBreakCount <= 0)
        {
            DebugMgr.Log("totalBreakCount is 0이라 오프라인 보상 없음", Consts.eLogType.InGame);
            return;
        }

        if (DataMgr.Inst.GameData.hasPendingOfflineReward)
        {
            DebugMgr.Log("hasPendingOfflineReward — 이전 미수령 보상 복구", Consts.eLogType.InGame);
            RestorePendingOfflineReward();
            return;
        }

        DateTime curTime = TimerMgr.Inst.GetServerTime_Now();
        string offTime = DataMgr.Inst.LoadedLastLogInTimeStr;
        if (string.IsNullOrEmpty(offTime))
            offTime = DataMgr.Inst.GameData.lastLogInTimeStr;

        if (string.IsNullOrEmpty(offTime))
        {
            DebugMgr.LogError("lastLogInTimeStr is null", Consts.eLogType.InGame);
            return;
        }

        DateTime exitTime = Convert.ToDateTime(offTime);
        TimeSpan ts = curTime - exitTime;

        DebugMgr.Log($"offTime : {offTime}, curTime : {curTime}, ts.TotalHours = {ts.TotalHours:F2}", Consts.eLogType.InGame);

        // 최소 오프라인 시간 미달 → 보상 없음
        if (ts.TotalMinutes < Consts.Def.MinOfflineMinutes)
        {
            DebugMgr.Log("오프라인 시간이 너무 짧아 보상 없음", Consts.eLogType.InGame);
            return;
        }

        // 최대 12시간으로 보상 제한
        double cappedHours = Math.Min(ts.TotalHours, Consts.Def.MaxOfflineHours);
        TimeSpan cappedOfflineTime = TimeSpan.FromHours(cappedHours);

        ShowOfflineReward(cappedOfflineTime);
    }

    public void ShowOfflineReward(TimeSpan timeStamp)
    {
        string timeText = Util.GetTimeFormat(timeStamp, Consts.eTimeFormatType.Localized);

        BigInteger totalBreakCnt = DataMgr.Inst.GameData.totalBreakCount;

        curGoldRewardAmount = DataMgr.Inst.CalculateGoldByMinutes(timeStamp.TotalMinutes);

        DataMgr.Inst.SavePendingOfflineReward(curGoldRewardAmount);

        DebugMgr.Log($"totalBreakCount: {totalBreakCnt}, cappedMinutes: {timeStamp.TotalMinutes}, curGoldRewardAmount: {curGoldRewardAmount}", Consts.eLogType.InGame);

        string amountTxt = TableMgr.Inst.GetString("OFFLINE_REWARD_AMOUNT", Util.ConvertUnit(curGoldRewardAmount.ToString()));

        string infoTxt = TableMgr.Inst.GetString("OFFLINE_REWARD_TIME", timeText) + " " + TableMgr.Inst.GetString("OFFLINE_REWARD_MAX_TIME", 12);

        PopupMgr.Inst.SetOfflineRewardUI(infoTxt, amountTxt, GetOfflineRewardAds).Forget();
    }

    public void GetOfflineRewardAds()
    {
        // 기본 보상 지급
        if (DataMgr.Inst.SetCurrency(Consts.eCurrency.GD, curGoldRewardAmount, true))
        {
            DataMgr.Inst.ClearPendingOfflineReward();
            DebugMgr.Log($"GetOfflineReward Success 획득: {Util.ConvertUnit(curGoldRewardAmount.ToString())}");
            ShowRewardResultPopup(curGoldRewardAmount);
        }
        else
        {
            DebugMgr.LogError("GetOfflineReward Fail");
        }
    }

    private void RestorePendingOfflineReward()
    {
        if (!BigInteger.TryParse(DataMgr.Inst.GameData.pendingOfflineRewardStr, out var amount) || amount <= 0)
        {
            DebugMgr.LogError("pendingOfflineRewardStr 파싱 실패 — pending 클리어");
            DataMgr.Inst.ClearPendingOfflineReward();
            return;
        }

        curGoldRewardAmount = amount;

        string amountTxt = TableMgr.Inst.GetString("OFFLINE_REWARD_AMOUNT", Util.ConvertUnit(amount.ToString()));
        string infoTxt = TableMgr.Inst.GetString("OFFLINE_REWARD_MAX_TIME", 12);

        PopupMgr.Inst.SetOfflineRewardUI(infoTxt, amountTxt, GetOfflineRewardAds).Forget();
    }

    private void ShowRewardResultPopup(BigInteger amount)
    {
        string title = TableMgr.Inst.GetString("OFFLINE_REWARD_CLAIM_TITLE");
        string amountTxt = TableMgr.Inst.GetString("OFFLINE_REWARD_AMOUNT", Util.ConvertUnit(amount.ToString()));
        PopupMgr.Inst.PopUp(title, amountTxt, Consts.ePopup.Message);
        UIMgr.Inst.OnReceiveCoinEffect();
        SoundMgr.Inst.PlaySfx(Consts.eSfxCategory.UI, "Reward");
    }

    public async UniTaskVoid GetOfflineRewardDoubleAds()
    {
        BigInteger doubleAmount = curGoldRewardAmount * 2;

        if(DataMgr.Inst.IsAdDisabled())
        {
            if (DataMgr.Inst.SetCurrency(Consts.eCurrency.GD, doubleAmount, true))
            {
                DataMgr.Inst.ClearPendingOfflineReward();
                QuestMgr.Inst.ReportProgress(Consts.eQuestType.WatchAdReward, 1);
                QuestMgr.Inst.ReportProgress(Consts.eQuestType.GainGold, doubleAmount);
                DebugMgr.Log($"GetOfflineReward Ads Skip Success: {Util.ConvertUnit(doubleAmount.ToString())}");
                ShowRewardResultPopup(doubleAmount);
            }
            else
            {
                DebugMgr.LogError("GetOfflineReward Ads Skip Fail");
            }

            return;
        }

        var result = await AdsMgr.Inst.ShowAdsAsync((int)Consts.eRewardID_Ad_Unit.Reward_Offline_Reward);

        await UniTask.SwitchToMainThread();
        await UniTask.DelayFrame(1);

        if (DataMgr.Inst == null || QuestMgr.Inst == null || PopupMgr.Inst == null)
        {
            DebugMgr.LogError("GetOfflineRewardDoubleAds: 매니저 null — 보상 지급 불가");
            return;
        }

        if (result == Consts.eAdResult.Rewarded)
        {
            if (DataMgr.Inst.SetCurrency(Consts.eCurrency.GD, doubleAmount, true))
            {
                DataMgr.Inst.ClearPendingOfflineReward();
                QuestMgr.Inst.ReportProgress(Consts.eQuestType.WatchAdReward, 1);
                QuestMgr.Inst.ReportProgress(Consts.eQuestType.GainGold, doubleAmount);
                FirebaseMgr.Inst?.LogRewardedAdComplete("offline_reward");
                DebugMgr.Log($"GetOfflineReward Ads Success: {Util.ConvertUnit(doubleAmount.ToString())}");
                ShowRewardResultPopup(doubleAmount);
            }
            else
            {
                DebugMgr.LogError("GetOfflineReward Ads Fail");
            }
        }
        else
        {
            DebugMgr.LogError($"재생 가능한 광고 없음: {result.ToString()}");
            // 광고 실패 시 기본 보상(1배) 지급 → 보상 팝업 확인 후 광고 없음 안내
            if (DataMgr.Inst.SetCurrency(Consts.eCurrency.GD, curGoldRewardAmount, true))
            {
                DataMgr.Inst.ClearPendingOfflineReward();
                DebugMgr.Log($"GetOfflineReward Ad Fallback: {Util.ConvertUnit(curGoldRewardAmount.ToString())}");
                string title = TableMgr.Inst.GetString("OFFLINE_REWARD_CLAIM_TITLE");
                string amountTxt = TableMgr.Inst.GetString("OFFLINE_REWARD_AMOUNT", Util.ConvertUnit(curGoldRewardAmount.ToString()));
                UIMgr.Inst.OnReceiveCoinEffect();
                SoundMgr.Inst.PlaySfx(Consts.eSfxCategory.UI, "Reward");
                PopupMgr.Inst.PopUp(title, amountTxt, Consts.ePopup.Message, () =>
                    PopupMgr.Inst.PopUp("AD_NOT_READY_POPUP_TITLE", "AD_NOT_READY_POPUP_DESC", Consts.ePopup.Message));
            }
            else
            {
                DebugMgr.LogError("GetOfflineReward Ad Fallback Fail");
                PopupMgr.Inst.PopUp("AD_NOT_READY_POPUP_TITLE", "AD_NOT_READY_POPUP_DESC", Consts.ePopup.Message);
            }
        }
    }
}
