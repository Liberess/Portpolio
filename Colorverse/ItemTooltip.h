#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "Components/TextBlock.h"
#include "ItemTooltip.generated.h"

UCLASS(BlueprintType)
class COLORVERSE_API UItemTooltip : public UUserWidget
{
	GENERATED_BODY()

public:
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(BindWidget))
	UTextBlock* ItemNameTxt;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(BindWidget))
	UTextBlock* ItemDescriptionTxt;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(BindWidget))
	UTextBlock* ItemTypeTxt;
};
