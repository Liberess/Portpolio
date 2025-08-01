#pragma once

#include "CoreMinimal.h"
#include "IItem.h"
#include "Blueprint/UserWidget.h"
#include "Components/Image.h"
#include "Components/TextBlock.h"
#include "ItemAcquiredWidget.generated.h"

UCLASS(BlueprintType)
class COLORVERSE_API UItemAcquiredWidget : public UUserWidget
{
	GENERATED_BODY()
	
public:
	UPROPERTY(BlueprintReadOnly, Category="Item Acquired")
	bool bIsViewComplete = false;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Item Acquired")
	int LogIndex = 0;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Item Acquired")
	FItem ItemData;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Item Acquired", meta=(BindWidget))
	UImage* ItemImg;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Item Acquired", meta=(BindWidget))
	UTextBlock* ItemNameTxt;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Item Acquired", meta=(BindWidget))
	UTextBlock* ItemAmountTxt;

	UPROPERTY(BlueprintReadOnly, Category="Item Acquired")
	FTimerHandle RemoveLogTimer;

	UPROPERTY(BlueprintReadOnly, Category="Item Acquired", meta=(BindWidgetAnim), Transient)
	UWidgetAnimation* ShowAnim;
	
	UPROPERTY(BlueprintReadOnly, Category="Item Acquired", meta=(BindWidgetAnim), Transient)
	UWidgetAnimation* HideAnim;
	
	UFUNCTION(BlueprintCallable, Category = "Item Acquired")
	void SetupItemWidget(const FItem& _ItemData);
	
	UFUNCTION(BlueprintCallable, Category = "Item Acquired")
	void SetupItemLogTimer();
	
	UFUNCTION(BlueprintCallable, Category = "Item Acquired")
	void UpdateItemInformation();

	UFUNCTION(BlueprintCallable, Category = "Item Acquired")
	void UpdateItemLogIndex();

	UFUNCTION(BlueprintCallable, Category = "Item Acquired")
	void ReleaseItemLogWidget();
};
