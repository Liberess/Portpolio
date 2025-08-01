#include "CleaningManager.h"
#include "AfterSchoolWorldSettings.h"

bool UCleaningManager::ShouldCreateSubsystem(UObject* Outer) const
{
	if(!Super::ShouldCreateSubsystem(Outer))
		return false;

	UWorld* WorldOuter = Cast<UWorld>(Outer);
	if(IsValid(WorldOuter))
	{
		AAfterSchoolWorldSettings* WorldSettings = Cast<AAfterSchoolWorldSettings>(WorldOuter->GetWorldSettings());
		if(IsValid(WorldSettings))
			return WorldSettings->bUseCleaningManager;
	}
	
	return false;
}

void UCleaningManager::Initialize(FSubsystemCollectionBase& Collection)
{
	Super::Initialize(Collection);

	TotalProgress = 0.0f;
	OrganizeProgress = 0.0f;
	RemoveProgress = 0.0f;

	DataManager = UGameInstance::GetSubsystem<UDataManager>(GetWorld()->GetGameInstance());
}

void UCleaningManager::SyncProgress()
{
	TotalProgress = OrganizeProgress + RemoveProgress;
	
	FString Text = FString::SanitizeFloat(TotalProgress);
}

void UCleaningManager::StageClear(EStageName StageName)
{
	DataManager->StageClears[static_cast<int>(StageName)] = true;
}

void UCleaningManager::SetProgress_Implementation(EProgressType ProgressType, float Progress)
{
	if(ProgressType == EProgressType::Organize)
	{
		OrganizeProgress += Progress;
		if(OrganizeProgress >= 50.0f)
			OrganizeProgress = 50.0f;
		else if(OrganizeProgress <= 0.0f)
			OrganizeProgress = 0.0f;
	}
	else
	{
		RemoveProgress += Progress;
		if(RemoveProgress >= 50.0f)
			RemoveProgress = 50.0f;
		else if(RemoveProgress <= 0.0f)
			RemoveProgress = 0.0f;
	}

	SyncProgress();
}
