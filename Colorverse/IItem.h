#pragma once

#include "CoreMinimal.h"
#include "Enums.h"
#include "Engine/DataTable.h"
#include "UObject/Interface.h"
#include "IItem.generated.h"

USTRUCT(BlueprintType, BlueprintType)
struct FItem : public FTableRowBase
{
	GENERATED_USTRUCT_BODY();
	
	FItem() : CombineType(EItemCombineType::Source), Amount(1), RecoveryAmount(0.0f), IconImg() {};
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Item")
	EItemCombineType CombineType;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Item")
	FText Name;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, meta = (MultiLine = true), Category = "Item")
	FText Description;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Item")
	int Amount;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Item")
	float RecoveryAmount;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Item")
	UTexture2D* IconImg;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Item")
	bool bIsValid = false;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Item")
	bool bIsConsume = false;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Item")
	EConsumeType ConsumeType;
};

USTRUCT(BlueprintType, BlueprintType)
struct FCombine : public FTableRowBase
{
	GENERATED_USTRUCT_BODY();
	
public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combine")
	FText SrcName = FText::FromString("");
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combine")
	FText DestName = FText::FromString("");;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combine")
	FText ResultItemName = FText::FromString("");;
};

USTRUCT(BlueprintType, BlueprintType)
struct FPaintCombo : public FTableRowBase
{
	GENERATED_USTRUCT_BODY();

	FPaintCombo() : ResultColor(FColor(1, 1, 1, 1))
	{};

public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "PaintCombo")
	TArray<ECombineColors> ComboColors;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "PaintCombo")
	FLinearColor ResultColor = FColor(1, 1, 1, 1);
};

UINTERFACE(MinimalAPI)
class UIItem : public UInterface
{
	GENERATED_BODY()
};

class COLORVERSE_API IIItem
{
	GENERATED_BODY()
	
public:
	UFUNCTION(BlueprintNativeEvent, BlueprintCallable, Category = "Item")
	void OnUse();
};
