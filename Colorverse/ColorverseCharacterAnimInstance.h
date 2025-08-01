// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Animation/AnimInstance.h"
#include "ColorverseCharacterAnimInstance.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnNextAttackCheckDelegate);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnEndAttackJudgDelegate);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnStartAttackJudgDelegate);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnEndAttackDelegate);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnEndDamagedDelegate);

UCLASS()
class COLORVERSE_API UColorverseCharacterAnimInstance : public UAnimInstance
{
	GENERATED_BODY()

public:
	UColorverseCharacterAnimInstance();

	virtual void NativeUpdateAnimation(float DeltaSeconds) override;

protected:
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Attack Montage", meta = (AllowPrivateAccess = "true"))
	TArray <UAnimMontage*> AttackMontage;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Attack Montage", meta = (AllowPrivateAccess = "true"))
	UAnimMontage* JumpAttackMontage;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Damaged Montage", meta = (AllowPrivateAccess = "true"))
	UAnimMontage* DamagedMontage;

public:
	void PlayAttackMontage(int32 skillNum = 0);
	void PlayJumpAttackMontage();
	UFUNCTION(BlueprintNativeEvent, BlueprintCallable)
	void PlayDamagedMontage();
	void JumpToAttackMontageSection(int32 NewSection, int32 skillNum = 0);

public:
	UPROPERTY(BlueprintAssignable, BlueprintCallable)
	FOnNextAttackCheckDelegate OnNextAttackCheck;
	
	UPROPERTY(BlueprintAssignable, BlueprintCallable)
	FOnStartAttackJudgDelegate OnStartAttackJudg;
	
	UPROPERTY(BlueprintAssignable, BlueprintCallable)
	FOnEndAttackJudgDelegate OnEndAttackJudg;

	UPROPERTY(BlueprintAssignable, BlueprintCallable)
	FOnEndAttackDelegate OnEndAttack;

	UPROPERTY(BlueprintAssignable, BlueprintCallable)
	FOnEndDamagedDelegate OnEndDamaged;

	UFUNCTION()
	void AnimNotify_NextAttackCheck();

	UFUNCTION()
	void AnimNotify_StartAttackJudg();

	UFUNCTION()
	void AnimNotify_EndAttackJudg();

	UFUNCTION()
	void AnimNotify_EndAttack();

	UFUNCTION()
	void AnimNotify_EndDamaged();

	FName GetAttackMontageSectionName(int32 Section);
};
