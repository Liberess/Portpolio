#pragma once

#include "CoreMinimal.h"
#include "InteractObject.h"
#include "Statue.generated.h"

UCLASS(Blueprintable)
class COLORVERSE_API AStatue : public AInteractObject
{
	GENERATED_BODY()
	
private:
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, meta=(AllowPrivateAccess), Category=Statue)
	bool bIsUnlock = false;

protected:
	virtual void BeginPlay() override;

public:
	virtual void OnEnter_Implementation() override;
	virtual void OnInteract_Implementation() override;
	virtual void OnExit_Implementation() override;
};
