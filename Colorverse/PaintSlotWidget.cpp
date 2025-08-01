#include "PaintSlotWidget.h"

void UPaintSlotWidget::SetPaintColor(ECombineColors CombineColor)
{
	PaintColor = CombineColor;
}

void UPaintSlotWidget::SetInputKeyText(const FString& Str)
{
	InputKeyTxt->SetText(FText::FromString(Str));
}

void UPaintSlotWidget::SetPaintBarPercent(float Amount)
{
	PaintBarAmount = Amount;
}
