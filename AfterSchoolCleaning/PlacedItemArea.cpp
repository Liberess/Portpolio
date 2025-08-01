#include "PlacedItemArea.h"
#include "DrawDebugHelpers.h"
#include "InteractableItemParent.h"

APlacedItemArea::APlacedItemArea()
{
	OnActorBeginOverlap.AddDynamic(this, &APlacedItemArea::OnOverlapBegin);
	OnActorEndOverlap.AddDynamic(this, &APlacedItemArea::OnOverlapEnd);
}

void APlacedItemArea::BeginPlay()
{
	Super::BeginPlay();

	//OnDrawDebugBox(FColor::Yellow);
}

void APlacedItemArea::OnOverlapBegin(AActor* OverlappedActor, AActor* OtherActor)
{
	if (OtherActor && OtherActor != this)
	{
		AInteractableItemParent* Item = Cast<AInteractableItemParent>(OtherActor);
		if (IsValid(Item))
		{
			Item->CurrentPlacedItemArea = this;
		}
	}
}

void APlacedItemArea::OnOverlapEnd(AActor* OverlappedActor, AActor* OtherActor)
{
	if (OtherActor && OtherActor != this)
	{
		AInteractableItemParent* Item = Cast<AInteractableItemParent>(OtherActor);
		if (IsValid(Item))
		{
			Item->CurrentPlacedItemArea = nullptr;
			//OnDrawDebugBox(FColor::Yellow);
		}
	}
}

/*void APlacedItemArea::OnDrawDebugBox(FColor DebugColor)
{
	DrawDebugBox(GetWorld(), GetActorLocation(), GetComponentsBoundingBox().GetExtent(), DebugColor, true, -1, 0, 1);
}*/
