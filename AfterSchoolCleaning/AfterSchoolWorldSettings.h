#pragma once

#include "CoreMinimal.h"
#include "GameFramework/WorldSettings.h"
#include "AfterSchoolWorldSettings.generated.h"

UCLASS()
class AFTERSCHOOLCLEANING_API AAfterSchoolWorldSettings : public AWorldSettings
{
	GENERATED_BODY()

public:
	AAfterSchoolWorldSettings();

	UPROPERTY(EditAnywhere, Category="AfterSchoolWorldSettings | Cleaning System")
	bool bUseCleaningManager;

	UPROPERTY(EditAnywhere, Category="AfterSchoolWorldSettings | Counting System")
	bool bUseCountManager;
};
