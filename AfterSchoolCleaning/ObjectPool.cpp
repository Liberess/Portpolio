// Fill out your copyright notice in the Description page of Project Settings.


#include "ObjectPool.h"
#include "UObject/ConstructorHelpers.h"

// Sets default values for this component's properties
UObjectPool::UObjectPool()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = false;

}


AGraffitiObstacle* UObjectPool::GetPooledObject()
{
	for (AGraffitiObstacle* PoolableActor : Pool)
	{
		if (!PoolableActor->GetActive())
		{
			return PoolableActor;
		}
	}

	return nullptr;
}

// Called when the game starts
void UObjectPool::BeginPlay()
{
	Super::BeginPlay();

	if (PooledObjectSubclass != NULL)
	{
		UWorld* const World = GetWorld();
		if (World)
		{
			for (int i = 0; i < PoolSize; i++)
			{
				AGraffitiObstacle* PoolableActor = World->SpawnActor<AGraffitiObstacle>(PooledObjectSubclass, FVector().ZeroVector, FRotator().ZeroRotator);
				PoolableActor->Deactivate();
				Pool.Add(PoolableActor);
			}
		}
	}
	
}

