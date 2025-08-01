#pragma once

#include "CoreMinimal.h"
#include "Enums.h"
#include "InteractObject.h"
#include "Components/BoxComponent.h"
#include "Engine/PostProcessVolume.h"
#include "ColorArea.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnSetEnabledStageInteract, bool, IsEnabled);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnDisabledStageInteract);

UCLASS(Blueprintable, BlueprintType)
class COLORVERSE_API AColorArea : public AActor
{
	GENERATED_BODY()

protected:
	UPROPERTY(BlueprintReadOnly)
	USceneComponent* DefaultRoot = nullptr;
	
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	UBoxComponent* BoxCol;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	UStaticMeshComponent* StaticMesh;

protected:
	virtual void BeginPlay() override;
	
public:
	AColorArea();

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category=ColorArea)
	EPuzzleTag PuzzleTag = EPuzzleTag::Puzzle_Red;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="ColorArea|Effect")
	APostProcessVolume* PostVolume;

	UFUNCTION(BlueprintCallable)
	void SetEnabledPostProcess(bool Active);

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=ColorArea)
	bool IsLightness = false;
	
	UPROPERTY(BlueprintAssignable, BlueprintCallable, Category=ColorArea)
	FOnSetEnabledStageInteract OnSetEnabledStageInteract;
	
	UPROPERTY(BlueprintAssignable, BlueprintCallable, Category=ColorArea)
	FOnDisabledStageInteract OnDisabledStageInteract;
};
