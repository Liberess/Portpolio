#include "NPC.h"
#include "ColorverseCharacter.h"
#include "ColorverseGameMode.h"
#include "Kismet/GameplayStatics.h"

ANPC::ANPC()
{
	PrimaryActorTick.bCanEverTick = true;

	CapsuleCol = CreateDefaultSubobject<UCapsuleComponent>(TEXT("Capsule Col"));
	if (CapsuleCol)
	{
		CapsuleCol->InitCapsuleSize(34.0f, 88.0f);
		CapsuleCol->SetCollisionProfileName(UCollisionProfile::Pawn_ProfileName);
		CapsuleCol->CanCharacterStepUpOn = ECB_No;
		CapsuleCol->SetShouldUpdatePhysicsVolume(true);
		CapsuleCol->SetCanEverAffectNavigation(false);
		CapsuleCol->bDynamicObstacle = true;
		SetRootComponent(CapsuleCol);
	}

	TriggerZone = CreateDefaultSubobject<USphereComponent>(TEXT("Trigger Zone"));
	if (TriggerZone)
	{
		TriggerZone->InitSphereRadius(120.0f);
		static FName TriggerZoneProfileName(TEXT("OverlapAllDynamic"));
		TriggerZone->SetCollisionProfileName(TriggerZoneProfileName);
		TriggerZone->SetCanEverAffectNavigation(false);
		TriggerZone->bDynamicObstacle = false;
		TriggerZone->SetupAttachment(CapsuleCol);
		TriggerZone->OnComponentBeginOverlap.AddDynamic(this, &ANPC::OnOverlapBegin);
		TriggerZone->OnComponentEndOverlap.AddDynamic(this, &ANPC::OnOverlapEnd);
	}

	Arrow = CreateEditorOnlyDefaultSubobject<UArrowComponent>(TEXT("Arrow"));
	if (Arrow)
	{
		Arrow->ArrowColor = FColor(150, 200, 255);
		Arrow->bTreatAsASprite = true;
		Arrow->SetupAttachment(CapsuleCol);
		Arrow->bIsScreenSizeScaled = true;
		Arrow->SetSimulatePhysics(false);
	}

	Mesh = CreateOptionalDefaultSubobject<USkeletalMeshComponent>(TEXT("NPC Mesh"));
	if (Mesh)
	{
		Mesh->AlwaysLoadOnClient = true;
		Mesh->AlwaysLoadOnServer = true;
		Mesh->VisibilityBasedAnimTickOption = EVisibilityBasedAnimTickOption::AlwaysTickPose;
		Mesh->bCastDynamicShadow = true;
		Mesh->bAffectDynamicIndirectLighting = true;
		Mesh->PrimaryComponentTick.TickGroup = TG_PrePhysics;
		Mesh->SetupAttachment(CapsuleCol);
		static FName MeshCollisionProfileName(TEXT("CharacterMesh"));
		Mesh->SetCollisionProfileName(MeshCollisionProfileName);
		Mesh->SetCanEverAffectNavigation(false);
	}

	Camera = CreateDefaultSubobject<UCameraComponent>(TEXT("Camera"));
	if (Camera)
		Camera->SetupAttachment(CapsuleCol);

	static ConstructorHelpers::FObjectFinder<UDataTable> DataTable(TEXT("/Game/DataTables/DT_Dialogue"));
	if (DataTable.Succeeded())
		DialogueDT = DataTable.Object;
}

void ANPC::BeginPlay()
{
	Super::BeginPlay();

	if (IsValid(DialogueDT))
		DialogueData = *(DialogueDT->FindRow<FDialogue>(NPCName, ""));

	PlayerController = UGameplayStatics::GetPlayerController(this, 0);
	PlayerCamera = PlayerController->GetViewTarget();
}

void ANPC::OnOverlapBegin(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp,
                          int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	if (OtherActor && OtherActor != this)
	{
		AColorverseCharacter* Character = Cast<AColorverseCharacter>(OtherActor);
		if (IsValid(Character))
			SetActiveInteractUI(true);
	}
}

void ANPC::OnOverlapEnd(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp,
                        int32 OtherBodyIndex)
{
	if (OtherActor && OtherActor != this)
	{
		AColorverseCharacter* Character = Cast<AColorverseCharacter>(OtherActor);
		if (IsValid(Character))
			OnEndTalk_Implementation();
	}
}

void ANPC::OnBeginTalk_Implementation()
{
	IIDialogue::OnBeginTalk_Implementation();

	if (!bIsInteractable)
		return;

	++TalkIndex;

	if (TalkIndex < DialogueData.Dialogues.Num())
	{
		DialogueWidget->AddToViewport();
		DialogueWidget->SetDialogueText(
			DialogueData.DisplayName,
			DialogueData.Dialogues[TalkIndex]);

		SetActiveInteractUI(false);

		bIsTalking = true;
		SetNPCCamera(true);
	}
	else
	{
		OnEndTalk_Implementation();
	}
}

void ANPC::OnEndTalk_Implementation()
{
	IIDialogue::OnEndTalk_Implementation();

	TalkIndex = -1;

	if (DialogueWidget != nullptr)
		DialogueWidget->RemoveFromParent();

	if (InteractWidget != nullptr)
		InteractWidget->RemoveFromParent();

	bIsTalking = false;
	SetNPCCamera(false);
	SetActiveInteractUI(false);
}

void ANPC::OnQuestClear_Implementation()
{
	IIDialogue::OnQuestClear_Implementation();
}

void ANPC::SetActiveInteractUI(bool IsActive)
{
	if (InteractWidget != nullptr)
	{
		if (IsActive)
		{
			InteractWidget->SetInteractText(FText::FromName(FName(TEXT("[F] 대화하기"))));
			InteractWidget->AddToViewport();
		}
		else
		{
			InteractWidget->RemoveFromParent();
		}
	}
}

void ANPC::SetNPCCamera(bool IsNPCCam)
{
	if (IsValid(PlayerController) && IsValid(PlayerCamera))
	{
		if (IsNPCCam)
		{
			if (PlayerController->GetViewTarget() == PlayerCamera)
				PlayerController->SetViewTarget(Camera->GetOwner());
				//PlayerController->SetViewTargetWithBlend(Camera->GetOwner(), SmoothBlendTime);
		}
		else
		{
			if (PlayerController->GetViewTarget() == Camera->GetOwner())
				PlayerController->SetViewTarget(PlayerCamera);
				//PlayerController->SetViewTargetWithBlend(PlayerCamera, SmoothBlendTime);
		}
	}
}
