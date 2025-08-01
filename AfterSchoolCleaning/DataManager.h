#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "DataManager.generated.h"

UCLASS()
class AFTERSCHOOLCLEANING_API UDataManager : public UGameInstanceSubsystem
{
	GENERATED_BODY()

public:
	virtual void Initialize(FSubsystemCollectionBase& Collection) override;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=DataManager)
	TArray<bool> StageClears;
};
