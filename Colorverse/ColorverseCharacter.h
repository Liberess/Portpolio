#pragma once

#include "CoreMinimal.h"
#include "InteractObject.h"
#include "InteractWidget.h"
#include "InventoryManager.h"
#include "LivingEntity.h"
#include "CombatSystem.h"
#include "PaintedCollectObject.h"
#include "ColorverseCharacterAnimInstance.h"
#include "GameFramework/Character.h"
#include "ColorverseCharacter.generated.h"

UCLASS(config=Game)
class AColorverseCharacter : public ACharacter
{
	GENERATED_BODY()

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = Camera, meta = (AllowPrivateAccess = "true"))
	class USpringArmComponent* CameraBoom;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = Camera, meta = (AllowPrivateAccess = "true"))
	class UCameraComponent* FollowCamera;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Combat System", meta = (AllowPrivateAccess = "true"))
	class UCombatSystem* CombatSystem;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Living Entity", meta = (AllowPrivateAccess = "true"))
	class ULivingEntity* LivingEntity;
	
public:
	AColorverseCharacter();

	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat System")
	TArray<AActor*> AttackHitResults;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat System")
	FVector MoveInputY;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat System")
	FVector MoveInputX;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat System")
	float attackResourceValue;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Camera)
	float BaseTurnRate;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Camera)
	float BaseLookUpRate;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Player Movement")
	bool bIsRunning;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Player Movement")
	float WalkSpeed;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Player Movement")
	float RunSpeed;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Player Movement")
	float AutoRunStartDelay;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Player Movement")
	float RollSpeed;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Player Movement")
	float RollDelay;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	TArray<FLinearColor> BrushColors;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly)
	bool IsDrawing = false;
	
	UFUNCTION(BlueprintCallable)
	void SetEnabledToggleRun();

	virtual void Jump() override;
	virtual void StopJumping() override;

	UFUNCTION(BlueprintNativeEvent, BlueprintCallable)
	void Attack(int skillNum = 0);

	UFUNCTION(BlueprintNativeEvent, BlueprintCallable)
	void SetDisabledAttack();

	UFUNCTION(BlueprintNativeEvent, BlueprintCallable)
	void Roll();

	UFUNCTION(BlueprintNativeEvent, BlueprintCallable)
	void SetDisabledRoll();

	UFUNCTION(BlueprintNativeEvent, BlueprintCallable)
	void HitCheck();

	UFUNCTION(BlueprintNativeEvent, BlueprintCallable)
	void Attacked(FDamageMessage damageMessage);

	UFUNCTION(BlueprintNativeEvent, BlueprintCallable)
	void Dead();

	UFUNCTION()
	void OnOverlapBegin(class UPrimitiveComponent* OverlappedComp, class AActor* OtherActor, class UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);

	UFUNCTION()
	void OnOverlapEnd(class UPrimitiveComponent* OverlappedComp, class AActor* OtherActor, class UPrimitiveComponent* OtherComp, int32 OtherBodyIndex);

	UFUNCTION(BlueprintNativeEvent, BlueprintCallable)
	void Interact();

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, meta=(AllowPrivateAccess), Category=Interact)
	bool bIsInteract;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Interact)
	AInteractObject* InteractObject;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category=Interact)
	TSubclassOf<UUserWidget> InteractWidgetClass;
	
	UPROPERTY(BlueprintReadWrite, Category=Interact)
	UInteractWidget* InteractWidget;

	UPROPERTY(BlueprintReadWrite, Category=Interact)
	APaintedCollectObject* CurrentPaintedCollectObj;

	UFUNCTION(BlueprintCallable, BlueprintNativeEvent)
	void ControlInventory();

	UFUNCTION(BlueprintCallable, BlueprintNativeEvent)
	void ChangeEquipPaint(ECombineColors CombineColor);

	UFUNCTION(BlueprintCallable)
	void SetNextAttackCheck();

	UFUNCTION(BlueprintCallable)
	void SetEnableCanAttackTrace();

	UFUNCTION(BlueprintCallable)
	void SetDisableCanAttackTrace();
	
	UFUNCTION(BlueprintCallable)
	void CureHeath(int HealAmount);
	
	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
	void SetVisibleInteractableOutline(bool Visible);

private:	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, meta=(AllowPrivateAccess), Category="Combat System")
	bool bIsDamageable;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category="Combat System")
	bool bIsRolling;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta = (AllowPrivateAccess), Category="Combat System")
	bool bIsAttacking;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta = (AllowPrivateAccess), Category = "Combat System")
	bool bIsAttacked;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta = (AllowPrivateAccess), Category = "Combat System")
	bool bIsCanMove = true;
	
	UPROPERTY(BlueprintReadOnly, meta = (AllowPrivateAccess))
	bool bIsWatchingInteractWidget;

	UPROPERTY(BlueprintReadOnly, meta = (AllowPrivateAccess))
	UInventoryManager* InvenMgr;

	UPROPERTY(BlueprintReadWrite, meta=(AllowPrivateAccess))
	UColorverseCharacterAnimInstance* ColorverseAnim;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Interact)
	bool bIsInteractable = true;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Interact)
	float InteractCoolTime = 1.0f;
	
	UPROPERTY(BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Interact)
	FTimerHandle InteractCoolTimer;

protected:
	void OnResetVR();

	void MoveForward(float Value);
	void MoveRight(float Value);

	void TurnAtRate(float Rate);
	void LookUpAtRate(float Rate);

	void TouchStarted(ETouchIndex::Type FingerIndex, FVector Location);
	void TouchStopped(ETouchIndex::Type FingerIndex, FVector Location);

protected:
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;
	virtual void PostInitializeComponents() override;

public:
	FORCEINLINE bool GetIsAttacking() const { return bIsAttacking; }

	FORCEINLINE class USpringArmComponent* GetCameraBoom() const { return CameraBoom; }
	FORCEINLINE class UCameraComponent* GetFollowCamera() const { return FollowCamera; }
	FORCEINLINE class UCombatSystem* GetCombatSystem() const { return CombatSystem; }
	FORCEINLINE class ULivingEntity* GetLivingEntity() const { return LivingEntity; }
	FORCEINLINE class UColorverseCharacterAnimInstance* GetColorverseAnim() const { return ColorverseAnim; }
};
