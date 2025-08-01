#pragma once

#include "CoreMinimal.h"
#include "Engine/DataTable.h"
#include "UObject/Interface.h"
#include "IDialogue.generated.h"

USTRUCT(BlueprintType)
struct FDialogue : public FTableRowBase
{
	GENERATED_USTRUCT_BODY();
	
	FDialogue() { Dialogues.Reserve(3); };
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Dialogue)
	FText DisplayName;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Dialogue)
	TArray<FText> Dialogues;
};

UINTERFACE(MinimalAPI)
class UIDialogue : public UInterface
{
	GENERATED_BODY()
};

class COLORVERSE_API IIDialogue
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category=Dialogue)
	void OnBeginTalk();

	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category=Dialogue)
	void OnEndTalk();
	
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category=Dialogue)
	void OnQuestClear();
};
