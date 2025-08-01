#pragma once

#include "CoreMinimal.h"
#include "Enums.h"
#include "Blueprint/UserWidget.h"
#include "Components/ProgressBar.h"
#include "Components/TextBlock.h"
#include "PaintSlotWidget.generated.h"

UCLASS(BlueprintType)
class COLORVERSE_API UPaintSlotWidget : public UUserWidget
{
	GENERATED_BODY()

protected:
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Paint)
	ECombineColors PaintColor;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Paint, meta=(BindWidget))
	UProgressBar* PaintBar;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Paint, meta=(BindWidget))
	UTextBlock* InputKeyTxt;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Paint)
	float PaintBarAmount = 0.0f;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Paint)
	float MaxPaintAmount = 100.0f;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=Paint)
	float PaintBarProgressVelocity = 2.0f;

public:
	UFUNCTION(BlueprintCallable, Category=Paint)
	void SetPaintColor(ECombineColors CombineColor);
	
	UFUNCTION(BlueprintCallable, Category=Paint)
	void SetInputKeyText(const FString& Str);

	UFUNCTION(BlueprintCallable, Category=Paint)
	void SetPaintBarPercent(float Amount);
};
