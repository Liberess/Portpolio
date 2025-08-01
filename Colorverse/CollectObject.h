#pragma once

#include "CoreMinimal.h"
#include "IItem.h"
#include "InteractObject.h"
#include "CollectObject.generated.h"

UCLASS(Blueprintable)
class COLORVERSE_API ACollectObject : public AInteractObject
{
	GENERATED_BODY()

protected:
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Collect Object", meta=(AllowPrivateAccess))
	UDataTable* ItemDT;

	/*
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Collect Object", meta=(AllowPrivateAccess))
	UMaterialInterface* ObjMatTemplate;

	UPROPERTY(BlueprintReadWrite, Category="Collect Object", meta=(AllowPrivateAccess))
	UMaterialInstanceDynamic* ObjMatInst;
	*/

	virtual void BeginPlay() override;
	virtual void Tick(float DeltaSeconds) override;
	virtual void OnInteract_Implementation() override;

public:
	ACollectObject();

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Collect Object")
	FName ItemName;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category="Collect Object")
	FItem ItemData;

	UFUNCTION(BlueprintCallable)
	void SetCollectObjectData(FName _itemName);

	UPROPERTY(BlueprintReadWrite)
	bool bIsGrown = false;

	UPROPERTY(BlueprintReadWrite, Category="Collect Object")
	float GlownVelocity = 2.0f;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category="Collect Object")
	float RespawnTime = 50.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Collect Object")
	float MaxRespawnTime = 120.0f;

	UPROPERTY(BlueprintReadWrite)
	FVector CurrentScale;

	UPROPERTY(BlueprintReadWrite)
	FVector TargetScale;

	FTimerHandle RespawnHandle;

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
	void ActiveGlown();
};
