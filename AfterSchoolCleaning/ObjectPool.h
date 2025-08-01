// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "GraffitiObstacle.h"
#include "ObjectPool.generated.h"


UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class AFTERSCHOOLCLEANING_API UObjectPool : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UObjectPool();

	AGraffitiObstacle* GetPooledObject();

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

public:	
	UPROPERTY(EditAnywhere, Category = "ObjectPooler")
	TSubclassOf<class AGraffitiObstacle> PooledObjectSubclass;

	UPROPERTY(EditAnywhere, Category = "ObjectPooler")
	int PoolSize = 100;

	TArray<AGraffitiObstacle*> Pool;
};
