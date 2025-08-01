#pragma once

#include "CoreMinimal.h"
#include "Enums.h"
#include "InteractObject.h"
#include "GameFramework/Actor.h"
#include "LightObject.generated.h"

UCLASS(Blueprintable)
class COLORVERSE_API ALightObject : public AInteractObject
{
	GENERATED_BODY()

public:	
	ALightObject();

protected:
	virtual void BeginPlay() override;

public:
	virtual void OnEnter_Implementation() override;
	virtual void OnInteract_Implementation() override;
	virtual void OnExit_Implementation() override;
};
