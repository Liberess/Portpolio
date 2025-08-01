#include "HUDWidget.h"

#include "ColorverseGameMode.h"
#include "Components/UniformGridSlot.h"
#include "Kismet/GameplayStatics.h"

void UHUDWidget::InitializedHUD()
{
	//PaintWidget->SetPaintColor(static_cast<ECombineColors>(i));
	//PaintWidget->SetInputKeyText(FString::FromInt(i+1));
	PaintBar->SetPercent(0.0f);
	SetPaintBarPercent(0.0f);

	if(auto GameMode = Cast<AColorverseGameMode>(UGameplayStatics::GetGameMode(this)))
		ItemAcquiredWidgetClass = GameMode->ItemAcquiredWidgetClass;
}

void UHUDWidget::SetPaintBarPercent(float Amount)
{
	PaintBarAmount = Amount;
}

void UHUDWidget::AddItemLog(const FItem& ItemData)
{
	if(!ItemData.bIsValid)
		return;

	for(auto& Child : ItemLogGridPanel->GetAllChildren())
	{
		if(UItemAcquiredWidget* ItemWidget = Cast<UItemAcquiredWidget>(Child))
		{
			if(ItemData.Name.EqualTo(ItemWidget->ItemData.Name))
			{
				if(ItemWidget->LogIndex != LastIndex)
				{
					for(auto& Child2 : ItemLogGridPanel->GetAllChildren())
					{
						if(Child != Child2)
						{
							if(UItemAcquiredWidget* otherWidget = Cast<UItemAcquiredWidget>(Child2))
							{
								if(otherWidget->ItemData.bIsValid)
								{
									if(otherWidget->LogIndex > 1 && ItemWidget->LogIndex > 0)
									{
										otherWidget->LogIndex -= 1;
										if(UUniformGridSlot* ChildSlot = Cast<UUniformGridSlot>(Child2->Slot))
											ChildSlot->SetRow(otherWidget->LogIndex);
									}
								}
								else
								{
									otherWidget->ReleaseItemLogWidget();
								}
							}
						}
					}
				}

				ItemWidget->LogIndex = 0;
				ItemWidget->ItemData.Amount += ItemData.Amount;
				if(UUniformGridSlot* ChildSlot = Cast<UUniformGridSlot>(Child->Slot))
					ChildSlot->SetRow(ItemWidget->LogIndex);
				ItemWidget->UpdateItemInformation();
				ItemWidget->SetupItemLogTimer();
				UpdateItemLog();
				return;
			}
		}
	}

	if(ItemAcquiredWidgetClass != nullptr)
	{
		if(auto GameMode = Cast<AColorverseGameMode>(UGameplayStatics::GetGameMode(this)))
		{
			if(GameMode->ItemAcquiredWidgetClass)
				ItemAcquiredWidgetClass = ItemAcquiredWidgetClass;
		}
	}
	//UUserWidget* NewWidget = CreateWidget<UUserWidget>(GetWorld(), ItemAcquiredWidgetClass);
	if(UItemAcquiredWidget* ItemLogWidget = Cast<UItemAcquiredWidget>(GetOrCreateWidget(ItemAcquiredWidgetClass)))
	{
		ItemLogWidget->SetVisibility(ESlateVisibility::Visible);
		ItemLogWidget->LogIndex = 0;
		ItemLogWidget->bIsViewComplete = false;
		ItemLogWidget->SetupItemWidget(ItemData);
		ItemLogGridPanel->AddChildToUniformGrid(ItemLogWidget, 0, 0);
		UpdateItemLog();
	}
}

void UHUDWidget::UpdateItemLog()
{
	LastIndex = 0;
	for(auto& Child : ItemLogGridPanel->GetAllChildren())
	{
		if(UItemAcquiredWidget* ItemWidget = Cast<UItemAcquiredWidget>(Child))
		{
			ItemWidget->UpdateItemLogIndex();
			if(UUniformGridSlot* ChildSlot = Cast<UUniformGridSlot>(Child->Slot))
				ChildSlot->SetRow(ItemWidget->LogIndex);

			if(LastIndex < ItemWidget->LogIndex)
				LastIndex = ItemWidget->LogIndex;
		}
	}
}

void UHUDWidget::ReleaseItemLogWidget(UUserWidget* Widget)
{
	if (Widget != nullptr)
		return;

	ReleaseWidget(Widget);
}

void UHUDWidget::SetPaintColor(ECombineColors CombineColor)
{
	PaintColor = CombineColor;
}

UUserWidget* UHUDWidget::GetOrCreateWidget(TSubclassOf<UUserWidget> WidgetClass)
{
	if (!WidgetClass)
		return nullptr;

	FWidgetData* FoundWidgetData = PoolMap.Find(WidgetClass);

	if (!FoundWidgetData)
	{
		FWidgetData NewWidgetData;
		NewWidgetData.WidgetClass = WidgetClass;
		PoolMap.Add(WidgetClass, NewWidgetData);
		FoundWidgetData = PoolMap.Find(WidgetClass);
	}

	for (UUserWidget* Widget : FoundWidgetData->WidgetArray)
	{
		if (Widget && !Widget->IsVisible())
			return Widget;
	}

	UWorld* World = GEngine->GetWorldFromContextObject(GetWorld(), EGetWorldErrorMode::LogAndReturnNull);
	if (!World)
		return nullptr;

	UUserWidget* NewWidget = CreateWidget<UUserWidget>(World, WidgetClass);
	FoundWidgetData->WidgetArray.Add(NewWidget);
	return NewWidget;
}

void UHUDWidget::ReleaseWidget(UUserWidget* Widget)
{
	const TSubclassOf<UUserWidget> WidgetClass = Widget->GetClass();
	auto FoundWidgetData = PoolMap.Find(WidgetClass);

	if (!FoundWidgetData)
		return;
	
	FoundWidgetData->WidgetClass = nullptr;
	FoundWidgetData->WidgetArray.Empty();

	Widget->SetVisibility(ESlateVisibility::Collapsed);
}
