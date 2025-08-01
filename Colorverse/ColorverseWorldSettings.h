#pragma once

#include "CoreMinimal.h"
#include "GameFramework/WorldSettings.h"
#include "ColorverseWorldSettings.generated.h"

UCLASS()
class COLORVERSE_API AColorverseWorldSettings : public AWorldSettings
{
	GENERATED_BODY()

public:
	UPROPERTY(EditAnywhere, Category="ColorverseWorldSettings | Color System")
	bool bUseColorManager = true;
	
	UPROPERTY(EditAnywhere, Category="ColorverseWorldSettings | Inventory System")
	bool bUseInventoryManager = true;
};
