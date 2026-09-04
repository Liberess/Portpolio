using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using DG.Tweening;

using Consts;

public class GachaMgr : MonoSingletonAuto<GachaMgr>
{
    [Header("Popup Root")]
    [SerializeField] private CanvasGroup popupCanvasGroup;     // 전체 페이드용
    [SerializeField] private RectTransform popupRoot;          // 스케일 인/아웃
    [SerializeField] private Image dimBackground;              // 뒷배경 딤 처리(선택)

    [Header("Item UI")]
    [SerializeField] private GameObject itemPrefab;
    private Image itemImage;
    private Text itemNameText;
    private Text rarityText;
    private Text priceText;

    [Header("Rarity Visuals")]
    [SerializeField] private Image rarityFrame;                // 등급 테두리(색상/그라데)
    [SerializeField] private Image rarityGlow;                 // 부드러운 발광(루프)
    [SerializeField] private ParticleSystem rareFx;            // Rare 이상 공용
    [SerializeField] private ParticleSystem epicFx;            // Epic 이상
    [SerializeField] private ParticleSystem legendaryFx;       // Legendary 전용

    [Header("Buttons")]
    [SerializeField] private Button nextButton;                // “다음” 버튼
    [SerializeField] private Button skipButton;                // “넘기기/바로 다음” (선택)

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip sfxPopupIn;
    [SerializeField] private AudioClip sfxReveal;
    [SerializeField] private AudioClip sfxRare;
    [SerializeField] private AudioClip sfxEpic;
    [SerializeField] private AudioClip sfxLegendary;

    [Header("Timings")]
    [SerializeField, Min(0f)] private float holdIntro = 0.8f;       // 최초 정지
    [SerializeField, Min(0f)] private float popupFade = 0.25f;      // 팝업 페이드
    [SerializeField, Min(0f)] private float rootScaleIn = 0.35f;    // 팝업 스케일 인
    [SerializeField, Min(0f)] private float imageRevealDelay = 0.15f;
    [SerializeField, Min(0f)] private float pulseLoopDuration = 0.8f; // glow 루프

    [Header("Scales")]
    [SerializeField] private Vector3 rootStartScale = new Vector3(0.85f, 0.85f, 1f);
    [SerializeField] private Vector3 rootEndScale = Vector3.one;
    [SerializeField] private Vector3 imagePopScale = new Vector3(1.1f, 1.1f, 1f);

    [Header("Colors")]
    [SerializeField] private Color dimColor = new Color(0, 0, 0, 0.75f);
    [SerializeField] private Color commonColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color rareColor = new Color(0.4f, 0.6f, 1.0f);
    [SerializeField] private Color epicColor = new Color(0.8f, 0.4f, 1.0f);
    [SerializeField] private Color legendaryColor = new Color(1.0f, 0.7f, 0.2f);

    [Header("Misc")]
    [SerializeField] private bool closeOnQueueEmpty = true;     // 큐 비면 자동 닫기

    // ==== 내부 상태 ====
    private readonly Queue<ItemData> _queue = new Queue<ItemData>();
    private bool _isPlaying;
    private ItemData _current;
    private Sequence _seq;              // 현재 연출 시퀀스
    private Tween _glowLoop;           // 희귀도 Glow 루프
    private ParticleSystem[] _allFx;   // 파티클 캐시

    [Serializable]
    public struct ItemData
    {
        public string name;
        public Sprite icon;
        public eRarity rarity;
        public long price;             // 단순 예시 (원화/골드 등)

        public ItemData(string name, Sprite icon, eRarity rarity, long price)
        {
            this.name = name;
            this.icon = icon;
            this.rarity = rarity;
            this.price = price;
        }
    }

    public override void OnInitialize()
    {
        _allFx = new[] { rareFx, epicFx, legendaryFx };
        nextButton?.onClick.AddListener(OnClickNext);
        if (skipButton) skipButton.onClick.AddListener(SkipCurrent);

        // 안전 초기화
        popupCanvasGroup.alpha = 0f;
        popupRoot.localScale = rootStartScale;
        SetUIActive(false);
        if (dimBackground) dimBackground.color = dimColor.WithA(0f);
    }

    private void OnDisable()
    {
        KillAllTweens();
    }

    // ====== 외부 API ======
    public void Enqueue(ItemData data)
    {
        _queue.Enqueue(data);
        TryPlayNext();
    }

    public void EnqueueRange(IEnumerable<ItemData> list)
    {
        foreach (var it in list) _queue.Enqueue(it);
        TryPlayNext();
    }

    // ====== 내부 처리 ======
    private void TryPlayNext()
    {
        if (_isPlaying) return;
        if (_queue.Count == 0)
        {
            if (closeOnQueueEmpty) HidePopup();
            return;
        }

        _isPlaying = true;
        _current = _queue.Dequeue();
        PlaySequence(_current);
    }

    private void PlaySequence(ItemData data)
    {
        KillAllTweens();
        PrepareUIFor(data);

        // 팝업 표시 시작
        SetUIActive(true);
        popupCanvasGroup.alpha = 0f;
        popupRoot.localScale = rootStartScale;
        if (dimBackground) dimBackground.color = dimColor.WithA(0f);

        // 이미지/텍스트 초깃값
        itemImage.color = Color.clear;
        itemImage.transform.localScale = Vector3.one;
        rarityGlow.color = rarityGlow.color.WithA(0f);
        nextButton.gameObject.SetActive(false);

        // 파티클 모두 끄기
        StopAllFx();

        // 희귀도에 따른 색상/FX 로직
        Color rColor = GeteRarityColor(data.rarity);
        rarityFrame.color = rColor;

        // 메인 시퀀스
        _seq = DOTween.Sequence();

        // 1) 팝업 페이드 + 스케일 인
        _seq.Append(popupCanvasGroup.DOFade(1f, popupFade));
        if (dimBackground) _seq.Join(dimBackground.DOFade(dimColor.a, popupFade));

        _seq.Join(popupRoot.DOScale(rootEndScale, rootScaleIn)
                 .SetEase(Ease.OutBack, overshoot: 1.5f));

        // 2) 잠깐 홀드
        _seq.AppendInterval(holdIntro);

        // 3) 이미지/텍스트 등장 + 팝
        _seq.AppendCallback(() =>
        {
            if (sfxSource && sfxPopupIn) sfxSource.PlayOneShot(sfxPopupIn);
        });

        _seq.AppendInterval(imageRevealDelay);

        _seq.Append(itemImage.DOFade(1f, 0.15f));
        _seq.Join(itemImage.transform.DOPunchScale(imagePopScale - Vector3.one, 0.25f, 12, 0.7f));

        _seq.AppendCallback(() =>
        {
            // 희귀도 글로우 루프
            StartGlowLoop(rColor);

            // 희귀도별 추가 연출
            PlayeRarityFx(data.rarity);
            PlayeRaritySfx(data.rarity);
        });

        // 4) 버튼 노출
        _seq.AppendInterval(0.35f);
        _seq.AppendCallback(() =>
        {
            nextButton.gameObject.SetActive(true);
            if (sfxSource && sfxReveal) sfxSource.PlayOneShot(sfxReveal);
        });

        _seq.OnKill(() => _seq = null);
    }

    private void OnClickNext()
    {
        SoundMgr.Inst.PlaySfx(eSfxCategory.UI, "Button");
        GoNext();
    }

    private void GoNext()
    {
        // 다음 큐 진행
        _isPlaying = false;
        TryPlayNext();

        // 큐가 없으면 닫기
        if (_queue.Count == 0 && closeOnQueueEmpty)
            HidePopup();
    }

    private void HidePopup()
    {
        // 이미 닫힌 상태면 무시
        if (popupCanvasGroup.alpha <= 0.01f && popupRoot.localScale == rootStartScale) return;

        KillAllTweens();

        Sequence closeSeq = DOTween.Sequence();
        closeSeq.Append(popupCanvasGroup.DOFade(0f, 0.2f));
        if (dimBackground) closeSeq.Join(dimBackground.DOFade(0f, 0.2f));
        closeSeq.Join(popupRoot.DOScale(rootStartScale, 0.2f).SetEase(Ease.InBack));
        closeSeq.OnComplete(() =>
        {
            SetUIActive(false);
            StopAllFx();
        });
    }

    private void SkipCurrent()
    {
        SoundMgr.Inst.PlaySfx(eSfxCategory.UI, "Button");

        // 현재 아이템 연출을 스킵하고 다음으로
        KillAllTweens();
        _isPlaying = false;
        GoNext();
    }

    private void PrepareUIFor(ItemData data)
    {
        itemNameText.text = data.name;
        rarityText.text = GeteRarityLabel(data.rarity);
        priceText.text = FormatPrice(data.price);
        itemImage.sprite = data.icon;
    }

    private void SetUIActive(bool active)
    {
        popupRoot.gameObject.SetActive(active);
        if (dimBackground) dimBackground.gameObject.SetActive(active);
    }

    private void KillAllTweens()
    {
        _seq?.Kill();
        _seq = null;

        _glowLoop?.Kill();
        _glowLoop = null;

        // 안전하게 특정 타겟의 트윈 제거(선택)
        DOTween.Kill(popupCanvasGroup);
        DOTween.Kill(popupRoot);
        DOTween.Kill(itemImage);
        DOTween.Kill(itemImage.transform);
        if (dimBackground) DOTween.Kill(dimBackground);
        if (rarityGlow) DOTween.Kill(rarityGlow);
    }

    private void StartGlowLoop(Color baseColor)
    {
        if (!rarityGlow) return;

        rarityGlow.color = baseColor.WithA(0.0f);
        _glowLoop = rarityGlow.DOFade(0.5f, pulseLoopDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void PlayeRarityFx(eRarity r)
    {
        switch (r)
        {
            case eRarity.Rare:
                PlayFx(rareFx);
                break;
            case eRarity.Epic:
                PlayFx(rareFx);
                PlayFx(epicFx);
                popupRoot.DOShakeScale(0.6f, strength: 0.05f, vibrato: 12);
                break;
            //case eRarity.Legendary:
            //    PlayFx(legendaryFx);
            //    popupRoot.DOShakeScale(0.8f, strength: 0.08f, vibrato: 15);
            //    break;
        }
    }

    private void PlayeRaritySfx(eRarity r)
    {
        if (!sfxSource) return;
        switch (r)
        {
            case eRarity.Rare:
                if (sfxRare) sfxSource.PlayOneShot(sfxRare);
                break;
            case eRarity.Epic:
                if (sfxEpic) sfxSource.PlayOneShot(sfxEpic);
                break;
            //case eRarity.Legendary:
            //    if (sfxLegendary) sfxSource.PlayOneShot(sfxLegendary);
            //    break;
        }
    }

    private void PlayFx(ParticleSystem fx)
    {
        if (!fx) return;
        fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        fx.Play();
    }

    private void StopAllFx()
    {
        if (_allFx == null) return;
        foreach (var fx in _allFx)
        {
            if (!fx) continue;
            fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private Color GeteRarityColor(eRarity r)
    {
        return r switch
        {
            eRarity.Common => commonColor,
            eRarity.Rare => rareColor,
            eRarity.Epic => epicColor,
            _ => commonColor
        };
    }

    private string GeteRarityLabel(eRarity r)
    {
        return r switch
        {
            eRarity.Common => "일반",
            eRarity.Rare => "희귀",
            eRarity.Epic => "에픽",
            _ => "일반"
        };
    }

    private string FormatPrice(long price)
    {
        // 1,234,567 형태
        return string.Format("{0:N0}", price);
    }
}

// ===== 유틸 확장 =====
public static class ColorExt
{
    public static Color WithA(this Color c, float a) => new Color(c.r, c.g, c.b, a);
}
