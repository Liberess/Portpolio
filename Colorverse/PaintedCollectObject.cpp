#include "PaintedCollectObject.h"
#include "InventoryManager.h"

APaintedCollectObject::APaintedCollectObject()
{
	PrimaryActorTick.bCanEverTick = true;

	static ConstructorHelpers::FObjectFinder<UDataTable> DataTable(TEXT("/Game/DataTables/DT_ItemData"));
	if (DataTable.Succeeded())
		ItemDT = DataTable.Object;
	
	static ConstructorHelpers::FObjectFinder<UDataTable> PaintComboTable(TEXT("/Game/DataTables/DT_PaintCombo"));
	if (PaintComboTable.Succeeded())
		PaintComboDT = PaintComboTable.Object;
}

void APaintedCollectObject::BeginPlay()
{
	Super::BeginPlay();

	//AddColorAreaEnabledAction();

	if(IsValid(ItemDT))
	{
		FString RowStr = "Item_";
		RowStr.Append(FString::FromInt(ItemID));
		ItemData = *(ItemDT->FindRow<FItem>(FName(*RowStr), ""));
		InteractWidgetDisplayTxt = ItemData.Name.ToString();
        
		UnlockItemData = *(ItemDT->FindRow<FItem>(FName(TEXT("UnlockStatue")), ""));
	}

	if(IsValid(PaintComboDT))
	{
		FString str = UEnum::GetDisplayValueAsText(PaintComboColorTag).ToString();
		PaintComboData = *(PaintComboDT->FindRow<FPaintCombo>(FName(*str), ""));
	}

	TArray<AActor*> tempActors;
	GetAllChildActors(tempActors, true);
	for (auto actor : tempActors)
	{
		if (auto collectObj = Cast<ACollectObject>(actor))
		{
			collectObj->ItemData = ItemData;
			//collectObj->PaintedCount = 0;
			//collectObj->PaintComboData = PaintComboData;
			//collectObj->NeedsPaintedCount = PaintComboData.ComboColors.Num();
			//collectObj->SetCollectObjectData(SeparatedItemName);
			CollectObjects.Add(collectObj);
		}
	}

	for (int i = 0; i < CollectObjects.Num(); i++)
	{
		FTimerHandle newHandle;
		SpawnTimerHandles.Add(newHandle);
	}
}

void APaintedCollectObject::OnInteract_Implementation()
{
	Super::OnInteract_Implementation();

	if (IsColorActive)
	{
		for (int i = 0; i < CollectObjects.Num(); i++)
		{
			if (!CollectObjects[i]->IsHidden())
			{
				//이제 달린 사과가 없으니 상호작용 끔
				if(i == CollectObjects.Num() - 1)
					IsInteractable = false;
				
				SetActiveCollectObject(false, i);
				GetWorldTimerManager().SetTimer(SpawnTimerHandles[i], FTimerDelegate::CreateLambda([i, this]
				{
					SetActiveCollectObject(true, i);
				}), SpawnDelayTime, false);

				UInventoryManager* InvenMgr = GetWorld()->GetSubsystem<UInventoryManager>();
				
				float Percentage_Chance = GetSacrificeProbability / 100.0f;
				
				int RandAccuracy = 10000000;
				float RandHitRange = Percentage_Chance * RandAccuracy;
				int Rand = FMath::RandRange(1, RandAccuracy + 1);

				if (Rand <= RandHitRange)
					InvenMgr->AddInventoryItem(UnlockItemData);
				else
					InvenMgr->AddInventoryItem(ItemData);

				break;
			}
		}
	}
}

void APaintedCollectObject::PaintToObject_Implementation(ECombineColors colorTag, FLinearColor PaintedColor)
{
	if (IsColorActive)
		return;

	for (auto& collectObj : CollectObjects)
	{
		/*if(!collectObj->bIsPaintComplete)
			collectObj->SetPaintedColorAndIntensity(colorTag, PaintedColor);*/
	}
}

void APaintedCollectObject::SetActiveCollectObject(bool active, int index)
{
	if (index < 0 || index >= CollectObjects.Num())
		return;

	if(active)
	{
		CollectObjects[index]->bIsGrown = true;
		CollectObjects[index]->TargetScale = FVector::OneVector;
	}
	else
	{
		CollectObjects[index]->bIsGrown = true;
		CollectObjects[index]->TargetScale = FVector::ZeroVector;
	}

	// active가 true라면 CollectObject를 활성화한다.
	CollectObjects[index]->SetActorHiddenInGame(!active);
	CollectObjects[index]->SetActorEnableCollision(active);
	CollectObjects[index]->SetActorTickEnabled(active);
}

void APaintedCollectObject::SetChildCollectObjectTexture(UTexture2D* texture)
{
	/*for (auto& collectObj : CollectObjects)
		collectObj->SetBaseTexture(texture);*/
}

void APaintedCollectObject::SetRecoveryColorComplete(ECombineColors color)
{
	if(IsInteractable && IsColorActive)
		return;
	
	IsInteractable = true;
	IsColorActive = true;

	SetChildCollectObjectTexture(ChildActiveTexture);

	FTimerHandle timer;
	GetWorldTimerManager().SetTimer(timer, FTimerDelegate::CreateLambda([&]
	{
		GroupMatInst->SetVectorParameterValue("OverlayColor", GroupActiveColor);
		PaintingMatInst->SetTextureParameterValue("BaseTexture", ActiveTexture);
		PaintingMatInst->SetVectorParameterValue("OverlayColor", FColor::White);
		GetWorldTimerManager().ClearTimer(timer);
	}), 2.0f, false);
}
