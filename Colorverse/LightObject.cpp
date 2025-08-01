#include "LightObject.h"
#include "ColorManager.h"
#include "ColorverseCharacter.h"

ALightObject::ALightObject()
{
	PrimaryActorTick.bCanEverTick = true;
}

void ALightObject::BeginPlay()
{
	Super::BeginPlay();
	
	IsInteractable = true;
}

void ALightObject::OnEnter_Implementation()
{
	//Print(1.0f, TEXT("LightObject::OnEnter"));
}

void ALightObject::OnInteract_Implementation()
{
	//Super::OnInteract();

	UColorManager* ColorMgr = GetWorld()->GetSubsystem<UColorManager>();
	check(ColorMgr);
	//ColorMgr->SetLightAmount(PuzzleTag, 1);
	Destroy();
}

void ALightObject::OnExit_Implementation()
{
	//Print(1.0f, TEXT("LightObject::OnExit"));
}
