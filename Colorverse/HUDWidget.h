#pragma once

#include "CoreMinimal.h"
#include "PaintSlotWidget.h"
#include "IItem.h"
#include "ItemAcquiredWidget.h"
#include "Blueprint/UserWidget.h"
#include "Components/UniformGridPanel.h"
#include "HUDWidget.generated.h"

USTRUCT(BlueprintType)
struct FWidgetData
{
	GENERATED_BODY()

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly)
	TSubclassOf<UUserWidget> WidgetClass;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly)
	TArray<UUserWidget*> WidgetArray;
};

UCLASS(BlueprintType)
class COLORVERSE_API UHUDWidget : public UUserWidget
{
	GENERATED_BODY()

protected:
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Paint)
	ECombineColors PaintColor;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Paint, meta=(BindWidget))
	UProgressBar* PaintBar;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Paint)
	float PaintBarAmount = 0.0f;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Paint)
	float MaxPaintAmount = 100.0f;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=Paint)
	float PaintBarProgressVelocity = 2.0f;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Paint)
	int LastIndex = 0;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Item Acquired", meta=(BindWidget))
	UUniformGridPanel* ItemLogGridPanel;

public:
	UFUNCTION(BlueprintCallable)
	void InitializedHUD();

	UFUNCTION(BlueprintCallable)
	void SetPaintBarPercent(float Amount);

	UFUNCTION(BlueprintCallable)
	void AddItemLog(const FItem& ItemData);

	UFUNCTION(BlueprintCallable)
	void UpdateItemLog();
	
	UFUNCTION(BlueprintCallable)
	void ReleaseItemLogWidget(UUserWidget* Widget);

	UFUNCTION(BlueprintCallable, Category=Paint)
	void SetPaintColor(ECombineColors CombineColor);

	UFUNCTION(BlueprintCallable, Category = "Item Acquired Widget Pool")
	UUserWidget* GetOrCreateWidget(TSubclassOf<UUserWidget> WidgetClass);

	UFUNCTION(BlueprintCallable, Category = "Item Acquired Widget Pool")
	void ReleaseWidget(UUserWidget* Widget);

	UPROPERTY(VisibleDefaultsOnly, BlueprintReadOnly, Category = "Item Acquired", meta=(AllowPrivateAccess))
	TSubclassOf<UItemAcquiredWidget> ItemAcquiredWidgetClass;

	UPROPERTY(VisibleDefaultsOnly)
	TMap<TSubclassOf<UUserWidget>, FWidgetData> PoolMap;
};
