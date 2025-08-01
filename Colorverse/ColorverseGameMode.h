#pragma once

#include "CoreMinimal.h"
#include "DialogueWidget.h"
#include "HUDWidget.h"
#include "InteractWidget.h"
#include "InventoryWidget.h"
#include "GameFramework/GameModeBase.h"
#include "ColorverseGameMode.generated.h"

UCLASS()
class AColorverseGameMode : public AGameModeBase
{
	GENERATED_BODY()

public:
	AColorverseGameMode();

	virtual void BeginPlay() override;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=InventoryManager)
	TObjectPtr<UInventoryWidget> InventoryWidget;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=InventoryManager)
	TObjectPtr<UHUDWidget> HUDWidget;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=NPC)
	TObjectPtr<UDialogueWidget> DialogueWidget;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=NPC)
	TObjectPtr<UInteractWidget> InteractWidget;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Item Acquired", meta=(AllowPrivateAccess))
	TSubclassOf<UItemAcquiredWidget> ItemAcquiredWidgetClass;
};



