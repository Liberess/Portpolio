#pragma once

#include "CoreMinimal.h"
#include "Enums.h"
#include "Components/ArrowComponent.h"
#include "GameFramework/Actor.h"
#include "PaintableObject.generated.h"

UCLASS(Blueprintable)
class COLORVERSE_API APaintableObject : public AActor
{
	GENERATED_BODY()

protected:
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	USceneComponent* DefaultRoot = nullptr;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	UStaticMeshComponent* StaticMesh;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	UArrowComponent* Arrow;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Paintable Object")
	int ID = 0;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Paintable Object")
	EPuzzleTag PuzzleTag = EPuzzleTag::Puzzle_Red;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Paintable Object")
	ECombineColors TargetColorTag = ECombineColors::Empty;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Paintable Object")
	ECombineColors CurrentColorTag = ECombineColors::Empty;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Paintable Object")
	bool bIsRightColor;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category="Paintable Object")
	bool bIsColorChanged;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category="Paintable Object")
	bool bIsPaintedComplete = false;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Paintable Object")
	bool bIsInteractable = true;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Paintable Object")
	FLinearColor TargetColor;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Paintable Object")
	FLinearColor CurrentColor;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Paintable Object")
	int PaintedCount;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Paintable Object")
	int PaintedCapacity = 3;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Paintable Object") 
	UMaterialInterface* PaintingMatTemplate;
	
	UPROPERTY(BlueprintReadWrite, Category="Paintable Object")
	UMaterialInstanceDynamic* PaintingMatInst;

	virtual void BeginPlay() override;
	
	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent, Category="Paintable Object")
	void CompletePainted();

public:
	APaintableObject();
	
	virtual void Tick(float DeltaTime) override;

	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category="Paintable Object")
	void PaintToObject(FLinearColor _PaintColor, ECombineColors _CurrentColorTag);
};
