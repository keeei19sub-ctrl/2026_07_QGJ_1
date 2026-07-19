# 現状UI一覧

確認日: 2026-07-14

## 調査範囲と結論

現在のビルドで有効になっているSceneは `Assets/coconMain.unity` のみで、画面UIは同Sceneの `UIDocument` が読み込む `Assets/UIs/GameUI.uxml` に集約されている。

プロジェクト内の全Scene、UXML、UI制御コードを確認した結果、現在存在する画面UIは次の7系統である。

1. 王のHPゲージ
2. ゴールまでの進行度ゲージ
3. 選択中アイテム表示
4. 店の商品表示
5. 購入結果メッセージ
6. 画面外にいる王の方向表示
7. 飛来物の左右警告

`title`、`start`、`game_over`、`result` などのScene名は存在するが、現状はカメラだけの空Sceneであり、タイトル、操作説明、ゲームオーバー、リザルトに相当するUIは実装されていない。また、UGUIの `Canvas`、TextMesh Pro、IMGUIによる別系統の画面UIも存在しない。

## 画面上の配置

基準解像度は1980×1080で、画面サイズに合わせてスケーリングされる。主な配置は次のとおり。

| 位置 | 表示要素 | 常時表示か |
| --- | --- | --- |
| 左上を起点とする画面の50%×50% | 王のHPゲージ | 常時 |
| 左端3%、上端15%、幅8%、高さ70% | 縦型の進行度ゲージ | 常時 |
| 右下、幅15%、高さ35% | 選択中アイテム | 常時 |
| 右端、幅25%、高さ100%の領域内 | 店の商品 | 店の範囲内のみ |
| 右端、上端57%、幅25%、高さ7% | 購入結果メッセージ | 背景は常時、文言は購入時のみ |
| 画面上端または下端 | 王の方向表示 | 王が上下方向に画面外へ出たときのみ |
| 画面左端または右端 | 飛来物警告 | 飛来物の出現直前のみ |

## 各UI要素

### 1. 王のHPゲージ

- UXML要素: `HealthBackground`、`HealthBarContainer`、`HealthBar`
- 見た目: `UIHealthFrame.png` の枠内に `UIHealthBar.png` のゲージを表示する。
- 配置: 画面左上を起点に幅50%、高さ50%。枠内のゲージ領域は左41%、上34%、幅56%、高さ28%。
- 表示内容: 王の現在HP ÷ 最大HP。
- 更新契機: 王がダメージまたは回復を受けたとき。
- 初期値: 100%。

注意: `HealthBar` に `flex-grow: 1` が設定されたまま、コードでは `width` を変更している。レイアウト上、指定した残量どおりに横幅が縮まない可能性がある。

### 2. ゴールまでの進行度ゲージ

- UXML要素: `ProgressBarBackground`、`ProgressBarContainer`、`ProgressBar`
- 見た目: `ProgressBackground.png` の縦長の枠内を、赤い矩形が下から上へ満たす。
- 配置: 画面左端から3%、上端から15%、幅8%、高さ70%。内側のゲージ領域は枠の左14%、上3%、幅72%、高さ94%。
- 表示内容: 王の開始Y座標からゴールY座標までの進行率。
- 更新契機: 毎フレーム。
- 初期値: 0%。

### 3. 選択中アイテム表示

- UXMLテンプレート: `ItemUI.uxml`
- UXML要素: `Background`、`ItemPicture`、`ItemName`、`ItemCount`、`ItemDescription`
- 配置: 画面右下、幅15%、高さ35%。
- 見た目:
  - 全体に半透明の黒背景。
  - 上半分にアイテム画像。
  - 中央に白背景の商品名と所持数。
  - 下部に薄い黄緑背景の説明文。
- アイテム未所持時:
  - 名前: `アイテムなし`
  - 個数: `×0`
  - 説明: `アイテムを入手すると、ここに説明が表示されます`
  - 画像は非表示ではなく、不透明度25%で薄く残る。
- アイテム所持時:
  - 名前: 選択中アイテムの表示名。空なら `名称未設定`。
  - 個数: `×所持数`
  - 説明: アイテムの説明。空なら `説明はありません`。
  - 画像の不透明度は100%。

現状登録されている表示内容は次のとおり。

| 商品名 | 価格 | 説明 |
| --- | ---: | --- |
| 王の薬 | 30 G | Shiftキーで使用。王のHPを最大HPの10%回復する。 |
| 大傘の薬 | 50 G | Shiftキーで使用。10秒間、傘と防御範囲を2倍にする。 |
| 投石止めのお守り | 70 G | Shiftキーで使用。10秒間、すべての投石を停止する。 |

注意: `ItemPicture` の背景画像は常に共通の `Item.png` であり、選択したアイテムごとの画像差し替え処理はない。

### 4. 店の商品表示

- UXMLテンプレート: `ShopUI.uxml`
- UXML要素: `ShopUI`、`Background`、`ShopItemName`、`ShopItemPrice`、`ShopItemDescription`
- 配置: 画面右端の幅25%、高さ100%の領域。上5%、下25%、右5%の内側余白がある。
- 見た目: `店商品.png` を背景に、商品名、価格、説明欄を重ねる。
- 表示条件: プレイヤーが店の範囲内にいる間だけ表示。起動時および店の範囲外では非表示。
- 表示内容:
  - 商品名: 店に設定されたアイテム名。空なら `名称未設定`。
  - 価格: `価格 G`。
  - 商品未設定時は名前が `商品未設定`、価格が `--`。

注意: `ShopItemDescription` はUXML上にあるが、コードから値を設定していない。そのため現状は、店を開いても初期文字列の `Description` が表示される。

### 5. 購入結果メッセージ

- UXMLテンプレート: `ItemResultUI.uxml`
- UXML要素: 無名の黒背景、`ShopMessageLabel`
- 配置: 画面右端、上端57%、幅25%、高さ7%。
- 見た目: 黒背景に白文字。
- 通常時: 文言は空。
- 購入操作後に表示しうる文言:
  - `購入しました`
  - `お金が足りません`
  - `アイテムは1個しか持てません`
  - `商品が設定されていません`
  - `購入できません`
  - `購入に失敗しました`

注意: このテンプレートは `ShopUI` の外にあり、店を閉じる処理でも要素自体は非表示にならない。文言だけが空になるため、黒い帯は常時表示される構造になっている。

### 6. 画面外にいる王の方向表示

- UXML要素: `KingAboveIndicator`、`KingBelowIndicator`
- 見た目: 黄色に着色した `Triangle.png`。
- 大きさ: 幅5%、高さ9%。
- 表示条件:
  - 王全体が画面上へ出ると、画面上端に上向き表示。
  - 王全体が画面下へ出ると、画面下端に下向き表示。
  - 王が画面内、カメラ背面、または参照不足の場合は非表示。
- 横位置: 王の画面上のX座標に追従し、左右端から最低1%の余白を確保する。
- 初期状態: 非表示。

### 7. 飛来物の左右警告

- UXML要素: `ProjectileWarningLeft`、`ProjectileWarningRight`
- 見た目: 薄赤色に着色した王の画像。右側は左右反転。
- 大きさと位置: 幅16%、高さ42%。左端または右端に接し、上端29%。
- 表示条件: 飛来物が出現する0.8秒前から出現直前まで、飛来方向側だけを表示する。
- 攻撃停止アイテムの効果中、攻撃キャンセル時、管理オブジェクト無効化時は非表示。
- 初期状態: 非表示。

## 定義と実装の不整合

### 所持金表示はコードにだけ存在する

`UIHandler` は `MoneyLabel` を探し、`所持金: 金額 G` を設定しているが、現在の全UXMLに `MoneyLabel` は存在しない。nullを許容する実装のためエラーにはならないが、所持金は画面に表示されない。

### 店の商品説明が更新されない

`ShopItemDescription` は定義されているが、`UIHandler` は参照も更新もしていない。実データの説明ではなく `Description` のままになる。

### 購入結果の黒背景が常時残る

購入結果UIは店UIと別のルート要素である。店を閉じてもラベルだけが空になり、黒背景は消えない。

### HPゲージの伸縮指定が競合している

`HealthBar` は伸長するFlex要素でありながら、HP残量を `width` で設定している。残量表示が意図どおり縮むか、実機表示での確認とレイアウト指定の整理が必要である。

## UIが存在しないScene

次のSceneには `UIDocument`、UGUIの `Canvas`、TextMesh Pro、IMGUIのいずれの画面UIもない。

- `Assets/title.unity`
- `Assets/start.unity`
- `Assets/game_over.unity`
- `Assets/result.unity`
- `Assets/koh.unity`
- `Assets/main.unity`
- `Assets/Scenes/SampleScene.unity`

なお、Build Settingsで有効なのは `Assets/coconMain.unity` のみである。

## 実装参照先

- UI全体: `Assets/UIs/GameUI.uxml`
- アイテム表示: `Assets/UIs/ItemUI.uxml`
- 店表示: `Assets/UIs/ShopUI.uxml`
- 購入結果: `Assets/UIs/ItemResultUI.uxml`
- 表示制御: `Assets/Scripts/UIHandler.cs`
- 王の画面外判定: `Assets/Scripts/KingOffscreenIndicator.cs`
- 飛来物警告: `Assets/Scripts/ProjectileManager.cs`
- 進行度更新: `Assets/Scripts/KingController.cs`
- HP更新: `Assets/Scripts/KingHealth.cs`
- 商品データ: `Assets/Data/ItemCatalog.asset`
- UIスケーリング: `Assets/UI Toolkit/PanelSettings.asset`
- UI配置Scene: `Assets/coconMain.unity`
- ビルド対象: `ProjectSettings/EditorBuildSettings.asset`
