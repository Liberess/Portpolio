#include "ItemParent.h"

AItemParent::AItemParent()
{
	PrimaryActorTick.bCanEverTick = true;
}

void AItemParent::BeginPlay()
{
	Super::BeginPlay();
}

void AItemParent::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
}

void AItemParent::OnOverlapBegin(UPrimitiveComponent* OverlappedComp, AActor* OtherActor,
                                 UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep,
                                 const FHitResult& SweepResult)
{
	if (OtherActor && (OtherActor != this) && OtherComp)
	{
		if (OtherActor->ActorHasTag("Player"))
			UseItem();
	}
}

void AItemParent::UseItem()
{
	GEngine->AddOnScreenDebugMessage(-1, 2.0f, FColor::Cyan, TEXT("ItemParent::UseItem"));
	Destroy();
}
