#include "DataManager.h"

void UDataManager::Initialize(FSubsystemCollectionBase& Collection)
{
	Super::Initialize(Collection);

	StageClears.Empty();
	for (int i = 0; i < 3; i++)
	{
		StageClears.Add(false);
	}
}