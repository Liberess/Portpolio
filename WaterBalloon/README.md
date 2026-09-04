# WaterBalloon

> Unity와 Photon Fusion 기반의 온라인 멀티플레이 장애물 레이스 게임

<p align="center">
  <img src="Images/screenshot.png" alt="WaterBalloon 게임 화면" width="800" />
</p>

## 프로젝트 개요

| 항목 | 내용 |
|---|---|
| 장르 | 멀티플레이 장애물 레이스 |
| 플랫폼 | PC / Steam, STOVE |
| 개발 환경 | Unity, C#, Photon Fusion |
| 개발 기간 | 2026.05.07 ~ 2026.07.20 (최초 Git 기록부터 출시까지) |
| 출시일 | 2026.07.20 |
| 담당 업무 | 게임 클라이언트 및 주요 시스템 개발 |

## 주요 구현 기능

- **핵심 게임플레이**: 캐릭터 조작 및 장애물 레이스 플레이 로직 구현
- **캐릭터 커스터마이징**: 외형 변경 및 코스튬 적용 기능 구현
- **커뮤니티 맵 에디터**: 사용자 제작 맵 편집 및 공유 기능 구현
- **온라인 멀티플레이**: Photon Fusion 기반 네트워크 연동 및 게임 상태 동기화
- **크로스플레이**: Steam–STOVE 사용자 간 멀티플레이 지원

## 핵심 코드

| 파일 | 주요 구현 내용 |
|---|---|
| [PlayerCtrl.cs](Scripts/PlayerCtrl.cs) | 캐릭터 이동·점프 입력, 로컬 제어와 원격 프록시 처리, RPC 이동 상태 동기화 |
| [PlayerCtrl.Custom.cs](Scripts/PlayerCtrl.Custom.cs) | 외형 파츠·색상 적용 및 네트워크 커스터마이징 동기화 |
| [EditHistory.cs](Scripts/Workshop/Editing/EditHistory.cs) | 명령 단위 Undo/Redo 이력 관리 |
| [WorkshopMapDocument.cs](Scripts/Workshop/Editing/WorkshopMapDocument.cs) | 맵 오브젝트 추가·삭제·복제·변형과 편집 명령 적용 |
| [WorkshopMapValidator.cs](Scripts/Workshop/Data/WorkshopMapValidator.cs) | 맵 배포 전 스키마·배치·속성·시작 지점 검증 |

### 코드 출처 및 실행 범위

- 원본 저장소: `Copiasoft/WaterBalloon`
- 기준 커밋: `b53086a7f333805da743120eb7f416d2c04e14ac` (원격 HEAD와 대조 확인)
- 원본 `Assets/Scripts/`에서 선별한 실제 코드를 `Scripts/`에 경로 구조를 유지해 수록했습니다.
- 코드는 열람용 발췌본입니다. 프로젝트 전용 타입, 나머지 partial 파일, Unity 씬·프리팹 및 외부 SDK는 포함하지 않아 이 폴더만으로 빌드할 수 없습니다.

---

[← 프로젝트 목록으로 돌아가기](../README.md)
