ScrollEx
===

## Overview

ScrollEx




## Requirement

* Unity5.5+ *(included Unity 2017.x)*
* No other SDK




## Usage

1. Download [ScrollEx.unitypackage](https://github.com/mob-sakai/ScrollEx/raw/master/ScrollEx.unitypackage) and install to your project.
1. Select `GameObject > UI > Scroll Rect Ex (Vertical)` to create scroll view.
1. Implement ScrollCellView and IScrollViewController.
1. Create prefabs for ScrollCellView.
1. Enjoy!




### 機能一覧

ScrollRectExは、EnhancedScrollerにインスパイアされたスクロール拡張機能で、以下の機能をサポートします。

* MVCパターン
* オブジェクトプール
* ループ（Controller利用時のみ：後述）
* インデックスジャンプ
* 複数のセルビューを組み合わせたスクロール
* 実行時に大きさを決定するセルビュー
* 【New!】ドラッグ終了時、最も近いセルにスナップ
* 【New!】スワイプ時、前後のセルにジャンプ
* 【New!】前後にジャンプするボタン
* 【New!】現在のスクロールインデックスを示すインジゲータ
* 【New!】自動送り

使い方については、ほとんどEnhancedScrollerと変わりません。http://www.echo17.com/enhancedscroller.html

1. データ作って
1. スクロールビュー作って
1. セルビュー作って
1. コントローラ作って
1. コントローラにデータ渡す

既に作り込んだスクロールビューに対して、スナップやインジケータ等の機能を付加したい場合は、ScrollRectExを追加するだけでOKです。






## Release Notes

### ver.0.2.0:

* Supports Unity 5.5+
* Fixed: ContentSizeFitter issue
* Feature: Buttons to jump to first/last index.

### ver.0.1.0:

* MVCパターン
* オブジェクトプール
* ループ（Controller利用時のみ：後述）
* インデックスジャンプ
* 複数のセルビューを組み合わせたスクロール
* 実行時に大きさを決定するセルビュー
* 【New!】ドラッグ終了時、最も近いセルにスナップ
* 【New!】スワイプ時、前後のセルにジャンプ
* 【New!】前後にジャンプするボタン
* 【New!】現在のスクロールインデックスを示すインジゲータ
* 【New!】自動送り




## License
MIT



## Author
[mob-sakai](https://github.com/mob-sakai)




## See Also

* GitHub Page : https://github.com/mob-sakai/UITransition
* Issue tracker : https://github.com/mob-sakai/UITransition/issues