# AI画像解析アプリケーション

このアプリケーションは、フォルダ内の画像ファイルを再帰的に検索し、ローカルAI（Ollama + LLaVA）を使用して各画像の内容を解析・説明するWindowsデスクトップアプリケーションです。

## 機能

- **フォルダ選択**: 解析対象のフォルダを選択
- **再帰的検索**: 指定フォルダ内のサブフォルダも含めて画像ファイルを検索
- **ローカルAI画像解析**: Ollama + LLaVAを使用して画像の内容を詳細に解析
- **結果表示**: 画像と解析結果を並べて表示
- **プログレス表示**: 解析の進行状況を表示
- **オフライン動作**: インターネット接続不要でローカルで動作

## 対応画像形式

- JPEG (.jpg, .jpeg)
- PNG (.png)
- BMP (.bmp)
- GIF (.gif)
- TIFF (.tiff, .tif)

## 必要な環境

- .NET 6.0以上
- Windows 10以上
- Ollama（ローカルLLMランタイム）
- LLaVA（画像解析対応AIモデル）

## セットアップ

1. **Ollamaのインストール**:
   - https://ollama.ai/ からOllamaをダウンロード・インストール
   - コマンドプロンプトまたはPowerShellで以下を実行:
   ```
   ollama pull llava
   ```

2. **Ollamaの起動**:
   ```
   ollama serve
   ```
   (通常は自動的に起動しますが、手動で起動する場合)

3. **プロジェクトのビルド**:
   ```
   dotnet build
   ```

4. **実行**:
   ```
   dotnet run
   ```

## 使用方法

1. Ollamaサーバーが起動していることを確認
2. アプリケーションを起動（接続状態が自動確認されます）
3. "フォルダを選択" ボタンをクリックして、解析したい画像が含まれるフォルダを選択
4. "解析開始" ボタンをクリックして解析を開始
5. 解析が完了すると、左側のリストに画像と解析結果が表示されます
6. リストの項目をクリックすると、右側に詳細な画像プレビューと解析結果が表示されます

## 推奨モデル

- **LLaVA**: 画像解析用の主要モデル（デフォルト）
- **LLaVA-1.6**: より高精度な画像解析（より多くのリソースが必要）

追加モデルのインストール:
```
ollama pull llava:13b
ollama pull llava:34b
```

## 注意事項

- **完全オフライン動作**: インターネット接続は不要
- **リソース使用量**: AIモデルは大量のRAMを使用します（最低8GB推奨）
- **処理時間**: 初回起動時やモデル変更時は時間がかかる場合があります
- **GPU加速**: NVIDIA GPUがあると処理が高速化されます

## トラブルシューティング

- **Ollama接続エラー**: 
  - Ollamaが起動しているか確認: `ollama list`
  - ポート11434が使用可能か確認
  - ファイアウォールの設定を確認

- **LLaVAモデルエラー**: 
  - モデルがインストールされているか確認: `ollama list`
  - 必要に応じて再インストール: `ollama pull llava`

- **画像読み込みエラー**: 
  - 画像ファイルが破損していないか確認
  - サポートされている画像形式か確認

- **メモリ不足**: 
  - 他のアプリケーションを終了してメモリを確保
  - より軽量なモデルを使用（llava:7b等）

## システム要件

- **RAM**: 8GB以上推奨（16GB以上を強く推奨）
- **ストレージ**: 最低5GB（モデルファイル用）
- **CPU**: 64bit プロセッサ
- **GPU**: NVIDIA GPU（オプション、処理高速化）