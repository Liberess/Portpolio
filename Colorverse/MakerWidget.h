#pragma once

#include "CoreMinimal.h"
#include "ContainerWidget.h"
#include "IItem.h"
#include "Blueprint/UserWidget.h"
#include "Components/UniformGridPanel.h"
#include "MakerWidget.generated.h"

UCLASS(BlueprintType)
class COLORVERSE_API UMakerWidget : public UContainerWidget
{
	GENERATED_BODY()

public:
	UMakerWidget(const FObjectInitializer& ObjectInitializer);
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=Maker, meta=(BindWidget))
	UUniformGridPanel* MakerGridPanel;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Transient, meta = (BindWidgetAnim))
	UWidgetAnimation* MakerShowAnim;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Transient, meta = (BindWidgetAnim))
	UWidgetAnimation* MakerHideAnim;

	virtual void CreateContainer(int Slots) override;
	virtual void UpdateContainer(TArray<FItem> Items) override;
};
