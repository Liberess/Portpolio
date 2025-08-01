#include "Sanctum.h"
#include "ColorverseCharacter.h"
#include "CombatSystem.h"
#include "Kismet/GameplayStatics.h"

ASanctum::ASanctum()
{
	//PrimaryComponentTick.bCanEverTick = true;
}

void ASanctum::Tick(float DeltaSeconds)
{
	Super::Tick(DeltaSeconds);

	if (bIsChangedColor && !bIsUnlock)
	{
		CurrentColor = FMath::Lerp(CurrentColor, TargetColor, DeltaSeconds * ChangedColorVelocity);
		
		if (FMath::Abs(CurrentColor.R - TargetColor.R) <= 0.01f)
		{
			bIsChangedColor = false;
			CurrentColor = TargetColor;
		}

		PaintingMatInst->SetVectorParameterValue("EmissiveColor", CurrentColor);
	}
}

void ASanctum::BeginPlay()
{
	StanctumID = static_cast<int>(StatueColorTag);
	
	Super::BeginPlay();

	AColorverseCharacter* Character = Cast<AColorverseCharacter>(UGameplayStatics::GetPlayerCharacter(GetWorld(), 0));
	CompleteColor = Character->GetCombatSystem()->GetCurrentPaintLinearColorByEnum(StatueColorTag);
}

void ASanctum::OnEnter_Implementation()
{
	
}

void ASanctum::OnInteract_Implementation()
{
	if(!bIsRecoveryComplete || bIsUnlock)
		return;

	bIsUnlock = true;
	IsInteractable = false;
	ActiveUnlockEffect();
}

void ASanctum::OnExit_Implementation()
{
	//Super::OnExit();
}

void ASanctum::IncreaseColor()
{
	if(bIsRecoveryComplete || bIsUnlock)
		return;
	
	bIsChangedColor = true;
	
	++RecoveryCount;
	
	PaintingMatInst->GetVectorParameterValue(FName(TEXT("EmissiveColor")), CurrentColor);

	TargetColor = FLinearColor(
			FMath::Lerp(CurrentColor.R, CompleteColor.R, 0.2f),
			FMath::Lerp(CurrentColor.G, CompleteColor.G, 0.2f),
			FMath::Lerp(CurrentColor.B, CompleteColor.B, 0.2f),
			1.0f);
	
	if (RecoveryCount >= MaxRecoveryCount)
	{
		bIsRecoveryComplete = true;
		//TargetColor = CompleteColor;
		IsInteractable = true;
		InteractWidgetDisplayTxt = FName(TEXT("성소 해금")).ToString();
	}
}

void ASanctum::DecreaseColor()
{
	if(bIsRecoveryComplete || bIsUnlock)
		return;

	if(RecoveryCount < 1)
		return;
	
	bIsChangedColor = true;

	--RecoveryCount;
	
	PaintingMatInst->GetVectorParameterValue(FName(TEXT("EmissiveColor")), CurrentColor);
	
	if (RecoveryCount <= 0)
	{
		TargetColor = FLinearColor(0.0f, 0.0f, 0.0f, 1.0f);
	}
	else
	{
		TargetColor = FLinearColor(
			FMath::Lerp(CurrentColor.R, 0.0f, 0.5f),
			FMath::Lerp(CurrentColor.G, 0.0f, 0.5f),
			FMath::Lerp(CurrentColor.B, 0.0f, 0.5f),
			1.0f);
	}
}
