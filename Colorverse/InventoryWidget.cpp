#include "InventoryWidget.h"
#include "ItemSlotWidget.h"

UInventoryWidget::UInventoryWidget(const FObjectInitializer& ObjectInitializer)
	: Super(ObjectInitializer)
{

}

void UInventoryWidget::CreateContainer(int Slots)
{
	for(int i = 0; i < InventoryGridPanel->GetChildrenCount(); i++)
	{
		if(UItemSlotWidget* Widget = Cast<UItemSlotWidget>(InventoryGridPanel->GetChildAt(i)))
		{
			Widget->bIsMaker = false;
			Widget->ItemLocation = EItemSlotLocationType::Inventory;
			Widget->Index = i;
		}
	}

	for(int i = 0; i < MakerGridPanel->GetChildrenCount(); i++)
	{
		if(UItemSlotWidget* Widget = Cast<UItemSlotWidget>(MakerGridPanel->GetChildAt(i)))
		{
			Widget->bIsMaker = true;
			Widget->ItemLocation = EItemSlotLocationType::Maker;
			Widget->Index = i;
			Widget->ThumbnailImg->SetVisibility(ESlateVisibility::Hidden);
			ResultImg->SetVisibility(ESlateVisibility::Hidden);
		}
	}
}

void UInventoryWidget::UpdateContainer(TArray<FItem> Items)
{
	for (int i = 0; i < InventoryGridPanel->GetChildrenCount(); i++)
	{
		if (UItemSlotWidget* ItemSlot = Cast<UItemSlotWidget>(InventoryGridPanel->GetChildAt(i)))
		{
			ItemSlot->Index = i;
			const FItem& Item = i < Items.Num() ? Items[i] : FItem();
			ItemSlot->UpdateItemSlot(Item);
			ItemSlot->ThumbnailImg->SetBrushFromTexture(Item.bIsValid ? Item.IconImg : EmptyImg);
			ItemSlot->ThumbnailImg->SetVisibility(Item.bIsValid ? ESlateVisibility::Visible : ESlateVisibility::Hidden);
		}
	}
}

void UInventoryWidget::UpdateMakerContainer(TArray<FItem> Items)
{
	for (int i = 0; i < MakerGridPanel->GetChildrenCount(); i++)
	{
		if (UItemSlotWidget* ItemSlot = Cast<UItemSlotWidget>(MakerGridPanel->GetChildAt(i)))
		{
			ItemSlot->Index = i;
			const FItem& Item = i < Items.Num() ? Items[i] : FItem();
			ItemSlot->UpdateItemSlot(Item);
			ItemSlot->ThumbnailImg->SetBrushFromTexture(Item.bIsValid ? Item.IconImg : EmptyImg);
			ItemSlot->ThumbnailImg->SetVisibility(Item.bIsValid ? ESlateVisibility::Visible : ESlateVisibility::Hidden);
		}
	}
}

void UInventoryWidget::SetCombineResultUI(const FItem& Item, bool IsAlreadyCombine)
{
	if(Item.bIsValid)
	{
		ResultImg->SetBrushFromTexture(Item.IconImg);
		ResultImg->SetVisibility(ESlateVisibility::Visible);
		
		if(IsAlreadyCombine)
			ResultImg->SetColorAndOpacity(FLinearColor(1.0f, 1.0f, 1.0f, 1.0f));
		else
			ResultImg->SetColorAndOpacity(FLinearColor(0.0f, 0.0f, 0.0f, 1.0f));
	}
	else
	{
		ResultImg->SetVisibility(ESlateVisibility::Hidden);
	}
}
