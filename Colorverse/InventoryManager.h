#pragma once

#include "CoreMinimal.h"
#include "HUDWidget.h"
#include "IItem.h"
#include "InventoryWidget.h"
#include "Subsystems/WorldSubsystem.h"
#include "InventoryManager.generated.h"

UCLASS()
class COLORVERSE_API UInventoryManager : public UWorldSubsystem
{
	GENERATED_BODY()

private:
	UInventoryManager();
	
	UPROPERTY(BlueprintReadWrite, meta=(AllowPrivateAccess), Category=InventoryManager)
	UHUDWidget* HUDWidget;
	
	UPROPERTY(BlueprintReadWrite, meta=(AllowPrivateAccess), Category=InventoryManager)
	UInventoryWidget* InventoryWidget;

	UPROPERTY(BlueprintReadOnly, meta=(AllowPrivateAccess), Category=InventoryManager)
	bool bIsInventoryOpen = false;

	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	bool GetInventoryItemByName(const FText& Name, int& Index);

	UPROPERTY(BlueprintReadOnly, meta=(AllowPrivateAccess), Category=InventoryManager)
	UDataTable* CombineDataTable;

	UPROPERTY(BlueprintReadOnly, meta=(AllowPrivateAccess), Category=InventoryManager)
	UDataTable* ItemDataTable;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, meta=(AllowPrivateAccess), Category=InventoryManager)
	TMap<FName, bool> AlreadyCombineMap;

public:
	virtual bool ShouldCreateSubsystem(UObject* Outer) const override;
	virtual void Initialize(FSubsystemCollectionBase& Collection) override;

	UFUNCTION(BlueprintCallable)
	void InitializeManager();
	
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=InventoryManager)
	float PaintAmount = 0.0f;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category=InventoryManager)
	float MaxPaintAmount = 100.0f;

	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	void CurePaint(float Amount);

	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	void UpdatePaintUI();
	
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=InventoryManager)
	TArray<FItem> InventoryArray;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category=InventoryManager)
	TArray<FItem> MakerArray;
	
	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	void SetInventoryUI(bool IsActive, bool IsFlip = false);

	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	void UpdateInventory();

	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	void UpdateMaker();

	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	void AddInventoryItem(const FItem& Item, bool IsShowAcquiredUI = true);

	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	void UseInventoryItem(FItem Item);

	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	void UpdateMakerResultUI();
	
	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	void CombineItems();

	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	FORCEINLINE UHUDWidget* GetHUDWidget() { return HUDWidget; }

	UFUNCTION(BlueprintCallable, Category=InventoryManager)
	FORCEINLINE void SetVisibleHUDWidget(bool Visible) const
	{
		if(Visible)
			HUDWidget->AddToViewport();
		else
			HUDWidget->RemoveFromParent();
	}
};
