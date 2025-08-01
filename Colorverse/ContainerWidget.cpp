#include "ContainerWidget.h"
#include "InventoryManager.h"

UContainerWidget::UContainerWidget(const FObjectInitializer& ObjectInitializer)
	: Super(ObjectInitializer)
{
	static ConstructorHelpers::FObjectFinder<UTexture2D> EmptyImgObj(
		TEXT("/Game/UI/UI_Renewal/Inventory/Box_normal.Box_normal"));
	if (EmptyImgObj.Object != nullptr)
		EmptyImg = EmptyImgObj.Object;
}

void UContainerWidget::MoveItem(TArray<FItem>& SelectAry, TArray<FItem>& DropAry, bool IsMoveBetween)
{
	UInventoryManager* InvenMgr = GetWorld()->GetSubsystem<UInventoryManager>();
	check(InvenMgr)

	SelectItem = SelectAry[SelectItemIndex];
	DropItem = DropAry[DropItemIndex];

	const static FItem EmptyItem = FItem();
	
	const bool IsSelectMaker = SelectAry.Num() <= 2;
	const bool IsSameCombineType = SelectItem.CombineType == DropItem.CombineType;

	//SelectItem이 Src고 Drop할 곳도 Src
	const bool IsSourceSwap = SelectItem.CombineType == EItemCombineType::Source && DropItemIndex == 0;
	//SelectItem이 Dest고 Drop할 곳도 Dest
	const bool IsDestinationSwap = SelectItem.CombineType == EItemCombineType::Destination && DropItemIndex == 1;

	if (IsMoveBetween)
	{
		if (DropItem.bIsValid)
		{
			if (SelectItem.Name.EqualTo(DropItem.Name))
			{
				DropAry[DropItemIndex] = DropItem;
				DropAry[DropItemIndex].Amount = SelectItem.Amount + DropItem.Amount;
				SelectAry[SelectItemIndex] = EmptyItem;
			}
			else if (IsSameCombineType)
			{
				DropAry[DropItemIndex] = SelectItem;
				SelectAry[SelectItemIndex] = DropItem;
			}
		}
		else if (IsSelectMaker) //Maker -> Inventory
		{
			DropAry[DropItemIndex] = SelectItem;
			SelectAry[SelectItemIndex] = EmptyItem;
		}
		else if (IsSourceSwap || IsDestinationSwap)
		{
			DropAry[DropItemIndex] = SelectItem;
			SelectAry[SelectItemIndex] = EmptyItem;
		}
	}
	else
	{
		if (SelectItemIndex != DropItemIndex)
		{
			if (DropItem.bIsValid)
			{
				if (SelectItem.Name.EqualTo(DropItem.Name))
				{
					DropAry[DropItemIndex] = DropItem;
					DropAry[DropItemIndex].Amount = SelectItem.Amount + DropItem.Amount;
					SelectAry[SelectItemIndex] = EmptyItem;
				}
				else
				{
					DropAry[DropItemIndex] = SelectItem;
					SelectAry[SelectItemIndex] = DropItem;
				}
			}
			else
			{
				DropAry[DropItemIndex] = SelectItem;
				SelectAry[SelectItemIndex] = EmptyItem;
			}
		}
	}
}

void UContainerWidget::SetItemSlotArrays()
{
	UInventoryManager* InvenMgr = GetWorld()->GetSubsystem<UInventoryManager>();
	check(InvenMgr)

	switch (SelectLocation)
	{
	case EItemSlotLocationType::Inventory:
		if (DropLocation == EItemSlotLocationType::Maker)
		{
			MoveItem(InvenMgr->InventoryArray, InvenMgr->MakerArray, true);
			InvenMgr->UpdateInventory();
			InvenMgr->UpdateMaker();
		}
		else
		{
			MoveItem(InvenMgr->InventoryArray, InvenMgr->InventoryArray, false);
			InvenMgr->UpdateInventory();
		}
		break;

	case EItemSlotLocationType::Maker:
		if (DropLocation == EItemSlotLocationType::Inventory)
		{
			MoveItem(InvenMgr->MakerArray, InvenMgr->InventoryArray, true);
			InvenMgr->UpdateInventory();
			InvenMgr->UpdateMaker();
		}
		break;
	}
}
