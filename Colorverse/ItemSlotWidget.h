#pragma once

#include "CoreMinimal.h"
#include "IItem.h"
#include "Blueprint/UserWidget.h"
#include "Components/Border.h"
#include "Components/Image.h"
#include "Components/TextBlock.h"
#include "ItemSlotWidget.generated.h"

UCLASS(BlueprintType)
class COLORVERSE_API UItemSlotWidget : public UUserWidget
{
	GENERATED_BODY()
	
public:
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(BindWidget))
	UBorder* ThumbnailBorder;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(BindWidget))
	UImage* ThumbnailImg;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(BindWidget))
	UTextBlock* AmountTxt;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite)
	int Index;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite)
	bool bIsMaker;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite)
	EItemSlotLocationType ItemLocation;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite)
	FItem ItemData;
	
	UFUNCTION(BlueprintCallable, BlueprintNativeEvent)
	void OnClick();

	UFUNCTION(BlueprintCallable)
	void UpdateItemSlot(const FItem& Item);
};
