#include "InteractObject.h"
#include "ColorArea.h"
#include "ColorverseCharacter.h"
#include "Kismet/GameplayStatics.h"

AInteractObject::AInteractObject()
{
	PrimaryActorTick.bCanEverTick = true;
	
	DefaultRoot = CreateDefaultSubobject<USceneComponent>(TEXT("DefaultRoot"));
	SetRootComponent(DefaultRoot);

	StaticMesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Static Mesh"));
	StaticMesh->SetupAttachment(DefaultRoot);
	
	BoxCol = CreateDefaultSubobject<UBoxComponent>(TEXT("Box Collision"));
	BoxCol->InitBoxExtent(FVector(100.0f, 100.0f, 100.0f));
	BoxCol->SetCollisionProfileName(TEXT("Trigger"));
	BoxCol->SetupAttachment(DefaultRoot);

	Arrow = CreateDefaultSubobject<UArrowComponent>(TEXT("Arrow"));
	Arrow->ArrowLength = 150.0f;
	Arrow->SetupAttachment(DefaultRoot);
	
	BoxCol->OnComponentBeginOverlap.AddDynamic(this, &AInteractObject::OnOverlapBegin);
	BoxCol->OnComponentEndOverlap.AddDynamic(this, &AInteractObject::OnOverlapEnd);
}

void AInteractObject::BeginPlay()
{
	Super::BeginPlay();
}

void AInteractObject::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
}

void AInteractObject::OnOverlapBegin(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	if (OtherActor && OtherActor != this)
	{
		static AColorverseCharacter* Character = Cast<AColorverseCharacter>(OtherActor);
		if(IsValid(Character))
			OnEnter_Implementation();
	}
}

void AInteractObject::OnOverlapEnd(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex)
{
	if (OtherActor && OtherActor != this)
	{
		static AColorverseCharacter* Character = Cast<AColorverseCharacter>(OtherActor);
		if(IsValid(Character))
			OnExit_Implementation();
	}
}

void AInteractObject::OnEnter_Implementation()
{
	IIInteractable::OnEnter_Implementation();
}

void AInteractObject::OnInteract_Implementation()
{
	IIInteractable::OnInteract_Implementation();
}

void AInteractObject::OnExit_Implementation()
{
	IIInteractable::OnExit_Implementation();
}
