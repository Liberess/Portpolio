#include "InteractableItemParent.h"

#include "CleaningManager.h"
#include "Sweeper.h"
#include "Kismet/GameplayStatics.h"

AInteractableItemParent::AInteractableItemParent()
{
	PrimaryActorTick.bCanEverTick = true;

	IsRotate = false;
	IsGrounded = true;
	IsInteractable = true;

	ItemMesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("ItemMesh"));
	static ConstructorHelpers::FObjectFinder<UStaticMesh> _ItemMesh(
		TEXT("StaticMesh'/Engine/BasicShapes/Sphere.Sphere'"));

	if (_ItemMesh.Succeeded())
		ItemMesh->SetStaticMesh(_ItemMesh.Object);

	RootComponent = ItemMesh;
}

void AInteractableItemParent::BeginPlay()
{
	Super::BeginPlay();

	OriginVector = GetActorLocation();
}

void AInteractableItemParent::NotifyHit(UPrimitiveComponent* MyComp, AActor* Other, UPrimitiveComponent* OtherComp,
                                        bool bSelfMoved, FVector HitLocation, FVector HitNormal, FVector NormalImpulse,
                                        const FHitResult& Hit)
{
	if(!IsInteractable)
		return;
	
	Super::NotifyHit(MyComp, Other, OtherComp, bSelfMoved, HitLocation, HitNormal, NormalImpulse, Hit);

	if (IsValid(Other))
	{
		if(Other->ActorHasTag("Ground"))
		{
			IsGrounded = true;

			if(IsValid(CurrentPlacedItemArea))
			{
				if(CurrentPlacedItemArea->PlacedAreaTag == PlacedAreaTag)
					CompleteOrganize();
				else
					ResetLocation();
			}
		}
	}
}

void AInteractableItemParent::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
}

void AInteractableItemParent::CompleteOrganize_Implementation()
{
	IsInteractable = false;
	ItemMesh->SetCollisionProfileName("BlockAll");
	//ItemMesh->SetSimulatePhysics(false);
}

void AInteractableItemParent::ResetLocation_Implementation()
{
	ASweeper* Sweeper = Cast<ASweeper>(UGameplayStatics::GetPlayerPawn(GetWorld(), 0));
	check(Sweeper);

	Sweeper->PutObject();

	ItemMesh->SetSimulatePhysics(false);
	ItemMesh->SetCollisionEnabled(ECollisionEnabled::NoCollision);

	SetActorRelativeLocation(OriginVector);

	ItemMesh->SetSimulatePhysics(true);
	ItemMesh->SetCollisionEnabled(ECollisionEnabled::QueryAndPhysics);
}
