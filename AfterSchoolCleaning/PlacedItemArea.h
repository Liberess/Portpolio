#pragma once

#include "CoreMinimal.h"
#include "Engine/TriggerBox.h"
#include "PlacedItemArea.generated.h"

UENUM(BlueprintType)
enum class EPlacedAreaTag : uint8
{
	A UMETA(DisplayName = "A"),
	B UMETA(DisplayName = "B"),
	C UMETA(DisplayName = "C"),
	D UMETA(DisplayName = "D")
};

UCLASS()
class AFTERSCHOOLCLEANING_API APlacedItemArea : public ATriggerBox
{
	GENERATED_BODY()

protected:
	virtual void BeginPlay() override;

public:
	APlacedItemArea();

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	EPlacedAreaTag PlacedAreaTag;

	UFUNCTION()
	void OnOverlapBegin(class AActor* OverlappedActor, class AActor* OtherActor);

	UFUNCTION()
	void OnOverlapEnd(class AActor* OverlappedActor, class AActor* OtherActor);

	//void OnDrawDebugBox(FColor DebugColor);
};
