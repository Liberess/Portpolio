// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Engine/TriggerBox.h"
#include "PlacedItemArea.h"
#include "PlacedAIArea.generated.h"

/**
 * 
 */

UCLASS()
class AFTERSCHOOLCLEANING_API APlacedAIArea : public ATriggerBox
{
	GENERATED_BODY()

protected:
	virtual void BeginPlay() override;

public:
	APlacedAIArea();

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	EPlacedAreaTag PlacedAreaTag;

	UFUNCTION()
	void OnOverlapBegin(class AActor* OverlappedActor, class AActor* OtherActor);

	UFUNCTION()
	void OnOverlapEnd(class AActor* OverlappedActor, class AActor* OtherActor);

	void OnDrawDebugBox(FColor DebugColor);
};

