#include "ColorArea.h"

AColorArea::AColorArea()
{
	PrimaryActorTick.bCanEverTick = true;
	
	DefaultRoot = CreateDefaultSubobject<USceneComponent>(TEXT("DefaultRoot"));
	SetRootComponent(DefaultRoot);

	BoxCol = CreateDefaultSubobject<UBoxComponent>(TEXT("Box Collision"));
	BoxCol->InitBoxExtent(FVector(100.0f, 100.0f, 100.0f));
	BoxCol->SetCollisionProfileName(TEXT("Trigger"));
	BoxCol->SetupAttachment(DefaultRoot);

	StaticMesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Static Mesh"));
	StaticMesh->SetCollisionProfileName("NoCollision");
	StaticMesh->SetupAttachment(DefaultRoot);
}

void AColorArea::BeginPlay()
{
	Super::BeginPlay();
}

void AColorArea::SetEnabledPostProcess(bool Active)
{
	check(PostVolume);
	//PostVolume->
	PostVolume->bEnabled = Active;
	OnSetEnabledStageInteract.Broadcast(!Active);
}
