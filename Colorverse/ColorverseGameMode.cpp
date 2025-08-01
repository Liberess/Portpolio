#include "ColorverseGameMode.h"
#include "UObject/ConstructorHelpers.h"

AColorverseGameMode::AColorverseGameMode()
{
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnBPClass(TEXT("/Game/Blueprints/BP_ColorverseCharacter"));
	if (PlayerPawnBPClass.Class != NULL)
	{
		DefaultPawnClass = PlayerPawnBPClass.Class;
	}
	
	const FSoftClassPath WidgetBPClassRef(TEXT("/Game/UI/BP_ItemAcquiredWidget.BP_ItemAcquiredWidget_C"));
	if(UClass* WidgetClass = WidgetBPClassRef.TryLoadClass<UItemAcquiredWidget>())
	{
		ItemAcquiredWidgetClass = WidgetClass;
	}
	
	const FSoftClassPath DialogueRef(TEXT("/Game/UI/BP_Dialogue.BP_Dialogue_C"));
	auto DialogueWidgetRef = DialogueRef.TryLoadClass<UDialogueWidget>();
	if(DialogueWidgetRef)
		DialogueWidget = Cast<UDialogueWidget>(CreateWidget(GetWorld(), DialogueWidgetRef));

	const FSoftClassPath InteractRef(TEXT("/Game/UI/BP_InteractWidget.BP_InteractWidget_C"));
	auto InteractWidgetRef = InteractRef.TryLoadClass<UInteractWidget>();
	if(InteractWidgetRef)
		InteractWidget = Cast<UInteractWidget>(CreateWidget(GetWorld(), InteractWidgetRef));
}

void AColorverseGameMode::BeginPlay()
{
	Super::BeginPlay();
}
