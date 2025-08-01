#include "AfterSchoolCleaningGameModeBase.h"
#include "Blueprint/UserWidget.h"
#include "Kismet/GameplayStatics.h"

AAfterSchoolCleaningGameModeBase::AAfterSchoolCleaningGameModeBase()
{
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnBPClass(
		TEXT("/Game/Blueprints/BP_Sweeper"));

	if (PlayerPawnBPClass.Class != nullptr)
		DefaultPawnClass = PlayerPawnBPClass.Class;
}

void AAfterSchoolCleaningGameModeBase::BeginPlay()
{
	Super::BeginPlay();

	AActor* Actor = UGameplayStatics::GetActorOfClass(this, APostProcessVolume::StaticClass());
	if (IsValid(Actor))
		PostVolume = Cast<APostProcessVolume>(Actor);

	AActor* TempActor = UGameplayStatics::GetActorOfClass(this, ASweeper::StaticClass());
	if (IsValid(TempActor))
		Sweeper = Cast<ASweeper>(TempActor);

	ChangeMenuWidget(StartingWidget);
}

void AAfterSchoolCleaningGameModeBase::SetOutlinePostProcess(bool Active, float Duration)
{
	if (!IsValid(PostVolume))
		return;

	GetWorldTimerManager().ClearTimer(SeeOutlineTimer);

	if (Active)
	{
		PostVolume->bEnabled = true;
		GetWorld()->GetTimerManager().SetTimer(SeeOutlineTimer, FTimerDelegate::CreateLambda([&]()
		{
			PostVolume->bEnabled = false;
		}), Duration, false);
	}
	else
	{
		PostVolume->bEnabled = false;
	}
}

void AAfterSchoolCleaningGameModeBase::SetObstacleDebuff(EObstacleType ObsType, bool Active, float Duration)
{
	if(!IsValid(Sweeper))
		return;
	
	if (ObsType == EObstacleType::WallObs)
	{
		GetWorldTimerManager().ClearTimer(WallDebuffTimer);

		if (Active)
		{
			SetBuffUI(ObsType, Duration);
			Sweeper->SetToolRemoveDamage(ETool::Wall, 3);
			GetWorld()->GetTimerManager().SetTimer(WallDebuffTimer, FTimerDelegate::CreateLambda([&]()
			{
				Sweeper->SetToolRemoveDamage(ETool::Wall, 1);
			}), Duration, false);
		}
		else
		{
			Sweeper->SetToolRemoveDamage(ETool::Wall, 1);
		}
	}
	else
	{
		GetWorldTimerManager().ClearTimer(FloorDebuffTimer);

		if (Active)
		{
			SetBuffUI(ObsType, Duration);
			Sweeper->SetToolRemoveDamage(ETool::Floor, 3);
			GetWorld()->GetTimerManager().SetTimer(FloorDebuffTimer, FTimerDelegate::CreateLambda([&]()
			{
				Sweeper->SetToolRemoveDamage(ETool::Floor, 1);
			}), Duration, false);
		}
		else
		{
			Sweeper->SetToolRemoveDamage(ETool::Floor, 1);
		}
	}
}

void AAfterSchoolCleaningGameModeBase::ChangeMenuWidget(TSubclassOf<UUserWidget> NewWidget)
{
	if (CurrentWidget != nullptr)
	{
		CurrentWidget->RemoveFromViewport();
		CurrentWidget = nullptr;
	}

	if (NewWidget != nullptr)
	{
		CurrentWidget = CreateWidget(GetWorld(), NewWidget);
		if (CurrentWidget != nullptr)
		{
			CurrentWidget->AddToViewport();
		}
	}
}
