 ## 現在の構成

  - Unity: 6000.5.2f1
  - 描画: URP 2D
  - 入力: New Input System
  - カメラ: Cinemachineを追加中
  - Scene: title、start、coconMain、game_over、resultなど
  - テスト・asmdef: なし

  主要なゲーム要素は次のように分かれています。

  - プレイヤー移動・左右の傘判定
    Assets/Scripts/PlayerController.cs:4

  - 王の移動・買い物・ゴールへの遷移
    Assets/Scripts/KingController.cs:3

  - 王のHP
    Assets/Scripts/KingHealth.cs:4

  - 日陰にいない場合の継続ダメージ
    Assets/Scripts/KingSun.cs:3

  - 飛来物の生成・移動・ダメージ
    Assets/Scripts/ProjectileManager.cs:3
    Assets/Scripts/Projectile.cs:4

  coconMain.unityにはKing、プレイヤー、ProjectileManager、ゴールなどのPrefabが配置され、Cinemachineも追加されています。ここが実
  質的なメインゲームSceneと見られます。