#include "ItemSlotWidget.h"
#include "InventoryManager.h"

void UItemSlotWidget::OnClick_Implementation()
{
	if(!ItemData.bIsValid || !ItemData.bIsConsume | bIsMaker)
		return;

	UInventoryManager* InvenMgr = UUserWidget::GetWorld()->GetSubsystem<UInventoryManager>();
	InvenMgr->UseInventoryItem(ItemData);
}

void UItemSlotWidget::UpdateItemSlot(const FItem& Item)
{
	ItemData = Item;

	if(ItemData.bIsValid)
		AmountTxt->SetText(FText::FromString(FString::FromInt(ItemData.Amount)));
	else
		AmountTxt->SetText(FText::FromString(FString(TEXT(""))));
}
