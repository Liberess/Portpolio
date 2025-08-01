#pragma once

#include "CoreMinimal.h"
#include "Enums.h"
#include "InteractObject.h"
#include "Sanctum.generated.h"

UCLASS(Blueprintable)
class COLORVERSE_API ASanctum : public AInteractObject
{
	GENERATED_BODY()

private:
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Sanctum)
	bool bIsRecoveryComplete = false;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, meta=(AllowPrivateAccess), Category=Sanctum)
	bool bIsChangedColor = false;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, meta=(AllowPrivateAccess), Category=Sanctum)
	bool bIsUnlock = false;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Sanctum)
	int StanctumID = 0;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (AllowPrivateAccess), Category=Sanctum)
	EPuzzleTag PuzzleTag = EPuzzleTag::Puzzle_Red;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Sanctum)
	ECombineColors StatueColorTag;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Sanctum)
	FLinearColor CurrentColor;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Sanctum)
	FLinearColor TargetColor;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Sanctum)
	FLinearColor CompleteColor;

	UPROPERTY(BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Sanctum)
	UMaterialInstanceDynamic* PaintingMatInst;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Sanctum)
	float ChangedColorVelocity = 0.5f;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Sanctum)
	int RecoveryCount = 0;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=Sanctum)
	int MaxRecoveryCount = 1;

protected:
	virtual void BeginPlay() override;

public:
	ASanctum();
	virtual void Tick(float DeltaSeconds) override;
	
	virtual void OnEnter_Implementation() override;
	virtual void OnInteract_Implementation() override;
	virtual void OnExit_Implementation() override;

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent, Category=Sanctum)
	void ActiveUnlockEffect();

	UFUNCTION(BlueprintCallable, Category=Sanctum)
	void IncreaseColor();

	UFUNCTION(BlueprintCallable, Category=Sanctum)
	void DecreaseColor();
};
