#include "BuffItem.h"

#include "AfterSchoolCleaningGameModeBase.h"
#include "Kismet/GameplayStatics.h"
#include "Particles/ParticleSystemComponent.h"

ABuffItem::ABuffItem()
{
	PrimaryActorTick.bCanEverTick = true;

	SphereCollision = CreateDefaultSubobject<USphereComponent>(TEXT("Sphere Collision"));
	SphereCollision->InitSphereRadius(10.0f);
	SphereCollision->SetCollisionProfileName(TEXT("Trigger"));
	SphereCollision->OnComponentBeginOverlap.AddDynamic(this, &ABuffItem::OnOverlapBegin);

	RootComponent = SphereCollision;

	ItemMesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("ItemMesh"));
	static ConstructorHelpers::FObjectFinder<UStaticMesh> _ItemMesh(
		TEXT("StaticMesh'/Engine/BasicShapes/Sphere.Sphere'"));

	if (_ItemMesh.Succeeded())
	{
		ItemMesh->SetStaticMesh(_ItemMesh.Object);
		ItemMesh->SetCollisionProfileName("Trigger");
		ItemMesh->SetupAttachment(RootComponent);
	}

	ParticleSystem = CreateDefaultSubobject<UParticleSystemComponent>(TEXT("ItemParticle"));
	ParticleSystem->SetupAttachment(RootComponent);
	ParticleSystem->bAutoActivate = true;
	ParticleSystem->SetRelativeLocation(FVector(0.0f, 0.0f, 0.0f));
	static ConstructorHelpers::FObjectFinder<UParticleSystem> ParticleAsset(TEXT("/Game/Effects/Effects/Particles/P_Stun_Stars_Base.P_Stun_Stars_Base"));
	if(ParticleAsset.Succeeded())
		ParticleSystem->SetTemplate(ParticleAsset.Object);
}

void ABuffItem::BeginPlay()
{
	Super::BeginPlay();

	if (BuffDuration <= 0.0f)
		BuffDuration = 20.0f;

	RepeatTime = 0.0f;
}

void ABuffItem::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	RepeatMovement(DeltaTime);
}

void ABuffItem::OnOverlapBegin(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp,
	int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	if (OtherActor && (OtherActor != this) && OtherComp)
	{
		if (OtherActor->ActorHasTag("Player"))
			UseItem();
	}
}

void ABuffItem::UseItem()
{
	AAfterSchoolCleaningGameModeBase* MyGameMode = Cast<AAfterSchoolCleaningGameModeBase>(UGameplayStatics::GetGameMode(GetWorld()));
	check(MyGameMode)

	switch (BuffItemType)
	{
	case EBuffItemType::See:
		MyGameMode->SetOutlinePostProcess(true, BuffDuration);
		break;
		
	case EBuffItemType::Wall: 
		MyGameMode->SetObstacleDebuff(EObstacleType::WallObs, true, BuffDuration);
		break;
		
	case EBuffItemType::Floor:
		MyGameMode->SetObstacleDebuff(EObstacleType::FloorObs, true, BuffDuration);
		break;
	}

	Destroy();
}

void ABuffItem::RepeatMovement(float DeltaTime)
{
	FVector NewLocation = GetActorLocation();
	float DeltaHeight = (FMath::Sin(RepeatTime + DeltaTime) - FMath::Sin(RepeatTime));

	NewLocation.Z += DeltaHeight * RepeatHeight;
	RepeatTime += DeltaTime * RepeatVelocity;
	SetActorLocation(NewLocation);
}

