![img_0418](https://cloud.githubusercontent.com/assets/17020011/25613994/26f08742-2f6b-11e7-9659-699e7fb1a87a.JPG)
【作品名】デジタルかりんちゃん

【ジャンル】バーチャル店員さんシステム

【目的】淡路島にある薫寿堂の線香工場を見学に訪れる子供連れの家族が、楽しめるようにする

【担当箇所】

　・Unity(ソフト部分)のプログラム全て  
　・CeVIO(さとうささら)を利用したボイスファイルの作成全て  
　・テキスト(CSVファイル)からテキスト画像(PNG)を作成するためのツールの作成  
　・テキスト画像の作成全て

【使用ツール】

　・Unity5.3.2f1  
　・Live2D  
　・CeVIO Creative Studio S  
　・Processing3.2.1  
 
 【フォルダについて】
 
　・LipSync－Unityのプロジェクト  
　・テキスト画像生成ツール－CSVファイルからテキスト画像を生成ツールをまとめたフォルダ  
　・DigitalKarin-exeFolder.zip－Windows用に書き出した実行ファイル(ファイル容量の関係上圧縮)  

【自分が書いたソースコード】

　LipSync/Assets/Scripts2内にあるCsvReader.csを除くソースコード  
　テキスト画像生成ツールフォルダ内にある各テキスト毎に最適化したProcessingのソースコード

【概要】

　下記の3機能を搭載  
　・ぴったり商品おすすめ※3択の質問に3回答えるとおすすめの線香を教えてくれる  
　・淡路島・お香に関するクイズ  
　・2種類のミニゲーム※3回勝負のじゃんけんとカードのシャッフルゲームの2種

　筐体に取り付けた4つのボタンから信号をキー入力として受け取っている。(A＝赤、S＝黄、D＝青、Esc＝緑※店員が強制的にタイトルに戻す際使用(5秒長押し))  
　キャラクターの表示にはチャレンジとしてLive2Dを使用。またもう1つのチャレンジとしてキャラクターの声にCeVIOのさとうささらを使用している。  
　権利の問題があり、音声・テキスト共にあらかじめファイルとして出力したものをUnityで再生・表示している
 
【最も苦労・工夫した点】

●ボイス・テキストファイルの読み込み方法  
　読み込むファイル数が膨大になったこと、納品後におすすめする商品の変更の要望があることなどから、変更と管理をしやすくする必要があった。そのため、あらかじめ参照するファイルをUnityのエディタ上で設定する方法ではなく、実行ファイル起動中にそれぞれResources・StreamingAssetsから読み込みを行うようにした。  
　また、初めはシーン遷移時に読み込む方法であったが、遷移の度に20秒から30秒の待ち時間が発生してしまい快適な動作とはいえなかった。作品利用時のことを考えると、起動の素早さよりも、遷移のスムーズさを優先させる必要があった。そのため起動後のTOPメニューに入る前に、遷移しても情報が保持されるオブジェクトにファイルを読み込ませ、そのオブジェクトからファイルを利用する方法を考え、適用した。結果、TOPメニューまでの起動に読み込み時間が発生する代わりに、遷移時の読み込み時間を2秒から5秒程度に抑える事に成功した。
