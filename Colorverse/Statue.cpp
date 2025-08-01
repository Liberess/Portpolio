#include "Statue.h"

void AStatue::BeginPlay()
{
	Super::BeginPlay();

	bIsUnlock = false;
	IsInteractable = true;
}

void AStatue::OnEnter_Implementation()
{
	
}

void AStatue::OnInteract_Implementation()
{
	if(bIsUnlock || !IsInteractable)
		return;

	bIsUnlock = true;
	IsInteractable = false;
	StaticMesh->SetRenderCustomDepth(false);
}

void AStatue::OnExit_Implementation()
{
	
}