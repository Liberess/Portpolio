#include "MakerWidget.h"
#include "ItemSlotWidget.h"

UMakerWidget::UMakerWidget(const FObjectInitializer& ObjectInitializer)
	: Super(ObjectInitializer)
{
	
}

void UMakerWidget::CreateContainer(int Slots)
{
	for(int i = 0; i < MakerGridPanel->GetChildrenCount(); i++)
	{
		if(UItemSlotWidget* Widget = Cast<UItemSlotWidget>(MakerGridPanel->GetChildAt(i)))
		{
			Widget->bIsMaker = true;
			Widget->ItemLocation = EItemSlotLocationType::Maker;
			Widget->Index = i;
		}
	}
}

void UMakerWidget::UpdateContainer(TArray<FItem> Items)
{
	for (int i = 0; i < MakerGridPanel->GetChildrenCount(); i++)
	{
		if (UItemSlotWidget* ItemSlot = Cast<UItemSlotWidget>(MakerGridPanel->GetChildAt(i)))
		{
			ItemSlot->Index = i;
			const FItem& Item = i < Items.Num() ? Items[i] : FItem();
			ItemSlot->UpdateItemSlot(Item);
			ItemSlot->ThumbnailImg->SetBrushFromTexture(Item.bIsValid ? Item.IconImg : EmptyImg);
			//ItemSlot->ThumbnailBorder->SetBrushFromTexture(Item.bIsValid ? Item.IconImg : EmptyImg);
		}
	}
}
