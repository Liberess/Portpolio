#include "ColorverseCharacter.h"
#include "HeadMountedDisplayFunctionLibrary.h"
#include "Sanctum.h"
#include "TimerManager.h"
#include "Camera/CameraComponent.h"
#include "Components/CapsuleComponent.h"
#include "Components/InputComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "GameFramework/Controller.h"
#include "GameFramework/SpringArmComponent.h"

AColorverseCharacter::AColorverseCharacter()
{
	GetCapsuleComponent()->InitCapsuleSize(42.f, 96.0f);
	GetCapsuleComponent()->OnComponentBeginOverlap.AddDynamic(this, &AColorverseCharacter::OnOverlapBegin);
	GetCapsuleComponent()->OnComponentEndOverlap.AddDynamic(this, &AColorverseCharacter::OnOverlapEnd);

	BaseTurnRate = 45.f;
	BaseLookUpRate = 45.f;

	bUseControllerRotationPitch = false;
	bUseControllerRotationYaw = false;
	bUseControllerRotationRoll = false;

	GetCharacterMovement()->bOrientRotationToMovement = true;
	GetCharacterMovement()->RotationRate = FRotator(0.0f, 540.0f, 0.0f);
	GetCharacterMovement()->JumpZVelocity = 600.f;
	GetCharacterMovement()->AirControl = 0.2f;
	
	CameraBoom = CreateDefaultSubobject<USpringArmComponent>(TEXT("CameraBoom"));
	CameraBoom->SetupAttachment(RootComponent);
	CameraBoom->TargetArmLength = 300.0f;
	CameraBoom->bUsePawnControlRotation = true;

	FollowCamera = CreateDefaultSubobject<UCameraComponent>(TEXT("FollowCamera"));
	FollowCamera->SetupAttachment(CameraBoom, USpringArmComponent::SocketName);
	FollowCamera->bUsePawnControlRotation = false;

	CombatSystem = CreateDefaultSubobject<UCombatSystem>(TEXT("CombatSystem"));
	LivingEntity = CreateDefaultSubobject<ULivingEntity>(TEXT("LivingEntity"));

	BrushColors = {
		FLinearColor::FromSRGBColor(FColor::Red),
		FLinearColor::FromSRGBColor(FColor::Yellow),
		FLinearColor::FromSRGBColor(FColor::Blue)
	};
	
	/*BrushColors.Add(FLinearColor(1.0f, 0.0f, 0.0f, 1.0f));
	BrushColors.Add(FLinearColor(1.0f, 1.0f, 0.0f, 1.0f));
	BrushColors.Add(FLinearColor(0.0f, 0.0f, 1.0f, 1.0f));*/
}

void AColorverseCharacter::BeginPlay()
{
	Super::BeginPlay();

	if (IsValid(GetCharacterMovement()))
		GetCharacterMovement()->MaxWalkSpeed = RunSpeed;

	InvenMgr = GetWorld()->GetSubsystem<UInventoryManager>();

	bIsDamageable = true;
}

void AColorverseCharacter::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
	if (CombatSystem->bIsCanAttackTrace)
	{
		HitCheck();
	}
}

void AColorverseCharacter::SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent)
{
	check(PlayerInputComponent);
	PlayerInputComponent->BindAction("Jump", IE_Pressed, this, &AColorverseCharacter::Jump);
	PlayerInputComponent->BindAction("Jump", IE_Released, this, &AColorverseCharacter::StopJumping);

	PlayerInputComponent->BindAction<TDelegate<void(int32)>>("Attack", IE_Pressed, this, &AColorverseCharacter::Attack, 0);
	PlayerInputComponent->BindAction("Roll", IE_Pressed, this, &AColorverseCharacter::Roll);

	PlayerInputComponent->BindAxis("MoveForward", this, &AColorverseCharacter::MoveForward);
	PlayerInputComponent->BindAxis("MoveRight", this, &AColorverseCharacter::MoveRight);

	PlayerInputComponent->BindAxis("Turn", this, &APawn::AddControllerYawInput);
	PlayerInputComponent->BindAxis("TurnRate", this, &AColorverseCharacter::TurnAtRate);
	PlayerInputComponent->BindAxis("LookUp", this, &APawn::AddControllerPitchInput);
	PlayerInputComponent->BindAxis("LookUpRate", this, &AColorverseCharacter::LookUpAtRate);

	PlayerInputComponent->BindTouch(IE_Pressed, this, &AColorverseCharacter::TouchStarted);
	PlayerInputComponent->BindTouch(IE_Released, this, &AColorverseCharacter::TouchStopped);

	PlayerInputComponent->BindAction("ResetVR", IE_Pressed, this, &AColorverseCharacter::OnResetVR);

	PlayerInputComponent->BindAction("Interact", IE_Pressed, this, &AColorverseCharacter::Interact);
	PlayerInputComponent->BindAction("Inventory", IE_Pressed, this, &AColorverseCharacter::ControlInventory);

	PlayerInputComponent->BindAction<TDelegate<void(ECombineColors)>>(
		TEXT("RedPaint"), IE_Pressed, this, &AColorverseCharacter::ChangeEquipPaint, ECombineColors::Red);
	PlayerInputComponent->BindAction<TDelegate<void(ECombineColors)>>(
		TEXT("YellowPaint"), IE_Pressed, this, &AColorverseCharacter::ChangeEquipPaint, ECombineColors::Yellow);
	PlayerInputComponent->BindAction<TDelegate<void(ECombineColors)>>(
		TEXT("BluePaint"), IE_Pressed, this, &AColorverseCharacter::ChangeEquipPaint, ECombineColors::Blue);
	PlayerInputComponent->BindAction<TDelegate<void(ECombineColors)>>(
		TEXT("EmptyPaint"), IE_Pressed, this, &AColorverseCharacter::ChangeEquipPaint, ECombineColors::Empty);
}

void AColorverseCharacter::PostInitializeComponents()
{
	Super::PostInitializeComponents();

	ColorverseAnim = Cast<UColorverseCharacterAnimInstance>(GetMesh()->GetAnimInstance());

	if (!IsValid(ColorverseAnim))
		return;

	ColorverseAnim->OnNextAttackCheck.AddDynamic(this, &AColorverseCharacter::SetNextAttackCheck);
	ColorverseAnim->OnStartAttackJudg.AddDynamic(this, &AColorverseCharacter::SetEnableCanAttackTrace);
	ColorverseAnim->OnEndAttackJudg.AddDynamic(this, &AColorverseCharacter::SetDisableCanAttackTrace);
	ColorverseAnim->OnEndAttack.AddDynamic(this, &AColorverseCharacter::SetDisabledAttack);
}

void AColorverseCharacter::SetNextAttackCheck()	
{
	CombatSystem->bIsCanInput = true;
}

void AColorverseCharacter::SetEnableCanAttackTrace()
{
	bIsCanMove = false;

	CombatSystem->bIsCanAttackTrace = true;
	AttackHitResults.Empty();

	IsDrawing = true;
}

void AColorverseCharacter::SetDisableCanAttackTrace()
{
	CombatSystem->bIsCanAttackTrace = false;

	IsDrawing = false;
	
	if(IsValid(CurrentPaintedCollectObj))
		CurrentPaintedCollectObj->IsDrawing = false;
}

void AColorverseCharacter::CureHeath(int HealAmount)
{
	LivingEntity->CureHealth(HealAmount);
}

#pragma region Movement 
void AColorverseCharacter::OnResetVR()
{
	UHeadMountedDisplayFunctionLibrary::ResetOrientationAndPosition();
}

void AColorverseCharacter::TouchStarted(ETouchIndex::Type FingerIndex, FVector Location)
{
	Jump();
}

void AColorverseCharacter::TouchStopped(ETouchIndex::Type FingerIndex, FVector Location)
{
	StopJumping();
}

void AColorverseCharacter::TurnAtRate(float Rate)
{
	AddControllerYawInput(Rate * BaseTurnRate * GetWorld()->GetDeltaSeconds());
}

void AColorverseCharacter::LookUpAtRate(float Rate)
{
	AddControllerPitchInput(Rate * BaseLookUpRate * GetWorld()->GetDeltaSeconds());
}

void AColorverseCharacter::MoveForward(float Value)
{
	if (LivingEntity->GetDead())
		return;

	if (bIsAttacked || bIsRolling)
		return;

	if (!bIsCanMove)
		return;

	if ((Controller != nullptr))
	{
		if (Value != 0.0f)
		{
			const FRotator Rotation = Controller->GetControlRotation();
			const FRotator YawRotation(0, Rotation.Yaw, 0);

			const FVector Direction = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X);

			MoveInputY = Direction * Value;

			if (bIsAttacking && CombatSystem->bIsCanInput && bIsCanMove)
			{
				SetDisabledAttack();
			}
			else if (bIsAttacking)
			{
				Value *= 0.001f;
			}

			AddMovementInput(Direction, Value);
		}
		else
		{
			MoveInputY = FVector::ZeroVector;
		}
	}
}

void AColorverseCharacter::MoveRight(float Value)
{
	if (LivingEntity->GetDead())
		return;

	if (bIsAttacked || bIsRolling)
		return;

	if (!bIsCanMove)
		return;

	if ((Controller != nullptr))
	{
		if (Value != 0.0f)
		{
			const FRotator Rotation = Controller->GetControlRotation();
			const FRotator YawRotation(0, Rotation.Yaw, 0);

			const FVector Direction = FRotationMatrix(YawRotation).GetUnitAxis(EAxis::Y);

			MoveInputX = Direction * Value;

			if (bIsAttacking && CombatSystem->bIsCanInput && bIsCanMove)
			{
				SetDisabledAttack();
			}
			else if (bIsAttacking)
			{
				Value *= 0.001f;
			}

			AddMovementInput(Direction, Value);
		}
		else
		{
			MoveInputX = FVector::ZeroVector;
		}
	}
}

void AColorverseCharacter::SetEnabledToggleRun()
{
	bIsRunning = true;
	GetCharacterMovement()->MaxWalkSpeed = RunSpeed;
}

void AColorverseCharacter::Jump()
{
	if (LivingEntity->GetDead())
		return;
	
	if (bIsRolling)
		return;

	if (bIsAttacked)
		return;

	if (bIsRolling)
		return;

	if (!CombatSystem->bIsCanInput)
		return;

	Super::Jump();
}

void AColorverseCharacter::StopJumping()
{
	Super::StopJumping();
}
#pragma endregion Movement

void AColorverseCharacter::Attack_Implementation(int skillNum)
{
	if (LivingEntity->GetDead())
		return;

	if (bIsAttacked)
		return;

	if (bIsRolling)
		return;

	if (GetCharacterMovement()->IsFalling())
	{
		ColorverseAnim->PlayJumpAttackMontage();
		bIsAttacking = true;
	}
		
	if (bIsAttacking)
	{
		if (CombatSystem->bIsCanInput)
		{
			CombatSystem->AttackStartComboState();

			if (CombatSystem->bCanNextCombo)
			{
				ColorverseAnim->JumpToAttackMontageSection(CombatSystem->CurrentCombo, skillNum);
			}
		}
	}
	else if(!bIsAttacking)
	{
		CombatSystem->AttackStartComboState();
		ColorverseAnim->PlayAttackMontage(skillNum);
		bIsAttacking = true;
	}
}

void AColorverseCharacter::SetDisabledAttack_Implementation()
{
	bIsAttacking = false;
	bIsCanMove = true;
	CombatSystem->AttackEndComboState();
}

void AColorverseCharacter::Roll_Implementation()
{
	if (LivingEntity->GetDead())
		return;

	if (bIsAttacked)
		return;

	if (!bIsRunning || bIsRolling)
		return;

	if (!CombatSystem->bIsCanInput)
		return;

	if (GetCharacterMovement()->IsFalling())
		return;

	bIsRolling = true;
	bIsDamageable = false;
}

void AColorverseCharacter::SetDisabledRoll_Implementation()
{
	bIsRolling = false;
	bIsDamageable = true;
}

void AColorverseCharacter::HitCheck_Implementation()
{

}

void AColorverseCharacter::Attacked_Implementation(FDamageMessage damageMessage)
{
	if (LivingEntity->GetDead())
		return;

	LivingEntity->ApplyDamage(damageMessage);

	if (LivingEntity->CurrentHealth <= 0)
	{
		Dead();
		return;
	}

	bIsAttacked = true;
	ColorverseAnim->PlayDamagedMontage();
}

void AColorverseCharacter::Dead_Implementation()
{
	LivingEntity->SetDead(true);
}

void AColorverseCharacter::OnOverlapBegin(class UPrimitiveComponent* OverlappedComp, class AActor* OtherActor,
                                          class UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep,
                                          const FHitResult& SweepResult)
{
	if (OtherActor && OtherActor != this)
	{
		InteractObject = Cast<AInteractObject>(OtherActor);
		if (IsValid(InteractObject))
		{
			bIsInteract = true;

			if (!bIsWatchingInteractWidget && InteractObject->GetInteractable())
			{
				if (IsValid(InteractWidgetClass))
				{
					if(InteractWidget == nullptr)
						InteractWidget = Cast<UInteractWidget>(CreateWidget(GetWorld(), InteractWidgetClass));
					
					InteractWidget->SetInteractText(FText::FromString(InteractObject->InteractWidgetDisplayTxt));
					bIsWatchingInteractWidget = true;
					InteractWidget->AddToViewport();
				}
			}
		}
	}
}

void AColorverseCharacter::OnOverlapEnd(class UPrimitiveComponent* OverlappedComp, class AActor* OtherActor,
                                        class UPrimitiveComponent* OtherComp, int32 OtherBodyIndex)
{
	if (OtherActor && OtherActor != this)
	{
		if (IsValid(InteractObject) && InteractObject == OtherActor)
		{
			InteractObject = nullptr;
			bIsInteract = false;

			if (InteractWidget != nullptr)
			{
				bIsWatchingInteractWidget = false;
				InteractWidget->RemoveFromParent();
				InteractWidget = nullptr;
			}
		}
	}
}

void AColorverseCharacter::ChangeEquipPaint_Implementation(ECombineColors CombineColor)
{
	if (!CombatSystem->bIsCanInput)
		return;

	CombatSystem->CurrentPaintColor = CombineColor;

	if(CombineColor != ECombineColors::Empty)
	{
		//CombatSystem->SetColorBuff();
		//CombatSystem->SetElementBuff((int)CombineColor);
	}
}

void AColorverseCharacter::ControlInventory_Implementation()
{
	InvenMgr->SetInventoryUI(true, true);
}

void AColorverseCharacter::Interact_Implementation()
{
	if (!bIsInteractable || !bIsInteract || !IsValid(InteractObject))
		return;

	bIsInteractable = false;

	GetWorld()->GetTimerManager().ClearTimer(InteractCoolTimer);
	GetWorld()->GetTimerManager().SetTimer(InteractCoolTimer, [this]()
	{
		bIsInteractable = true;
	}, InteractCoolTime, false);

	InteractObject->Execute_OnInteract(InteractObject);

	if (bIsWatchingInteractWidget && InteractWidget != nullptr)
	{
		bIsWatchingInteractWidget = false;
		InteractWidget->RemoveFromParent();
		InteractWidget = nullptr;
	}
}