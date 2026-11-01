# Unity 에셋 Import 목록

조사일: 2026-10-31

## 조사 결과

기존의 폴더명 추정 목록을 **Unity가 파일에 남긴 출처 기록과 실제 파일 대조 결과**로 교체했습니다.

- `Assets/`의 `.meta` 9,351개를 조사했습니다.
- **6,580개 파일의 `.meta`에 `AssetOrigin` 출처 기록이 남아 있으며, 상품 ID 기준 37개 패키지로 집계됩니다.**
- 그중 33개 상품은 로컬 캐시 원본과 프로젝트 파일의 GUID 대조로도 확인했습니다.
- 따라서 현재 프로젝트에서 Unity의 출처 기록으로 확인한 에셋 패키지는 **37개**입니다.

이 숫자는 **현재 남아 있는 파일의 출처 목록**입니다. 과거에 Import했다가 모두 삭제한 패키지, Import 버튼을 누른 날짜, 상품 전체의 Import 여부, 게임에서 실제 사용하는지는 나타내지 않습니다.

## 확인 방법

1. 모든 프로젝트 `.meta`를 읽고 `AssetOrigin`의 `productId`, `packageName`, `packageVersion`을 집계했습니다. 폴더명으로 상품명을 추정하지 않았습니다.
2. 로컬 Asset Store 캐시의 `.unitypackage` 94개를 읽었습니다. gzip 헤더의 상품 ID·상품명·버전과, 내부 파일의 GUID를 확인했습니다.
3. 프로젝트 파일의 GUID와 대조하고, 일치 파일은 SHA-256으로 내용까지 비교했습니다. 폴더 GUID와 `.meta` 파일 자체는 파일 일치 수에서 제외했습니다.
4. 프로젝트에 들어 있는 변환용 `.unitypackage` 7개도 추가 대조했습니다. 이 아카이브가 있다는 이유만으로 Import됐다고 판단하지 않았습니다.
5. 여러 상품이 공유하는 샘플 파일이나 일반 프로젝트 설정 파일만 일치하는 경우에는 해당 상품을 확인 목록에 넣지 않았습니다.

프로젝트 파일 GUID 중복과 아카이브 읽기 오류는 발견되지 않았습니다. 원본 파일을 추출하거나 프로젝트 에셋을 변경하지 않고 조사했습니다.

## 1. Unity 출처 기록으로 확인된 패키지: 37개

상품명과 버전은 **현재 프로젝트의 `AssetOrigin` 기록 그대로**입니다. 다운로드 캐시의 상품명·버전과 다를 수 있습니다.

`기록 파일 수`는 해당 상품의 출처가 기록된 현재 파일 수입니다. 원본 상품 전체 파일 수나 실제 사용 중인 파일 수가 아닙니다.

| # | 상품명 | 상품 ID | 기록 버전 | 기록 파일 수 | 대표 `.meta` 근거 |
|---|---|---:|---|---:|---|
| 1 | 3D Low Poly Environment Assets | 299354 | 1.0 | 120 | [GradientMaterials.mat.meta](<Assets/My Resources/For Map/For FireMap/Materials/GradientMaterials.mat.meta>) |
| 2 | 3D Low Poly Magical Forest | 323631 | 1.0.0 | 236 | [M_Glow_01.mat.meta](<Assets/My Resources/For Map/For Jungle/PolitePenguin/3DLowPolyMagicalForest/Materials/M_Glow_01.mat.meta>) |
| 3 | 500 Free Skill Icons | 344112 | 1.0 | 24 | [Attack Power Increase.png.meta](<Assets/My Resources/Image/Skill Icon Using/Attack Power Increase.png.meta>) |
| 4 | Battle Wizard Poly Art | 128097 | 1.2 | 47 | [Attack01.fbx.meta](<Assets/My Resources/Animations/Wizard_Player/Attack01.fbx.meta>) |
| 5 | Cartoon FX Remaster Free | 109565 | R 1.5.0 | 482 | [Wind Leaf.mat.meta](<Assets/Materials/Ranged Enemy/Objects/Wind Leaf.mat.meta>) |
| 6 | Cartoon Low-Poly Spaceship | 287099 | 1.0.0 | 67 | [Readme.pdf.meta](<Assets/My Resources/For Map/For Lightning Map/FlexUnit/CartoonSpaceship/Readme.pdf.meta>) |
| 7 | Dungeon Modular Pack | 295430 | 1.0 | 55 | [M_Arch_A.mat.meta](<Assets/My Resources/Materials/Dungeon Map/M_Arch_A.mat.meta>) |
| 8 | Fantasy FREE - Low Poly 3D Models Pack | 260367 | 2.1 | 100 | [Material.mat.meta](<Assets/My Resources/For Map/For Jungle/ithappy/Fantasy_FREE/Materials/Material.mat.meta>) |
| 9 | Free Demo Of Low Poly Space Alien Worlds 3D Asset Pack | 258683 | 1.0.0 | 77 | [Asset_Overview.unity.meta](<Assets/My Resources/For Map/For Lightning Map/Free Demo of Low Poly Space Alien Worlds 3D Asset Pack/Demo Scenes/Asset_Overview.unity.meta>) |
| 10 | FREE Stylized Textures - RPG Environment | 204187 | 1.0 | 60 | [StylizedFREETexturesDEMO.unity.meta](<Assets/My Resources/Materials/Materials for Orb/Art/Textures/Demo/StylizedFREETexturesDEMO.unity.meta>) |
| 11 | FREE Ultimate Procedural Primitives | 227955 | 1.0 | 78 | [프로젝트 내 안내 파일의 .meta](<Assets/My Resources/Prefabs/MeshAssets/00_Recommend Copy to your working folder and Use.meta>) |
| 12 | Giant Monster Model - Golem | 278960 | 1.0 | 57 | [Giant01_Texture01 Recolor01.mat.meta](<Assets/My Resources/Materials/Ranged Golem/Giant01/Giant01_Texture01/Giant01_Texture01 Recolor01.mat.meta>) |
| 13 | GUI - The Stone | 116852 | 2.3.0 | 2,173 | [LayerLab_UserAssetGuide.txt.meta](<Assets/My Resources/UI/Layer Lab/GUI-TheStone/+README+/LayerLab_UserAssetGuide.txt.meta>) |
| 14 | GUI Pro - Fantasy RPG | 170168 | 4.0.3 | 5 | [UIParticleSystem.cs.meta](<Assets/My Resources/UI/Layer Lab/GUI Pro-FantasyRPG/Extensions/UIParticle/UIParticleSystem.cs.meta>) |
| 15 | Hun0FX Fire vol1 | 321082 | 1.0 | 56 | [FireFX_vol1_demo.unity.meta](<Assets/My Resources/VFX/Hun0FX - FIre VFX - Using/DemoScene/FireFX_vol1_demo.unity.meta>) |
| 16 | Inferno World Free - Low Poly 3D Models | 328402 | 1.0 | 184 | [Bone.mat.meta](<Assets/My Resources/For Map/For FireMap/ithappy/Inferno_World_Free/Materials/Bone.mat.meta>) |
| 17 | KayKit - Adventurers Character Pack (for Unity) | 290679 | 2.0.1 | 360 | [T-Pose.anim.meta](<Assets/My Resources/Adventures Character Pack/KayKit/Characters/Animations/Animations/Rig_Large/T-Pose.anim.meta>) |
| 18 | Lite Dungeon Pack - Low Poly 3D Art by Gridness | 242692 | 1.0 | 34 | [Mat_Flag.mat.meta](<Assets/My Resources/Materials/Map - 0/Mat_Flag.mat.meta>) |
| 19 | Low Poly Atmospheric Locations Pack | 278928 | 1.0.0 | 439 | [guideline.pdf.meta](<Assets/My Resources/For Map/VariousMap Pack/Low Poly Atmospheric Locations Pack/guideline.pdf.meta>) |
| 20 | Low Poly Dungeons Lite | 177937 | 1.11 | 40 | [LowPolyDungeonsLite_BG.mat.meta](<Assets/My Resources/Materials/Dungeon Map/LowPolyDungeonsLite_BG.mat.meta>) |
| 21 | Lowpoly Environment Extreme Pack | 238098 | 2.0 | 231 | [Documentation.pdf.meta](<Assets/My Resources/For Map/Desert_Forest_Winter Map Pack/Documentation.pdf.meta>) |
| 22 | Magic Effects FREE | 247933 | 1.6 | 164 | [Demo fire 1Settings.lighting.meta](<Assets/My Resources/VFX/Hovl Studio - Various - Using/Demo scene/Demo fire 1Settings.lighting.meta>) |
| 23 | Mini Legion Grunt PBR HP | 98187 | 1.1 | 24 | [Attack01.anim.meta](<Assets/My Resources/Animations/Orc/Attack01.anim.meta>) |
| 24 | Mini Legion Lich PBR HP Polyart | 91497 | 1.3 | 23 | [Attack01.fbx.meta](<Assets/My Resources/Animations/Wizard_Enemy/Lich/Attack01.fbx.meta>) |
| 25 | Mini Legion Rock Golem PBR HP Polyart | 94707 | 1.2 | 30 | [Attack01.anim.meta](<Assets/My Resources/Animations/Golem/Attack01.anim.meta>) |
| 26 | Mini Simple Characters \| Skeleton \| Free Demo | 262897 | 1.0 | 11 | [Mini Simple Characters Demo Mat primary.mat.meta](<Assets/My Resources/Materials/Mini Skeleton/Mini Simple Characters Demo Mat primary.mat.meta>) |
| 27 | Monster Minion Survivor PBR Polyart | 269515 | 1.0 | 31 | [Swarm08.controller.meta](<Assets/My Resources/Animations/Bird & Stingray/Swarm08/Swarm08.controller.meta>) |
| 28 | Monsters Ultimate Pack 02 Cute Series | 179083 | 1.0 | 653 | [Bloom.controller.meta](<Assets/My Resources/Monsters_Pack_2/Monsters Ultimate Pack 02 Cute Series/Bloom Cute Series/FBX/Bloom.controller.meta>) |
| 29 | Nature Biomes Pack - Low Poly | 305817 | 1.0 | 306 | [NatureBiomesPack.unity.meta](<Assets/Scenes/NatureBiomesPack.unity.meta>) |
| 30 | Party Monster Duo Polyart PBR | 195698 | 1.2 | 51 | [Attack01.fbx.meta](<Assets/My Resources/Animations/PartyMonsterDuo/Attack01.fbx.meta>) |
| 31 | RPG Monster Buddy PBR Polyart | 253961 | 1.0 | 75 | [Cactus.controller.meta](<Assets/My Resources/Animations/Cactus & Mushroom/Cactus/Cactus.controller.meta>) |
| 32 | RPG Monster Duo PBR Polyart | 157762 | 1.5 | 47 | [Attack01_Slime_Anim.fbx.meta](<Assets/My Resources/Animations/Slime/Attack01_Slime_Anim.fbx.meta>) |
| 33 | RPG Monster Partners PBR Polyart | 168251 | 1.5 | 50 | [Attack01.fbx.meta](<Assets/My Resources/Animations/Octopus & Box/Beholder/Attack01.fbx.meta>) |
| 34 | Simple Forest Pack | 209273 | 1.1 | 131 | [Color001.mat.meta](<Assets/My Resources/For Map/For Jungle/Forest Pack/Materials/Color001.mat.meta>) |
| 35 | Stone Monster | 101433 | 1.0 | 8 | [StoneMonster_Attack.FBX.meta](<Assets/My Resources/Animations/StoneMonster/StoneMonster_Attack.FBX.meta>) |
| 36 | VFX URP - Fire Package | 305098 | 1.0 | 23 | [FireScene_Volume.asset.meta](<Assets/My Resources/VFX/Fire VFX - Using/FireScene_Volume.asset.meta>) |
| 37 | Weapons Pack - Bullets | 302702 | 0.0.1 | 28 | [Bullets_M.mat.meta](<Assets/My Resources/Adventures Character Pack/For Bullet/Bullets Pack/Materials/Bullets_M.mat.meta>) |

### 출처 기록 해석 시 주의할 항목

- **GUI Pro - Fantasy RPG:** 확인된 출처 기록은 5개 파일뿐입니다. 대표 파일은 `UIParticle` 확장 코드이므로, UI 상품 전체가 현재 남아 있다는 뜻은 아닙니다.
- **Weapons Pack - Bullets:** 프로젝트 출처 기록의 상품명입니다. 같은 상품 ID의 로컬 캐시 파일명과 헤더 상품명은 `Free Pack - Bullets`였습니다. 이름이 아니라 ID로 같은 상품임을 대조했습니다.
- `AssetOrigin`은 파일 이동·복사 이후에도 남을 수 있습니다. 따라서 기록 파일 수를 원본 상품의 Import 파일 수로 해석하면 안 됩니다.

## 2. 원본 파일 대조 결과

- 캐시 아카이브 94개 중 38개에서 프로젝트 파일과 GUID 일치가 있었습니다.
- 이 중 33개는 위 `AssetOrigin` 상품 ID와도 일치합니다.
- 나머지 5개는 공통 리소스 또는 일반 설정 파일만 일치하여 아래 제외 목록으로 분류했습니다.
- GUI - The Stone, GUI Pro - Fantasy RPG, KayKit, Monsters Ultimate Pack 02 Cute Series는 해당 상품의 글로벌 캐시가 없어 `AssetOrigin`을 근거로 확인했습니다.
- KayKit은 프로젝트에 포함된 URP 변환 아카이브와도 추가 대조했습니다. 파일 GUID 329개가 일치했고, 그중 319개는 내용도 동일했습니다.

**같은 GUID라고 해서 원본 패키지 Import가 확정되는 것은 아닙니다.** TextMesh Pro처럼 여러 상품에 포함되거나 Unity에서 따로 Import할 수 있는 리소스가 있기 때문입니다. 원본 캐시의 버전 또한 프로젝트의 Import 버전을 보장하지 않습니다.

## 3. Import 확인 목록에서 제외한 후보

캐시에 다운로드 파일이 있거나 GUID 일부가 일치하는 것만으로는 Import한 상품이라고 단정하지 않았습니다.

| 후보 상품 | 일치 파일 수 | 제외 이유 |
|---|---:|---|
| RPG Tiny Hero Duo PBR Polyart | 2 | Party Monster Duo에도 들어 있는 공통 `Skybox_Mat.mat`, `Stage.mat`만 일치합니다. 현재 파일의 출처 기록은 Party Monster Duo입니다. |
| Pirate Customized | 1 | 여러 상품이 공유하는 데모 Scene GUID만 일치하며, 파일 내용도 다릅니다. |
| Picture Frames | 1 | `Assets/Settings/SampleSceneProfile.asset`의 GUID만 일치합니다. 파일 내용이 다르고 액자 모델을 확인할 근거가 없어 이 상품을 확정하지 않았습니다. |
| VR Beginner: The Escape Room | 2 | 다른 상품에도 포함된 Unity 템플릿 `Readme.cs`, `ReadmeEditor.cs` GUID만 일치합니다. 파일 내용도 다릅니다. |
| Particle Pack | 28 | 27개는 함께 번들된 TextMesh Pro 리소스이고, 1개는 URP 전역 설정 파일입니다. 8개는 내용까지 같지만 모두 TextMesh Pro 공통 리소스이므로 Particle Pack Import의 근거로 삼지 않았습니다. |

캐시 94개 중 56개는 현재 프로젝트 파일과 GUID 일치가 없었습니다. 이것도 **과거에 한 번도 Import하지 않았다는 증거는 아닙니다.** 다른 버전을 Import했거나, Import 후 삭제했거나, `.meta`를 다시 만들었을 수 있습니다.

변환용 아카이브 7개 중 ithappy의 Built-In/HDRP/URP 변환 아카이브에서도 공통 파일이 일치했습니다. 변환 아카이브 간 공유 파일 때문에 어느 변환 패키지의 Import 버튼을 눌렀는지까지는 단정하지 않았으며, 별도 상품으로 중복 집계하지 않았습니다.

## 4. Unity Package Manager 직접 의존성

아래는 Asset Store `.unitypackage` 목록과 별개입니다. [manifest.json](Packages/manifest.json)의 **현재 직접 의존성**이며, 기본 엔진 `com.unity.modules.*` 항목과 간접 의존성은 이 표에서 제외했습니다. 기본 제공 패키지와 자동 추가 패키지도 포함되므로 사용자가 직접 설치한 이력은 아닙니다.

| 패키지 | 버전 |
|---|---|
| `com.unity.2d.sprite` | 1.0.0 |
| `com.unity.addressables` | 2.7.4 |
| `com.unity.ai.navigation` | 2.0.9 |
| `com.unity.collab-proxy` | 2.10.0 |
| `com.unity.entities` | 1.4.2 |
| `com.unity.entities.graphics` | 1.4.15 |
| `com.unity.ide.rider` | 3.0.38 |
| `com.unity.ide.visualstudio` | 2.0.25 |
| `com.unity.inputsystem` | 1.14.2 |
| `com.unity.memoryprofiler` | 1.1.9 |
| `com.unity.multiplayer.center` | 1.0.0 |
| `com.unity.nuget.newtonsoft-json` | 3.2.2 |
| `com.unity.postprocessing` | 3.5.0 |
| `com.unity.recorder` | 5.1.5 |
| `com.unity.render-pipelines.universal` | 17.0.4 |
| `com.unity.test-framework` | 1.6.0 |
| `com.unity.timeline` | 1.8.9 |
| `com.unity.ugui` | 2.0.0 |
