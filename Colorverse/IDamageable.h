#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "IDamageable.generated.h"

USTRUCT(BlueprintType)
struct FDamageMessage
{
	GENERATED_USTRUCT_BODY();

	FDamageMessage() : damager(nullptr), damageAmount(0.0f), hitPoint(FVector::ZeroVector), hitNormal(FVector::ZeroVector), hitImpact(FVector::ZeroVector)
	{};

public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Damage Message")
	AActor* damager = nullptr;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Damage Message")
	float damageAmount = 0.0f;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Damage Message")
	FVector hitPoint = FVector::ZeroVector;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Damage Message")
	FVector hitNormal = FVector::ZeroVector;

	UPROPERTY(EditAnyWhere, BlueprintReadWrite, Category = "Damage Message")
	FVector hitImpact = FVector::ZeroVector;
};