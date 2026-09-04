# WallBreaker

> 벽을 부수고 성장하는 Unity 기반 모바일 방치형 클리커 게임

<p align="center">
  <img src="Images/screenshot.png" alt="WallBreaker 게임 화면" width="360" />
</p>

## 프로젝트 개요

| 항목 | 내용 |
|---|---|
| 장르 | 방치형 / 클리커 성장 게임 |
| 플랫폼 | Android / Google Play |
| 개발 환경 | Unity, C#, PlayFab, Firebase |
| 담당 업무 | 게임 클라이언트 및 주요 시스템 개발 |

## 주요 구현 기능

- **핵심 게임플레이**: 벽부수기 플레이와 성장 시스템 구현
- **수집 및 성장 콘텐츠**: 무기·유물 뽑기와 인벤토리 시스템 구현
- **유저 데이터 관리**: PlayFab 기반 로그인 및 데이터 저장·불러오기 연동
- **모바일 수익화**: 인앱 결제 및 보상형 광고 연동
- **플레이 보상**: 출석·퀘스트·오프라인 보상 및 일일·주간 초기화 처리
- **운영 기능**: 우편함을 통한 공지 및 보상 수령 기능 구현
- **분석 및 오류 추적**: Firebase Analytics·Crashlytics 연동

## 핵심 코드

| 파일 | 주요 구현 내용 |
|---|---|
| [Breakable.cs](Scripts/Entity/Breakable.cs) | 벽 체력·피격·파괴 상태와 시각·사운드 피드백 처리 |
| [WeaponCtrl.cs](Scripts/WeaponCtrl.cs) | 무기 초기화, 직선·회전 이동 및 충돌 처리 |
| [GachaMgr.cs](Scripts/Manager/GachaMgr.cs) | 뽑기 결과 큐, 등급별 연출 및 연출 건너뛰기 처리 |
| [OfflineMgr.cs](Scripts/Manager/OfflineMgr.cs) | 서버 시간 기반 오프라인 보상, 미수령 복구 및 광고 보상 분기 |

### 코드 출처 및 실행 범위

- 원본 저장소: `Copiasoft/WallBreaker`
- 기준 커밋: `2a8b2be258850140d3ba9d9aa990566f389eb410` (원격 HEAD와 대조 확인)
- 원본 `Assets/Scripts/`에서 선별한 실제 코드를 `Scripts/`에 경로 구조를 유지해 수록했습니다.
- 코드는 열람용 발췌본입니다. 프로젝트 전용 타입, 나머지 partial 파일, Unity 씬·프리팹 및 외부 SDK는 포함하지 않아 이 폴더만으로 빌드할 수 없습니다.

---

[← 프로젝트 목록으로 돌아가기](../README.md)
