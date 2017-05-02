//フォントを画像化
String savetext;//保存するpngの名前を保存する変数
String text0, text1, text2, text3, text4;//画面出力用
PImage img;//保存用
PImage img2;//保存用

BufferedReader reader;
String line; 
String lines[]; 
int num;
PFont font2;//フォント
PFont font_;//フォント
PFont font;

//設定
/*
color   text_c      =color(255, 255, 255);      //文字色
color   background_c=color(0, 0, 0);       //背景色
*/
//黒バック白文字

color   text_c      =color(255,246,195);      //文字色
//color   background_c=color(255,255,255);//背景色(透過用)
color   background_c=color(89,73,59);       //背景色

//白バック黒文字
//color   text_c      =color(40,65,39);      //文字色
//color   background_c=color(255,255,255);       //背景色
//color   background_c=color(243,225,203);       //背景色

//255,87,176
String  font_name   ="DFHSMaruGothicStdN-W4.otf"; //使用するフォントの名前
int     font_size   =65;                      //使用するフォントのサイズ
String  csv_name    ="Karin_text_system.csv";//読み込ませるcsvファイルの名前
//String  csv_name    ="number.csv";//読み込ませるcsvファイルの名前
//透過する際はAlpha_blendをtrue、Anti_aliasingをfalse、背景色を白にして実行
boolean Alpha_blend =true;                   //pngの透過処理を行うかどうか
boolean Anti_aliasing=true;                  //アンチエイリアスを残す(単色でないとうまく機能しない可能性あり)

String SerialNumber_name="Text";  //連番保存する際の名前
int Size_X=910, Size_Y=260;        //保存するpngファイルのサイズ(縦×横)
//設定ここまで

void setup() {
  num=0;
  size(910, 310); 
  //size(260,65);
  frameRate(10);//表示する速度
  colorMode(RGB, 255, 255, 255, 100);
  font_= createFont(font_name, font_size);//読み込むフォントとサイズ
  textFont(font_);
  textAlign(LEFT, TOP);//テキストを横左揃え、縦中央揃え
  // CSVの中身を読み込む
  lines = loadStrings(csv_name);
}

void draw() {
  // 読み込むデータが無いならループを止める
  if (num==0) println("変換中...");
  /*1行を3分割*/
  if (num < lines.length) {
    String list[] = split(lines[num], ',');//カンマで区切る
    if(list.length>=1){
      text0=list[0];
    }
    if(list.length>=2){
      text1=list[1];
    }
    if(list.length>=3){
      text2=list[2];
    }
    if(list.length>=4){
      text3=list[3];
    }
    if(list.length>=5){
      text4=list[4];
    }
    textLeading(60);//テキストの行間
    background(background_c);
    fill(text_c);

    int tyousei=5;
    int tyousei2=12;//行間の調整
    if(list.length>=2){
      text(text1, 0, 0+tyousei);//保存するテキストを画面に表示する(表示されたテキストが保存される)
    }
    if(list.length>=3){
      text(text2, 0, font_size*1+tyousei2*1+tyousei);//保存するテキストを画面に表示する(表示されたテキストが保存される)
    }
    if(list.length>=4){
      text(text3, 0, font_size*2+tyousei2*2+tyousei);//保存するテキストを画面に表示する(表示されたテキストが保存される)
    }
    if(list.length>=5){
      text(text4, 0, font_size*3+tyousei2*3+tyousei);//保存するテキストを画面に表示する(表示されたテキストが保存される)
    }
    
    
    
    savetext=text0;
    saved(savetext);//string型を入れればその文字列がファイル名になりint型を入れれば連番保存が行われる
  } else {
    noLoop();
    println("保存完了");
  }
  num++;
}





//縦書きに変更
String convert(String str) {
  String convertStr="";
  for (int i=0; i<str.length (); i++) {
    if (str.substring (i, i+1).equals("ー")) {//ーのとき空白にする
      convertStr += "|"+"\n";
    } else {
      convertStr += str.substring (i, i+1)+"\n";
    }
  }
  return convertStr;
}

//ーのとき｜にする
String convertBack(String str) {
  String convertStr="";
  for (int i=0; i<str.length (); i++) {
    if (str.substring (i, i+1).equals("ー")) {//ーのとき｜にする
      convertStr += "|"+"\n";
    } else {
      convertStr += " "+"\n";
    }
  }
  return convertStr;
}

//連番保存とか
/*void saved(int num) {
  save(SerialNumber_name+num+".png"); //透過なしで保存
  if (Alpha_blend==true) {
    img = loadImage("Text"+num+".png");//透過なしのものを読み込み
    img2 = createImage(width, height, ARGB);//上書き用
    for (int y=0; y<img2.height; y++) {
      for (int x=0; x<img2.width; x++) {
        if (img.pixels[y*img.width+x] == background_c) {//ピクセルが白だったら透明化
          img2.pixels[y*img2.width+x] = color(0, 0, 0, 0);
        } else if (img.pixels[y*img.width+x] == text_c) {
          img2.pixels[y*img2.width+x] = img.pixels[y*img.width+x];
        } else if (Anti_aliasing==true) {
          color text_alpha=img.pixels[y*img.width+x];
          img2.pixels[y*img2.width+x] = color(255, 255, 255, red(text_alpha));//Redの値をアルファ値に変換しているため赤を含まない色には効果がない
        } else {
          img2.pixels[y*img2.width+x] = color(0, 0, 0, 0);
        }
      }
    }
    img2.save(SerialNumber_name+num+".png"); //保存（上書き）
  }
}*/

//文字列で保存
void saved(String num) {
  save(num+".png"); //透過なし保存
  if (Alpha_blend==true) {
    img = loadImage(num+".png");//透過なしのものを読み込み
    img2 = createImage(width, height, ARGB);//上書き用
    for (int y=0; y<img2.height; y++) {
      for (int x=0; x<img2.width; x++) {
        if (img.pixels[y*img.width+x] == background_c) {//ピクセルが白だったら透明化
          img2.pixels[y*img2.width+x] = color(0, 0, 0, 0);
        }else{
          img2.pixels[y*img2.width+x] = img.pixels[y*img.width+x];
        }
        /*
        else if (img.pixels[y*img.width+x] == text_c) {
          img2.pixels[y*img2.width+x] = img.pixels[y*img.width+x];
        } /*else if (Anti_aliasing==true) {
          color text_alpha=img.pixels[y*img.width+x];
          img2.pixels[y*img2.width+x] = color(255, 255, 255, red(text_alpha));//Redの値をアルファ値に変換しているため赤を含まない色には効果がない
        } else {
          img2.pixels[y*img2.width+x] = color(0, 0, 0, 0);
        }*/
      }
    }
    img2.save(num+".png"); //保存（上書き）
  }
}