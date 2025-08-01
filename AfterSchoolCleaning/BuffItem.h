#pragma once

#include "CoreMinimal.h"
#include "Components/SphereComponent.h"
#include "GameFramework/Actor.h"
#include "BuffItem.generated.h"

UENUM(BlueprintType)
enum class EBuffItemType : uint8
{
	See UMETA(DisplayName = "See Through"),
	Wall UMETA(DisplayName = "Wall Obstacle Debuff"),
	Floor UMETA(DisplayName = "Floor Obstacle Debuff")
};

UCLASS()
class AFTERSCHOOLCLEANING_API ABuffItem : public AActor
{
	GENERATED_BODY()
	
public:	
	ABuffItem();

protected:
	virtual void BeginPlay() override;

public:	
	virtual void Tick(float DeltaTime) override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	UStaticMeshComponent* ItemMesh;

	UPROPERTY(VisibleAnywhere)
	USphereComponent* SphereCollision;
	
	void RepeatMovement(float DeltaTime);

	float RepeatTime;

	UPROPERTY(EditAnywhere, BlueprintReadWrite,
		meta=(ClampMin=0, ClampMax=100, UIMin=0, UIMax=100))
	float RepeatHeight;

	UPROPERTY(EditAnywhere, BlueprintReadWrite,
		meta=(ClampMin=0, ClampMax=100, UIMin=0, UIMax=100))
	float RepeatVelocity;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	EBuffItemType BuffItemType;

	UFUNCTION()
	void OnOverlapBegin(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);
	
	UFUNCTION(BlueprintCallable)
	virtual void UseItem();

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	float BuffDuration;

	UPROPERTY(VisibleAnywhere)
	UParticleSystemComponent* ParticleSystem;
};
