#pragma once

#include "CoreMinimal.h"
#include "Enums.h"
#include "IInteractable.h"
#include "GameFramework/Actor.h"
#include "Components/BoxComponent.h"
#include "Components/ArrowComponent.h"
#include "InteractObject.generated.h"

UCLASS(Blueprintable)
class COLORVERSE_API AInteractObject : public AActor, public IIInteractable
{
	GENERATED_BODY()

protected:
	virtual void BeginPlay() override;

	UPROPERTY(BlueprintReadOnly)
	USceneComponent* DefaultRoot = nullptr;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	UBoxComponent* BoxCol;
	
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	UStaticMeshComponent* StaticMesh;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	UArrowComponent* Arrow;

	UPROPERTY(VisibleAnywhere,
		BlueprintGetter=GetInteractable,
		BlueprintSetter=SetInteractable,
		meta=(AllowPrivateAccess), Category=Interactable)
	bool IsInteractable = false;

	UFUNCTION()
	void SetEnabledInteractable(bool IsEnabled) { IsInteractable = IsEnabled; };

public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category=Interactable)
	FString InteractWidgetDisplayTxt;

	AInteractObject();
	
	virtual void Tick(float DeltaTime) override;

	UFUNCTION()
	virtual void OnOverlapBegin(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);

	UFUNCTION()
	virtual void OnOverlapEnd(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex);

	virtual void OnEnter_Implementation() override;
	virtual void OnInteract_Implementation() override;
	virtual void OnExit_Implementation() override;

	UFUNCTION(BlueprintGetter)
	bool GetInteractable() const { return IsInteractable; }

	UFUNCTION(BlueprintSetter)
	void SetInteractable(bool Enabled) { IsInteractable = Enabled; }
};
