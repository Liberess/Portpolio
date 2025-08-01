#include "FruitTree.h"
#include "InventoryManager.h"

AFruitTree::AFruitTree()
{
	PrimaryActorTick.bCanEverTick = true;

	static ConstructorHelpers::FObjectFinder<UDataTable> DataTable(TEXT("/Game/DataTables/DT_ItemData"));
	if (DataTable.Succeeded())
		ItemDT = DataTable.Object;

	SetRootComponent(StaticMesh);
	BoxCol->SetupAttachment(StaticMesh);
}

void AFruitTree::BeginPlay()
{
	Super::BeginPlay();

	IsInteractable = true;

	if(IsValid(ItemDT))
	{
		ItemData = *(ItemDT->FindRow<FItem>(ItemName, ""));
		WoodStickData = *(ItemDT->FindRow<FItem>(FName(TEXT("WoodStick")), ""));
		InteractWidgetDisplayTxt = ItemData.Name.ToString();
	}
}

void AFruitTree::OnInteract_Implementation()
{
	IIInteractable::OnInteract_Implementation();

	if(!IsInteractable)
		return;
	
	IsInteractable = false;

	UInventoryManager* InvenMgr = GetWorld()->GetSubsystem<UInventoryManager>();
	
	//흔들리는 Animation 출력

	//나뭇가지 획득
	WoodStickData.Amount = FMath::RandRange(1, MaxWoodStickAcquireAmount);
	InvenMgr->AddInventoryItem(WoodStickData);

	SetActiveCollectObject(false);

	ItemData.Amount = FMath::RandRange(1, MaxItemAcquireAmount);
		
	InvenMgr->AddInventoryItem(ItemData);

	FruitSpawnDelayTime = FMath::RandRange(10.0f, 60.0f);
	GetWorldTimerManager().ClearTimer(SpawnTimerHandle);
	GetWorldTimerManager().SetTimer(SpawnTimerHandle, FTimerDelegate::CreateLambda([this]
	{
		IsInteractable = true;
		SetActiveCollectObject(true);
	}), FruitSpawnDelayTime, false);
}
