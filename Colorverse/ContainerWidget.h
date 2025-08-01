#pragma once

#include "CoreMinimal.h"
#include "IItem.h"
#include "Blueprint/UserWidget.h"
#include "ContainerWidget.generated.h"

UCLASS()
class COLORVERSE_API UContainerWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	UContainerWidget(const FObjectInitializer& ObjectInitializer);

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category=Container)
	UTexture2D* EmptyImg;

	UFUNCTION(BlueprintCallable, Category=Container)
	virtual void CreateContainer(int Slots) PURE_VIRTUAL(UContainerWidget::CreateContainer,);
	
	UFUNCTION(BlueprintCallable, Category=Container)
	virtual void UpdateContainer(TArray<FItem> Items) PURE_VIRTUAL(UContainerWidget::UpdateContainer,);

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=Container)
	int SelectItemIndex = 0;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=Container)
	int DropItemIndex = 0;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=Container)
	FItem SelectItem;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=Container)
	FItem DropItem;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=Container)
	EItemSlotLocationType SelectLocation;
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=Container)
	EItemSlotLocationType DropLocation;
	
	UFUNCTION(BlueprintCallable, Category=Container)
	virtual void MoveItem(TArray<FItem>& SelectAry, TArray<FItem>& DropAry, bool IsMoveBetween);

	UFUNCTION(BlueprintCallable, Category=Container)
	virtual void SetItemSlotArrays();
};
