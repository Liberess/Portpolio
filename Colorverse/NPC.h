#pragma once

#include "CoreMinimal.h"
#include "DialogueWidget.h"
#include "IDialogue.h"
#include "InteractWidget.h"
#include "Camera/CameraComponent.h"
#include "Components/ArrowComponent.h"
#include "Components/CapsuleComponent.h"
#include "Components/SphereComponent.h"
#include "GameFramework/Pawn.h"
#include "NPC.generated.h"

UCLASS(BlueprintType)
class COLORVERSE_API ANPC : public APawn, public IIDialogue
{
	GENERATED_BODY()

public:
	ANPC();

	virtual void OnBeginTalk_Implementation() override;
	virtual void OnEndTalk_Implementation() override;
	virtual void OnQuestClear_Implementation() override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="NPC|Dialogue")
	FName NPCName;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category="NPC|Dialogue")
	FDialogue DialogueData;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category="NPC|Dialogue")
	int TalkIndex = -1;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="NPC|Dialogue")
	float SmoothBlendTime = 0.75f;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category="NPC|Dialogue")
	bool bIsTalking = false;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category="NPC|Dialogue")
	bool bIsInteractable = true;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category="NPC|Dialogue", meta=(AllowPrivateAccess))
	TObjectPtr<UDialogueWidget> DialogueWidget;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category="NPC|Dialogue", meta=(AllowPrivateAccess))
	TObjectPtr<UInteractWidget> InteractWidget;

protected:
	virtual void BeginPlay() override;

	UFUNCTION()
	virtual void OnOverlapBegin(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);
	
	UFUNCTION()
	virtual void OnOverlapEnd(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex);

	UFUNCTION(BlueprintCallable)
	void SetActiveInteractUI(bool IsActive);
	
	UFUNCTION(BlueprintCallable)
	void SetNPCCamera(bool IsNPCCam);

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category="NPC|Dialogue", meta=(AllowPrivateAccess))
	TObjectPtr<UDataTable> DialogueDT;

private:
	UPROPERTY(Category="NPC|Component", VisibleDefaultsOnly, BlueprintReadOnly, meta=(AllowPrivateAccess = "true"))
	TObjectPtr<USkeletalMeshComponent> Mesh;

	UPROPERTY(Category="NPC|Component", VisibleDefaultsOnly, BlueprintReadOnly, meta=(AllowPrivateAccess = "true"))
	TObjectPtr<UCapsuleComponent> CapsuleCol;

	UPROPERTY(Category="NPC|Component", VisibleDefaultsOnly, BlueprintReadOnly, meta=(AllowPrivateAccess = "true"))
	TObjectPtr<USphereComponent> TriggerZone;

	UPROPERTY(Category="NPC|Component", VisibleDefaultsOnly, BlueprintReadOnly, meta=(AllowPrivateAccess = "true"))
	TObjectPtr<UArrowComponent> Arrow;

	UPROPERTY(Category="NPC|Component", EditAnywhere, BlueprintReadOnly, meta=(AllowPrivateAccess = "true"))
	TObjectPtr<UCameraComponent> Camera;

	UPROPERTY(Category="NPC|Component", VisibleDefaultsOnly, BlueprintReadOnly, meta=(AllowPrivateAccess = "true"))
	TObjectPtr<AActor> PlayerCamera;

	UPROPERTY(Category="NPC|Component", VisibleDefaultsOnly, BlueprintReadOnly, meta=(AllowPrivateAccess = "true"))
	TObjectPtr<APlayerController> PlayerController;
};
