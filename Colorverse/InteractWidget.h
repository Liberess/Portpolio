#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "Components/TextBlock.h"
#include "InteractWidget.generated.h"

UCLASS(BlueprintType)
class COLORVERSE_API UInteractWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	UPROPERTY(meta=(BindWidget))
	UTextBlock* InteractTxt;

	UFUNCTION(BlueprintCallable)
	void SetInteractText(FText Txt);
};
