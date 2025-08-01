#pragma once

#include "CoreMinimal.h"
#include "ColorArea.h"
#include "Enums.h"
#include "ColorverseGameMode.h"
#include "Subsystems/WorldSubsystem.h"
#include "ColorManager.generated.h"

UCLASS()
class COLORVERSE_API UColorManager : public UWorldSubsystem
{
	GENERATED_BODY()

private:
	UPROPERTY(VisibleAnywhere, meta=(AllowPrivateAccess))
	TMap<EPuzzleTag, AColorArea*> ColorAreaMap;

	AColorverseGameMode* GameMode;
	
public:
	virtual bool ShouldCreateSubsystem(UObject* Outer) const override;

	UFUNCTION(BlueprintCallable)
	void InitializeManager();

	UFUNCTION(BlueprintCallable)
	void SpawnTutorialLightObject();
};
