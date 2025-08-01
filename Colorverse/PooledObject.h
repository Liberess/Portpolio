// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Components/BoxComponent.h"
#include "PooledObject.generated.h"

UCLASS()
class COLORVERSE_API APooledObject : public AActor
{
	GENERATED_BODY()
	
public:	
	// Sets default values for this actor's properties
	APooledObject();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Mesh", meta = (AllowPrivateAccess = "true"))
	UStaticMeshComponent* mesh;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Collision", meta = (AllowPrivateAccess = "true"))
	UBoxComponent* collision;

public:	
	virtual void Tick(float DeltaTime) override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	bool active;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (ClampMin = 0, ClampMax = 50))
	float ProgressValue;

	UPROPERTY(BlueprintReadWrite)
	bool IsInteractable;

	void SetActive(bool InActive);

	void Deactivate();

	UFUNCTION(BlueprintNativeEvent)
	void ActiveTrueEvent();

	UFUNCTION(BlueprintNativeEvent, BlueprintCallable)
	void CreatePooledObject(FVector location, FRotator rotator);

	UFUNCTION(BlueprintNativeEvent, BlueprintCallable)
	void DistroyPooledObject();

	FORCEINLINE class UStaticMeshComponent* GetStaticMesh() { return mesh; }
	FORCEINLINE class UBoxComponent* GetBoxCollision() { return collision; }
	FORCEINLINE bool GetActive() { return active; }

protected:
	void Init();
};