#include "ColorManager.h"
#include "ColorverseWorldSettings.h"
#include "LightObject.h"
#include "NavigationSystem.h"
#include "GameFramework/Character.h"
#include "Kismet/GameplayStatics.h"

bool UColorManager::ShouldCreateSubsystem(UObject* Outer) const
{
	if (!Super::ShouldCreateSubsystem(Outer))
		return false;

	const UWorld* WorldOuter = Cast<UWorld>(Outer);
	if (IsValid(WorldOuter))
	{
		AColorverseWorldSettings* WorldSettings = Cast<AColorverseWorldSettings>(WorldOuter->GetWorldSettings());
		if (IsValid(WorldSettings))
			return WorldSettings->bUseColorManager;
	}

	return false;
}

void UColorManager::InitializeManager()
{
	GameMode = Cast<AColorverseGameMode>(UGameplayStatics::GetGameMode(GetWorld()));

	TArray<AActor*> Actors;
	UGameplayStatics::GetAllActorsOfClass(this, AColorArea::StaticClass(), Actors);
	for (auto Actor : Actors)
	{
		AColorArea* ColorArea = Cast<AColorArea>(Actor);
		ColorAreaMap.Add(ColorArea->PuzzleTag, ColorArea);
	}
}

void UColorManager::SpawnTutorialLightObject()
{
	UNavigationSystemV1* NavSystem = UNavigationSystemV1::GetCurrent(GetWorld());
	
	UClass* GeneratedBP = Cast<UClass>(StaticLoadObject(UClass::StaticClass(), nullptr, TEXT("/Game/Blueprints/BP_LightObject.BP_LightObject_C")));

	auto Character = UGameplayStatics::GetPlayerCharacter(GetWorld(), 0);
	auto Location = Character->GetActorLocation();
	FNavLocation NavLocation;
	
	NavSystem->GetRandomReachablePointInRadius(Location, 100.0f, NavLocation);
	GetWorld()->SpawnActor<ALightObject>(GeneratedBP, NavLocation.Location, Character->GetActorRotation());
}
