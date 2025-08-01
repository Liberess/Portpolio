#pragma once

#include "CoreMinimal.h"
#include "Engine/PostProcessVolume.h"
#include "GameFramework/GameModeBase.h"
#include "GraffitiObstacle.h"
#include "AfterSchoolCleaningGameModeBase.generated.h"

UENUM(BlueprintType)
enum class EObstacleType : uint8
{
	WallObs = 0	UMETA(DisplayName = "Obstacle Wall"),
	FloorObs	UMETA(DisplayName = "Obstacle Floor"),
};

UCLASS()
class AFTERSCHOOLCLEANING_API AAfterSchoolCleaningGameModeBase : public AGameModeBase
{
	GENERATED_BODY()

private:
	AAfterSchoolCleaningGameModeBase();

public:
	virtual void BeginPlay() override;

	UFUNCTION(BlueprintCallable)
	void SetOutlinePostProcess(bool Active, float Duration);

	UFUNCTION(BlueprintCallable)
	void SetObstacleDebuff(EObstacleType ObsType, bool Active, float Duration);

	UFUNCTION(BlueprintImplementableEvent, BlueprintCallable)
	void SetBuffUI(EObstacleType ObsType, float Duration);

	FTimerHandle WallDebuffTimer;
	FTimerHandle FloorDebuffTimer;
	FTimerHandle SeeOutlineTimer;

	APostProcessVolume* PostVolume;

	ASweeper* Sweeper;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite)
	UUserWidget* CurrentWidget;

	UFUNCTION(BlueprintCallable)
	void ChangeMenuWidget(TSubclassOf<UUserWidget> NewWidget);
	
protected:
	UPROPERTY(EditAnywhere, BlueprintReadOnly)
	TSubclassOf<UUserWidget> StartingWidget;
};
