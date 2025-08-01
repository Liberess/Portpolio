#include "ItemAcquiredWidget.h"
#include "InventoryManager.h"

void UItemAcquiredWidget::SetupItemWidget(const FItem& _ItemData)
{
	if (bIsViewComplete)
		return;

	ItemData = _ItemData;

	UpdateItemInformation();
	SetupItemLogTimer();
}

void UItemAcquiredWidget::SetupItemLogTimer()
{
	PlayAnimation(ShowAnim);
	GetWorld()->GetTimerManager().ClearTimer(RemoveLogTimer);
	GetWorld()->GetTimerManager().SetTimer(RemoveLogTimer, [this]()
	{
		if (LogIndex >= 3)
			return;

		StopAnimation(ShowAnim);
		PlayAnimation(HideAnim);

		GetWorld()->GetTimerManager().SetTimer(RemoveLogTimer, FTimerDelegate::CreateLambda([&]()
		{
			bIsViewComplete = true;
			ReleaseItemLogWidget();
		}), 1.0f, false);
	}, 2.0f, false);
}

void UItemAcquiredWidget::UpdateItemInformation()
{
	ItemImg->SetBrushFromTexture(ItemData.IconImg);
	ItemNameTxt->SetText(ItemData.Name);
	ItemAmountTxt->SetText(FText::FromString("x" + FString::FromInt(ItemData.Amount)));
}

void UItemAcquiredWidget::UpdateItemLogIndex()
{
	if (bIsViewComplete)
		return;

	++LogIndex;
	if (LogIndex >= 4)
	{
		bIsViewComplete = true;
		StopAnimation(ShowAnim);
		StopAnimation(HideAnim);
		PlayAnimation(HideAnim, 0.0f, 1, EUMGSequencePlayMode::Forward, 4.0f);
		GetWorld()->GetTimerManager().ClearTimer(RemoveLogTimer);
		GetWorld()->GetTimerManager().SetTimer(RemoveLogTimer, [this]()
		{
			ReleaseItemLogWidget();
		}, 1.0f, false);
	}
	else if (LogIndex >= 3)
	{
		GetWorld()->GetTimerManager().ClearTimer(RemoveLogTimer);
		GetWorld()->GetTimerManager().SetTimer(RemoveLogTimer, [this]()
		{
			if (LogIndex >= 4)
				return;

			bIsViewComplete = true;
			StopAnimation(ShowAnim);
			PlayAnimation(HideAnim, 0.0f, 1, EUMGSequencePlayMode::Forward, 2.0f);
			
			GetWorld()->GetTimerManager().SetTimer(RemoveLogTimer,
			                                       FTimerDelegate::CreateUObject(
				                                       this, &UItemAcquiredWidget::ReleaseItemLogWidget), 1.0f,
			                                       false);
		}, 1.0f, false);
	}
}

void UItemAcquiredWidget::ReleaseItemLogWidget()
{
	//SetVisibility(ESlateVisibility::Collapsed);

	UInventoryManager* InvenMgr = GetWorld()->GetSubsystem<UInventoryManager>();
	if (InvenMgr)
	{
		ItemData = FItem();
		InvenMgr->GetHUDWidget()->ReleaseItemLogWidget(this);
	}

	GetWorld()->GetTimerManager().ClearTimer(RemoveLogTimer);
}
