// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "PooledObject.h"
#include "MyObjectPool.generated.h"


UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class COLORVERSE_API UMyObjectPool : public UActorComponent
{
	GENERATED_BODY()

public:
	UMyObjectPool();

	APooledObject* GetPooledObject();

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

public:
	UPROPERTY(EditAnywhere, Category = "ObjectPooler")
	TSubclassOf<class APooledObject> PooledObjectSubclass;

	UPROPERTY(EditAnywhere, Category = "ObjectPooler")
	int PoolSize = 5;

	TArray<APooledObject*> Pool;
};
