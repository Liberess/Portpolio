#pragma once

#include "CoreMinimal.h"
#include "ContainerWidget.h"
#include "IItem.h"
#include "Blueprint/UserWidget.h"
#include "Components/Image.h"
#include "Components/UniformGridPanel.h"
#include "InventoryWidget.generated.h"

UCLASS(BlueprintType)
class COLORVERSE_API UInventoryWidget : public UContainerWidget
{
	GENERATED_BODY()

public:
	UInventoryWidget(const FObjectInitializer& ObjectInitializer);
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category=Inventory)
	int GridColumnAmount = 4;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Inventory, meta=(BindWidget))
	UUniformGridPanel* InventoryGridPanel;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Maker, meta=(BindWidget))
	UUniformGridPanel* MakerGridPanel;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Transient, meta = (BindWidgetAnim))
	UWidgetAnimation* InventoryShowAnim;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Transient, meta = (BindWidgetAnim))
	UWidgetAnimation* MakerShowAnim;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Transient, meta = (BindWidgetAnim))
	UWidgetAnimation* MakerHideAnim;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Maker, meta=(BindWidget))
	UImage* ResultImg;

	virtual void CreateContainer(int Slots) override;
	virtual void UpdateContainer(TArray<FItem> Items) override;

	UFUNCTION(BlueprintCallable)
	void UpdateMakerContainer(TArray<FItem> Items);
	
	UFUNCTION(BlueprintCallable)
	void SetCombineResultUI(const FItem& Item, bool IsAlreadyCombine);

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
	void PlayInventorySound(bool IsOpen);
};
