#include "PaintableObject.h"
#include "ColorverseCharacter.h"

APaintableObject::APaintableObject()
{
	PrimaryActorTick.bCanEverTick = true;

	DefaultRoot = CreateDefaultSubobject<USceneComponent>(TEXT("DefaultRoot"));
	SetRootComponent(DefaultRoot);

	StaticMesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Static Mesh"));
	StaticMesh->SetupAttachment(DefaultRoot);

	Arrow = CreateDefaultSubobject<UArrowComponent>(TEXT("Arrow"));
	Arrow->ArrowLength = 150.0f;
	Arrow->SetupAttachment(DefaultRoot);
}

void APaintableObject::BeginPlay()
{
	Super::BeginPlay();

	PaintingMatInst = UMaterialInstanceDynamic::Create(PaintingMatTemplate, this);
	StaticMesh->SetMaterial(0, PaintingMatInst);
}

void APaintableObject::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	if(!bIsInteractable)
		return;

	if(!bIsPaintedComplete && bIsColorChanged)
	{
		CurrentColor = FMath::Lerp(CurrentColor, TargetColor, DeltaTime * 2.0f);
		PaintingMatInst->SetVectorParameterValue("OverlayColor", CurrentColor);

		if(FLinearColor::Dist(CurrentColor, TargetColor) <= 0.01f)
		{
			bIsColorChanged = false;

			if(PaintedCount >= PaintedCapacity)
			{
				CurrentColor = TargetColor;
				PaintingMatInst->SetVectorParameterValue("OverlayColor", CurrentColor);

				if(bIsRightColor)
				{
					bIsPaintedComplete = true;
					CompletePainted();
				}
			}
		}
	}
}

void APaintableObject::PaintToObject_Implementation(FLinearColor _PaintColor, ECombineColors _CurrentColorTag)
{
	if(!bIsInteractable)
		return;

	CurrentColorTag = _CurrentColorTag;
	
	bIsRightColor = (_CurrentColorTag == TargetColorTag) ? true : false;
	if(TargetColorTag == ECombineColors::Empty)
		bIsRightColor = true;
	bIsColorChanged = true;
	
	if(_CurrentColorTag != ECombineColors::Empty)
	{
		if(!bIsPaintedComplete)
		{
			++PaintedCount;
			PaintingMatInst->GetVectorParameterValue(FName(TEXT("OverlayColor")), CurrentColor);

			if(PaintedCount >= PaintedCapacity)
			{
				TargetColor = _PaintColor;
			}
			else
			{
				TargetColor = FLinearColor(
				FMath::Lerp(CurrentColor.R, _PaintColor.R, 0.5f),
				FMath::Lerp(CurrentColor.G, _PaintColor.G, 0.5f),
				FMath::Lerp(CurrentColor.B, _PaintColor.B, 0.5f),
				1.0f);
			}
		}
	}
	else
	{
		TargetColor = _PaintColor;
		PaintedCount = 0;
		bIsPaintedComplete = false;
	}
}

