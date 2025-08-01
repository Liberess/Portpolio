#pragma once

#include "CoreMinimal.h"
#include "UObject/Interface.h"
#include "IInteractable.generated.h"

UINTERFACE(MinimalAPI)
class UIInteractable : public UInterface
{
	GENERATED_BODY()
};

class COLORVERSE_API IIInteractable
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category=Interactable)
	void OnEnter();
	
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category=Interactable)
	void OnInteract();
	
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category=Interactable)
	void OnExit();
};
