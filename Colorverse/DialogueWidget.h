#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "Components/TextBlock.h"
#include "DialogueWidget.generated.h"

UCLASS(BlueprintType)
class COLORVERSE_API UDialogueWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(BindWidget))
	UTextBlock* NPCNameTxt;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(BindWidget, MultiLine=true))
	UTextBlock* DialogueTxt;

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
	void SetDialogueText(const FText& Name, const FText& Dialogue);
};
