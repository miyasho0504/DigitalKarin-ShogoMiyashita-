using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using System;//add
using System.IO;//add
using live2d;//add
using live2d.framework;

public class quizScript2 : MonoBehaviour
{
    #region 変数宣言
    int step = 0;
    float time = 0;
    float escTime = 0;//Escキー(メインへ強制遷移)
    float resetTime = 0;
    bool once = false;
    bool once2 = false;
    bool once3 = false;
    bool once4 = false;
    bool lipsync = true;//リップシンクを行うかどうか
    int ansNum = 0;         //選択した解答の番号
    int questionNum = 0;    //クイズ番号
    int ansCount = 0;       //現在が何問目か
    int ansSeikai_num = 0;  //正解した数
    int questionMAX = 27;//問題の合計数

    //bool debug_mode = false;//デバッグモードにするか

    int Q1=-1;//1問目に出題された問題の番号
    int Q2=-1;//2問目に出題された問題の番号

    int[] zyanru ={0,1,1,1,1,2,2,2,2,3,3,3,3,4,4,5,6,6,6,7,7,7,8,8,8,8,9,9};

    public int debug_questionNum=0;

    LogOutput _LogO = new LogOutput();
 
    //正解・不正解SE
    public AudioClip se_pinpon_;
    public AudioClip se_bubu_;

    //ゲームオブジェクト
    public GameObject GOquestion_msbox_;

    public GameObject GOquestion_red_;
    public GameObject GOquestion_yellow_;
    public GameObject GOquestion_blue_;

    public GameObject GOquizBeforeInfo_msbox_;
    public GameObject GOquizBeforeInfo_button_;

    public GameObject GOseikai_circle_;
    public GameObject GOhuseikai_circle_;
    public GameObject GOunselected_panel1_;
    public GameObject GOunselected_panel2_;
    public GameObject GOunselected_panel3_;

    #region ボイス用変数
    private AudioSource audioSource_;
    private AudioClip[] voice_quiz_basic_;
    private AudioClip[] voice_quiz_beforeinfo_;
    private AudioClip[] voice_quiz_question_;
    private AudioClip[] voice_quiz_info_;
    #endregion

    #region CSV
    private class QuestionData
    {
        [CsvColumnAttribute(0, 0)]
        private int no;

        [CsvColumnAttribute(1, 0)]
        private int ans1;

        [CsvColumnAttribute(2, 0)]
        private int ans2;

        [CsvColumnAttribute(3, 0)]
        private int ans3;

        [CsvColumnAttribute(4, 0)]
        private int genre;//ジャンル

        public override string ToString()
        {
            return string.Format("No={0} ans1={1}, ans2={2}, ans3={3},genre={4}", no, ans1, ans2, ans3,genre);
        }

    }
    #endregion

    #region 画像ファイル

    //システムメッセージ画像
    Sprite[] quizSystemSprites = new Sprite[14];
    //private SpriteRenderer quizBeforeInfoSpriteRenderer_;
    //前説文画像
    public GameObject quizBeforeInfoTextGO_;
    Sprite[] quizBeforeInfoSprites = new Sprite[31];
    private SpriteRenderer quizBeforeInfoSpriteRenderer_;
    //問題文画像
    public GameObject questionTextGO_;
    Sprite[] questionTextSprites = new Sprite[31];
    private SpriteRenderer questionTextSpriteRenderer_;
    //選択肢画像
    public GameObject questionAnsTextGO1_;
    public GameObject questionAnsTextGO2_;
    public GameObject questionAnsTextGO3_;
    Sprite[] questionAnsTextSprites = new Sprite[91];
    private SpriteRenderer questionAnsTextSpriteRenderer1_;
    private SpriteRenderer questionAnsTextSpriteRenderer2_;
    private SpriteRenderer questionAnsTextSpriteRenderer3_;
    //解説文画像
    Sprite[] quizInfoSprites = new Sprite[31];

    #endregion

    #region Live2D用変数
    private Live2DModelUnity live2DModel;
    private EyeBlinkMotion eyeBlink;
    private Live2DMotion[] motion;
    private L2DExpressionMotion[] expression;
    private MotionQueueManager motionMgr;
    private MotionQueueManager expressionMgr;

    private Matrix4x4 live2DCanvasPos;

    //リップシンク用変数
    private float scaleVolume = 30; //音声の倍率（後で実値にかけて使う）
    private bool smoothing = false;  //スムーシング処理を行うかどうか
    private float lastVolume = 0;   //?

    //目パチ用変数
    private int blink_interval = 3500;//インターバルの間隔単位(ミリ秒)

    public TextAsset mocFile;
    public TextAsset modelJson;
    public Texture2D[] textureFiles;
    public TextAsset[] motionFiles;
    public TextAsset physicsFile;
    public TextAsset poseFile;
    public TextAsset[] expressionFiles;
    //public bool stop = false;//キーが押されるとストップする
    private L2DPhysics physics;
    private L2DPose pose;
    #endregion
    #endregion 変数宣言終了
    // Use this for initialization
	void Start () {
        resetTime = 0;
        live2d_setup();

        //debug_mode = false;

        //CSV
        //readerをList<TestEnemyData>化して、それぞれ表示
        using (var reader = new CSVReader<QuestionData>("Texts/questionData", true))
        {
            reader.ToList().ForEach(question => Debug.Log(question.ToString()));
        }

        //リソースを管理しているオブジェクトを呼び出す
        GameObject go = GameObject.Find("ResourceManager");
        resourceManageScript resScript = go.GetComponent<resourceManageScript>();
        #region　ボイス読み込み
        audioSource_ = gameObject.GetComponent<AudioSource>();
        //基本ボイスの読み込み
        voice_quiz_basic_ = resScript.voice_quiz_basic_;
        //前説ボイスの読み込み
        voice_quiz_beforeinfo_ = resScript.voice_quiz_beforeinfo_;
        //問題ボイスの読み込み
        voice_quiz_question_ = resScript.voice_quiz_question_;
        //解説ボイスの読み込み
        voice_quiz_info_ = resScript.voice_quiz_info_;
        #region 旧読み込み方法
        /*
        //基本ボイスの読み込み
        voice_quiz_basic_ = new AudioClip[21];
        for (int i = 0; i < voice_quiz_basic_.Length; i++)
        {
            voice_quiz_basic_[i] = Resources.Load<AudioClip>(voiceFolderNameBasic + "quiz_basic"+i);
        }
        //前説ボイスの読み込み
        voice_quiz_beforeinfo_ = new AudioClip[28];
        for (int i = 1; i < voice_quiz_beforeinfo_.Length; i++)
        {
            voice_quiz_beforeinfo_[i] = Resources.Load<AudioClip>(voiceFolderNameQuizBeforeInfo + "beforeinfo" + i);
        }
        //問題ボイスの読み込み
        voice_quiz_question_ = new AudioClip[28];
        for (int i = 0; i < voice_quiz_question_.Length; i++)
        {
            voice_quiz_question_[i] = Resources.Load<AudioClip>(voiceFolderNameQuestion + "question" + i);
        }
        //解説ボイスの読み込み
        voice_quiz_info_ = new AudioClip[28];
        for (int i = 0; i < voice_quiz_info_.Length; i++)
        {
            voice_quiz_info_[i] = Resources.Load<AudioClip>(voiceFolderNameQuizInfo + "info" + i);
        }

        /*
        for(int i=0;i<basicVoiceName.Length;i++){
            string url = Application.streamingAssetsPath + "/voices/products/basic/" + basicVoiceName[i];
            WWW www = new WWW(url);
            voice_product_basic_[i] = www.audioClip;
        }
        
        //string url ="file:///"+ Application.streamingAssetsPath + "/voices/products/basic/product-top.wav";
        string url = "file:///" + "C:/Users/1423074/Documents/Awazishima/Unity_Projects/LipSync/Assets/StreamingAssets/voices/products/basic/product-top.wav";
        WWW www = new WWW(url);
        audioSource_.clip = www.audioClip;
        */
        #endregion
        #endregion

        #region 画像読み込み

        //システムメッセージ画像
        quizSystemSprites = resScript.quizSystemSprites;
        //前説文画像
        quizBeforeInfoSpriteRenderer_ = quizBeforeInfoTextGO_.GetComponent<SpriteRenderer>();
        quizBeforeInfoSprites = resScript.quizBeforeInfoSprites;
        //問題文画像
        questionTextSpriteRenderer_ = questionTextGO_.GetComponent<SpriteRenderer>();
        questionTextSprites = resScript.questionTextSprites;
        //選択肢
        questionAnsTextSpriteRenderer1_ = questionAnsTextGO1_.GetComponent<SpriteRenderer>();
        questionAnsTextSpriteRenderer2_ = questionAnsTextGO2_.GetComponent<SpriteRenderer>();
        questionAnsTextSpriteRenderer3_ = questionAnsTextGO3_.GetComponent<SpriteRenderer>();
        questionAnsTextSprites = resScript.questionAnsTextSprites;
        //解説文画像
        quizInfoSprites = resScript.quizInfoSprites;

        #region 旧読み込み方法
        /*
        Texture2D texture_;
        //システムメッセージ画像
        for (int i = 0; i <= 13; i++)
        {
            string filename = Application.streamingAssetsPath + "/sprites/quizSystem/quizT" + i + ".png";
            if (ReadPng(filename) != null)
            {
                texture_ = ReadPng(filename);
                quizSystemSprites[i] = Sprite.Create(texture_, new Rect(0, 0, 910, 310), Vector2.zero);
            }
            else
            {
                //NoImage用画像にすりかえ
                quizSystemSprites[i] = null;
            }
            Debug.Log(quizSystemSprites[i]);
        }
        
        //前説文画像
        quizBeforeInfoSpriteRenderer_ = quizBeforeInfoTextGO_.GetComponent<SpriteRenderer>();
        for (int i = 0; i <= 30; i++)
        {
            string filename = Application.streamingAssetsPath + "/sprites/quizBefore/quiz_before" + i + ".png";
            if (ReadPng(filename) != null)
            {
                texture_ = ReadPng(filename);
                quizBeforeInfoSprites[i] = Sprite.Create(texture_, new Rect(0, 0, 940, 620), Vector2.zero);
            }
            else
            {
                //NoImage用画像にすりかえ
                quizBeforeInfoSprites[i] = null;
            }
            Debug.Log(quizBeforeInfoSprites[i]);
        }
        //問題文画像
        questionTextSpriteRenderer_ = questionTextGO_.GetComponent<SpriteRenderer>();
        for (int i = 0; i <= 30; i++)
        {
            string filename = Application.streamingAssetsPath + "/sprites/quizQuestion/question" + i + ".png";
            if (ReadPng(filename) != null)
            {
                texture_ = ReadPng(filename);
                questionTextSprites[i] = Sprite.Create(texture_, new Rect(0, 0, 930, 310), Vector2.zero);
            }
            else
            {
                //NoImage用画像にすりかえ
                questionTextSprites[i] = null;
            }

        }
        //選択肢
        questionAnsTextSpriteRenderer1_ = questionAnsTextGO1_.GetComponent<SpriteRenderer>();
        questionAnsTextSpriteRenderer2_ = questionAnsTextGO2_.GetComponent<SpriteRenderer>();
        questionAnsTextSpriteRenderer3_ = questionAnsTextGO3_.GetComponent<SpriteRenderer>();
        for (int i = 0; i < 90; i++)
        {
            string filename = Application.streamingAssetsPath + "/sprites/quizQuestionAns/questionAns" + i + ".png";
            if (ReadPng(filename) != null)
            {
                texture_ = ReadPng(filename);
                questionAnsTextSprites[i] = Sprite.Create(texture_, new Rect(0, 0, 780, 60), Vector2.zero);
            }
            else
            {
                //NoImage用画像にすりかえ
                questionAnsTextSprites[i] = null;
            }

        }

        for (int i = 0; i <= 30; i++)
        {
            string filename = Application.streamingAssetsPath + "/sprites/quizInfo/quiz_info" + i + ".png";
            if (ReadPng(filename) != null)
            {
                texture_ = ReadPng(filename);
                quizInfoSprites[i] = Sprite.Create(texture_, new Rect(0, 0, 930, 310), Vector2.zero);
            }
            else
            {
                //NoImage用画像にすりかえ
                quizInfoSprites[i] = null;
            }
        }
         */ 
        #endregion 旧読み込み方法終わり
        #endregion

        quizInfoMs(false);
        quizObject(false);
        quizRYB(false);
        circle_hyouzi(false, 1);
        cross_hyouzi(false, 1);
        unselected_hyouzi(false, 1);
	}
	
	// Update is called once per frame
	void Update () {
        live2d_update1();
        #region メインへの強制遷移処理
        if(Input.GetKey(KeyCode.Escape)){
            escTime += Time.deltaTime;
            Debug.Log(escTime);
            if(escTime>5.0f){
                SceneManager.LoadScene("main");
            }
        }else{
            escTime=0.0f;
        }
        #endregion

        #region メインへの強制遷移処理(T.O)
        resetTime += Time.deltaTime;
        Debug.Log("resetTime=" + resetTime);
        if (resetTime > 300)
        {
            resetTime = 0;
            SceneManager.LoadScene("main");
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Escape))
        {
            resetTime = 0;
        }
        #endregion
        /*
        //デバッグモードではEnterキーを押すと読み上げをキャンセルできるようにする
        if(debug_mode==true){
            if (Input.GetKeyDown(KeyCode.Return))
            {
                audioSource_.Stop();
            }
        }*/
        #region TOP説明
        if (step == 0)
        {
            /*
            #region デバッグモード切替
            if (debug_mode == false)
            {
                if(Input.GetKeyDown(KeyCode.Space)){
                    debug_mode = true;
                    //questionNum = 15;
                    questionNum = debug_questionNum;
                }
            }
            #endregion
             */ 
            quizInfoMs(false);
            quizObject(false);
            quizRYB(false);
            motionPlayLoop(1);
            if (once == false)
            {
                Debug.Log("TOP説明1");
                //Debug.Log(voice_product_basic_[0]);
                audioSource_.PlayOneShot(voice_quiz_basic_[0]);

                Debug.Log("で次のステップへ");
                once = true;
                ansCount = 0;
            }
            //T.Oで次のステップへ
            if (audioSource_.isPlaying == false)
            {
                Debug.Log("TOP説明2");
                time = 0;
                step = 1;
                once = false;
            }
            /*
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                audioSource_.Stop();
                step = 1;
                time = 0;
                once = false;
            }*/
        }
        #endregion

        #region クイズ前説
        else if (step == 1)
        {
            motionPlayLoop(0);
            if (once == false)
            {
                Debug.Log("クイズ前説1");
                quizInfoMs(false);
                quizObject(false);
                quizRYB(false);
                circle_hyouzi(false, ansNum);
                cross_hyouzi(false, ansNum);
                unselected_hyouzi(false, ansNum);
                questionTextSpriteRenderer_.sprite = null;
                //クイズ番号を決める(既に1問目か2問目に出題した問題であればもう一度抽選する)
                while (true)
                {
                    questionNum = UnityEngine.Random.Range(1, 28);
                    Debug.Log(questionNum);
                    if (Q1 != questionNum)
                    {
                        if (Q2 != questionNum)
                        {
                            break;
                        }
                    }
                }
                /*
                if (debug_mode == false)
                {

                }
                else
                {
                    questionNum++;
                }
                 */ 
                //「それでは○問目」のボイスを再生
                if (ansCount == 0)
                {
                    //Debug.Log("Q1");
                    audioSource_.PlayOneShot(voice_quiz_basic_[1]);
                    Q1 = questionNum;
                }
                else if (ansCount == 1)
                {
                    //Debug.Log("Q2");
                    audioSource_.PlayOneShot(voice_quiz_basic_[2]);
                    Q2 = questionNum;
                }
                else if (ansCount == 2)
                {
                    //Debug.Log("Q3");
                    audioSource_.PlayOneShot(voice_quiz_basic_[3]);
                }
                once = true;
            }
            else if (once2 == false && audioSource_.isPlaying == false)
            {
                Debug.Log("クイズ前説2");
                quizObject(true);
                questionTextSpriteRenderer_.sprite=quizSystemSprites[zyanru[questionNum]];
                //「○○(ジャンル)からの問題です」ボイスを再生(※ジャンルを決定するint型配列から再生するボイスを決定)
                audioSource_.PlayOneShot(voice_quiz_basic_[zyanru[questionNum]+11]);
                once2 = true;
            }
            else if (once3 == false && audioSource_.isPlaying == false)
            {
                Debug.Log("クイズ前説3");
                quizObject(false);
                quizInfoMs(true);
                //現在のクイズの前説のスプライトを表示する
                quizBeforeInfoSpriteRenderer_.sprite = quizBeforeInfoSprites[questionNum];
                //Debug.Log(quizBeforeInfoSprites[1]);
                //「(前説文)」ボイスを再生
                Debug.Log(voice_quiz_beforeinfo_[questionNum]);
                audioSource_.PlayOneShot(voice_quiz_beforeinfo_[questionNum]);
                once3 = true;
            }
            else if (once4 == false && audioSource_.isPlaying == false)
            {
                Debug.Log("クイズ前説4");
                //「それでは」ボイス再生
                audioSource_.PlayOneShot(voice_quiz_basic_[4]);
                once4 = true;
            }
            else if (audioSource_.isPlaying == false)
            {
                Debug.Log("クイズ前説5");
                once = false;
                once2 = false;
                once3 = false;
                once4 = false;
                step++;
            }
            /*
            if(once3==true&&audioSource_.isPlaying==true){
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    audioSource_.Stop();
                }
            }*/
        }
        #endregion

        #region クイズ出題&回答
        else if (step == 2)
        {
            
            if (once == false)
            {
                Debug.Log("クイズ出題1");
                quizInfoMs(false);
                quizObject(true);
                //現在のクイズ番号のスプライトを表示する
                questionTextSpriteRenderer_.sprite = questionTextSprites[questionNum];
                questionAnsTextSpriteRenderer1_.sprite = questionAnsTextSprites[questionNum * 3 - 2];
                questionAnsTextSpriteRenderer2_.sprite = questionAnsTextSprites[questionNum * 3 - 1];
                questionAnsTextSpriteRenderer3_.sprite = questionAnsTextSprites[questionNum * 3];
                //「(問題文)」ボイスを再生
                Debug.Log("QNum=" + questionNum);
                audioSource_.PlayOneShot(voice_quiz_question_[questionNum]);
                once = true;
            }
            else if (audioSource_.isPlaying == false)
            {
                //Debug.Log("クイズ出題2");
                motionPlayLoop(1);
                quizRYB(true);
                if (Input.GetKeyDown(KeyCode.A))
                {
                    audioSource_.Stop();
                    //ansCount++;
                    ansNum = 1;
                    step=3;
                    time = 0;
                    once = false;
                    once2 = false;
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    audioSource_.Stop();
                    //ansCount++;
                    ansNum = 2;
                    step=3;
                    time = 0;
                    once = false;
                    once2 = false;
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    audioSource_.Stop();
                    //ansCount++;
                    ansNum = 3;
                    step=3;
                    time = 0;
                    once = false;
                    once2 = false;
                }
                
            }
            motionPlayLoop(0);
        }
        #endregion

        #region Aモード
        else if (step == 3)
        {
            //Debug.Log("Aモード");
            motionPlayLoop(0);
            
            if (once == false)
            {
                ansCount++;
                Debug.Log("ansnum="+ansNum);
                unselected_hyouzi(true, ansNum);
                Debug.Log("Aモード１");
                
                //選択した解答番号が正しいかどうか
                int seikaiNum = questionNum % 3;
                Debug.Log("seikaiNum="+seikaiNum);
                if (seikaiNum == 0) seikaiNum = 3;
                //正解の○を表示
                circle_hyouzi(true, seikaiNum);

                //正解不正解のSEを再生
                //リップシンク設定をOFF
                lipsync = false;
                //ログに出力するための現在何問目か
                string _logmode = "QUIZ" + ansCount;
                //SE再生部
                if (ansNum == seikaiNum)
                {
                    //ログを出力
                    _LogO.LogOutputTextfile(_logmode, questionNum.ToString(), (ansNum - 1).ToString(), "1");

                    audioSource_.PlayOneShot(se_pinpon_);
                }
                else
                {
                    //ログを出力
                    _LogO.LogOutputTextfile(_logmode, questionNum.ToString(), (ansNum - 1).ToString(), "0");

                    audioSource_.PlayOneShot(se_bubu_);
                    //不正解の×を表示
                    cross_hyouzi(true, ansNum);
                }
                once = true;
            }
            else if (once2 == false && audioSource_.isPlaying == false)
            {
                //選択した解答番号が正しいかどうか
                int seikaiNum = questionNum % 3;
                Debug.Log("seikaiNum="+seikaiNum);
                if (seikaiNum == 0) seikaiNum = 3;
                //リップシンク設定をON
                lipsync = true;
                if (ansNum == seikaiNum)
                {
                    motionPlayOnce_stop(3);
                    //正解の場合
                    //「正解です！」ボイス再生
                    audioSource_.PlayOneShot(voice_quiz_basic_[5]);
                    ansSeikai_num++;//正解数を加算

                }
                else
                {
                    motionPlayOnce_stop(4);
                    //不正解の場合
                    //「うーん、残念」ボイス再生
                    audioSource_.PlayOneShot(voice_quiz_basic_[6]);
                }
                once2 = true;
            }
            else if (once3 == false && audioSource_.isPlaying == false)
            {
                Debug.Log("Aモード2");
                //「解説文」ボイス再生
                audioSource_.PlayOneShot(voice_quiz_info_[questionNum]);
                //解説文画像に変える
                questionTextSpriteRenderer_.sprite = quizInfoSprites[questionNum];
                once3 = true; Debug.Log("Aモード2→3");
            }
            else if (audioSource_.isPlaying == false)
            {
                if (ansCount >= 3)
                {
                    Debug.Log("結果発表へ");
                    step = 4;//結果発表へ
                }
                else if (ansCount == 2)
                {
                    Debug.Log("もう１問");
                    step = 1;//3問やるまで前説に戻る
                }
                else if (ansCount == 1)
                {
                    Debug.Log("もう１問");
                    step = 1;//3問やるまで前説に戻る
                }
                Debug.Log("Aモード3");
                /*
                if (debug_mode == false)
                {
                    
                }
                else
                {
                    if (questionNum == questionMAX)
                    {
                        SceneManager.LoadScene("main");
                    }
                    else
                    {
                        Debug.Log("もう１問");
                        step = 1;//全問やるまで前説に戻る
                    }
                }*/
                once = false;
                once2 = false;
                once3 = false;
            }
        }
        #endregion
        #region 結果発表
        else if (step == 4)
        {
            if (once == false)
            {
                quizInfoMs(false);
                quizRYB(false);
                quizObject(true);
                circle_hyouzi(false,0);
                cross_hyouzi(false, 0);
                unselected_hyouzi(false, 1);
                questionTextSpriteRenderer_.sprite = null;

                Debug.Log("結果発表1");
                unselected_hyouzi(false, ansNum);
                circle_hyouzi(false, ansNum);
                //「それでは結果を発表します」ボイス再生
                audioSource_.PlayOneShot(voice_quiz_basic_[7]);
                once = true;
            }
            else if (once2 == false && audioSource_.isPlaying == false)
            {
                Debug.Log("結果発表2");
                //正答に応じてボイスを再生
                if (ansSeikai_num == 3)
                {
                    motionPlayOnce_stop(3);
                    //「全問正解～」ボイス再生
                    audioSource_.PlayOneShot(voice_quiz_basic_[8]);
                    //テキスト変更
                    questionTextSpriteRenderer_.sprite=quizSystemSprites[10];
                }
                else if (ansSeikai_num == 2)
                {
                    motionPlayOnce_stop(2);
                    //「3問中2問正解～」ボイス再生
                    audioSource_.PlayOneShot(voice_quiz_basic_[9]);
                    //テキスト変更
                    questionTextSpriteRenderer_.sprite = quizSystemSprites[11];
                }
                else if (ansSeikai_num == 1)
                {
                    motionPlayOnce_stop(2);
                    //「3問中1問正解～」ボイス再生
                    audioSource_.PlayOneShot(voice_quiz_basic_[10]);
                    //テキスト変更
                    questionTextSpriteRenderer_.sprite = quizSystemSprites[12];
                }
                else
                {
                    motionPlayOnce_stop(4);
                    //「正解は0問」ボイス再生
                    audioSource_.PlayOneShot(voice_quiz_basic_[11]);
                    //テキスト変更
                    questionTextSpriteRenderer_.sprite = quizSystemSprites[13];
                }
                once2 = true;
            }
            else if(audioSource_.isPlaying==false)
            {
                time += Time.deltaTime;
                if(time>3.0f){
                    SceneManager.LoadScene("main");
                }
            }
        }
        #endregion
        live2d_update2();
    }

    #region live2D用関数
    //start()で行う処理
    void live2d_setup()
    {
        #region Live2D用記述
        Live2D.init();

        //モデル設定ファイルの読み込み
        ModelSettingJson modelSettingJson = new ModelSettingJson(modelJson.text);

        //モデルのロード
        live2DModel = Live2DModelUnity.loadModel(mocFile.bytes);
        eyeBlink = new EyeBlinkMotion();

        //描画モードの設定(モデルのロード直後、テクスチャーのロード直前までに呼び出し)
        live2DModel.setRenderMode(Live2D.L2D_RENDER_DRAW_MESH);

        //テクスチャのロード
        for (int i = 0; i < textureFiles.Length; i++)
        {
            live2DModel.setTexture(i, textureFiles[i]);
        }

        float modelWidth = live2DModel.getCanvasWidth();
        live2DCanvasPos = Matrix4x4.Ortho(0, modelWidth, modelWidth, 0, -50.0f, 50.0f);

        motionMgr = new MotionQueueManager();
        motion = new Live2DMotion[motionFiles.Length];
        for (int i = 0; i < motionFiles.Length; i++)
        {
            //motion[i] = Live2DMotion.loadMotion(motionFiles[i].bytes);
            if (Live2DMotion.loadMotion(motionFiles[i].bytes) != null)
            {
                motion[i] = Live2DMotion.loadMotion(motionFiles[i].bytes);
                Debug.Log(i + "番目のモーションのロードに成功");
            }
            else
            {
                Debug.Log(i + "番目のモーションのロードに失敗");
            }
        }
        expressionMgr = new MotionQueueManager();
        expression = new L2DExpressionMotion[expressionFiles.Length];
        //表情ファイル読み込み
        for (int i = 0; i < expressionFiles.Length; i++)
        {
            //motion[i] = Live2DMotion.loadMotion(motionFiles[i].bytes);
            if (expressionFiles[i] != null)
            {
                expression[i] = L2DExpressionMotion.loadJson(expressionFiles[i].bytes);
                Debug.Log(i + "番目の表情モーションのロードに成功");
            }
            else
            {
                Debug.Log(i + "番目の表情モーションのロードに失敗");
            }
        }
        //物理演算設定ファイル読み込み
        if (physicsFile != null) physics = L2DPhysics.load(physicsFile.bytes);
        //ポーズ設定ファイルの読み込み
        if (poseFile != null) pose = L2DPose.load(poseFile.bytes);
        #endregion
    }
    //update()の最初に行う処理
    void live2d_update1()
    {
        eyeBlink.setInterval(blink_interval);
        if (live2DModel == null) return;
        live2DModel.setMatrix(transform.localToWorldMatrix * live2DCanvasPos);

        if (!Application.isPlaying)
        {
            eyeBlink.setParam(live2DModel);
            live2DModel.update();
            return;
        }
    }
    //update()の最後に行う処理
    void live2d_update2()
    {
        //表情の更新
        if (expressionMgr != null) expressionMgr.updateParam(live2DModel);//  表情でパラメータ更新（相対変化）
        #region リップシンク
        //初期化しておく
        float volume = 0;

        //スムーシング処理がONであれば
        if (smoothing)
        {
            float currentVolume = GetCurrentVolume(GetComponent<AudioSource>());

            if (Mathf.Abs(lastVolume - currentVolume) < 0.2f)
            {
                volume = lastVolume * 0.9f + currentVolume * 0.1f;
            }
            else if (lastVolume - currentVolume > 0.2f)
            {
                volume = lastVolume * 0.7f + currentVolume * 0.3f;
            }
            else
            {
                volume = lastVolume * 0.2f + currentVolume * 0.8f;
            }
            lastVolume = volume;
        }
        //スムーシング処理がOFFであれば
        else
        {
            volume = GetCurrentVolume(GetComponent<AudioSource>());
        }
        if (lipsync == true)
        {
            //Live2Dモデルの口のY座標を変更する(リップシンクを行う)
            live2DModel.setParamFloat("PARAM_MOUTH_FORM", 0);
            live2DModel.setParamFloat("PARAM_MOUTH_OPEN_Y", volume * scaleVolume);
        }
        #endregion

        //motionPlayLoop(5);

        motionMgr.updateParam(live2DModel);
        if (motionMgr.isFinished())
        {
            eyeBlink.setParam(live2DModel);
        }
        if (physics != null) physics.updateParam(live2DModel);
        if (pose != null) pose.updateParam(live2DModel);

        //モデルを描画する
        // 描画モードがL2D_RENDER_DRAW_MESHの場合
        if (live2DModel.getRenderMode() == Live2D.L2D_RENDER_DRAW_MESH) RenderModel();

        //Live2Dモデルをアップデート(必ず最後)
        live2DModel.update();
    }

    void OnRenderObject()
    {
        if (live2DModel == null) return;
        //描画モードがL2D_RENDER_DRAW_MESH_NOWの場合
        if (live2DModel.getRenderMode() == Live2D.L2D_RENDER_DRAW_MESH_NOW) RenderModel();
    }

    void RenderModel()
    {
        //Live2Dモデルの描画
        live2DModel.draw();
    }
    //音データを分析する
    private float GetCurrentVolume(AudioSource audio)
    {
        //波形データを格納する配列
        float[] data = new float[256];
        float sum = 0;
        //配列dataに波形データを格納する
        audio.GetOutputData(data, 0);
        foreach (float s in data)
        {
            //絶対値(小数点以下も)を足し合わせる
            sum += Mathf.Abs(s);
        }
        //合計した波形データを総数で割って平均値を返す(つまり音の大きさにしか左右されない？)
        return sum / 256.0f;
    }

    //モーションの再生
    public void motionPlay(int num)
    {
        switch (num)
        {
            case 0:
                if (motionMgr.startMotion(motion[0]) == -1) Debug.Log("モーションを再生できません");
                else motionMgr.startMotion(motion[0]);
                break;
            case 1:
                if (motionMgr.startMotion(motion[1]) == -1) Debug.Log("モーションを再生できません");
                else motionMgr.startMotion(motion[1]);
                break;
            case 2:
                if (motionMgr.startMotion(motion[2]) == -1) Debug.Log("モーションを再生できません");
                else motionMgr.startMotion(motion[2]);
                break;
            case 3:
                if (motionMgr.startMotion(motion[3]) == -1) Debug.Log("モーションを再生できません");
                else motionMgr.startMotion(motion[3]);
                break;
            case 4:
                if (motionMgr.startMotion(motion[4]) == -1) Debug.Log("モーションを再生できません");
                else motionMgr.startMotion(motion[4]);
                break;
            case 5:
                if (motionMgr.startMotion(motion[5]) == -1) Debug.Log("モーションを再生できません");
                else motionMgr.startMotion(motion[5]);
                break;
            case 6:
                if (motionMgr.startMotion(motion[6]) == -1) Debug.Log("モーションを再生できません");
                else motionMgr.startMotion(motion[6]);
                break;
            case 7:
                if (motionMgr.startMotion(motion[7]) == -1) Debug.Log("モーションを再生できません");
                else motionMgr.startMotion(motion[7]);
                break;
            case 8:
                if (motionMgr.startMotion(motion[8]) == -1) Debug.Log("モーションを再生できません");
                else motionMgr.startMotion(motion[8]);
                break;
            default: Debug.Log("無効な値です(モーション再生エラー)"); break;
        }

    }
    //モーションの再生(１回だけ)※他モーションを確認せずに再生
    public void motionPlayOnce(int num)
    {
        motionPlay(num);
    }
    //モーションの再生(１回だけ)※停止してから再生
    public void motionPlayOnce_stop(int num)
    {
        motionMgr.stopAllMotions();
        motionPlay(num);
    }
    //モーションの再生(ループ用)※止まっているか確認する
    public void motionPlayLoop(int num)
    {
        if (motionMgr.isFinished())
        {
            motionPlay(num);
        }
    }
    #endregion
    //画像を読みこむ
    public static byte[] ReadPngFile(string path)
    {
        if (System.IO.File.Exists(path) == false)
        {
            Debug.Log("file not found");
            return null;
        }
        else
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader bin = new BinaryReader(fileStream);
            byte[] values = bin.ReadBytes((int)bin.BaseStream.Length);

            bin.Close();

            return values;
        }
    }
    //読み込み画像をテクスチャとして返す
    public static Texture2D ReadPng(string path)
    {
        if (ReadPngFile(path) != null)
        {
            byte[] readBinary = ReadPngFile(path);
            int pos = 16; // 16バイトから開始

            int width = 0;
            for (int i = 0; i < 4; i++)
            {
                width = width * 256 + readBinary[pos++];
            }

            int height = 0;
            for (int i = 0; i < 4; i++)
            {
                height = height * 256 + readBinary[pos++];
            }

            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(readBinary);

            return texture;
        }
        else
        {
            return null;
        }

    }
    //質問関連のオブジェクトを表示・隠す
    void quizObject(bool appear)
    {
        Vector3 pos1 = GOquestion_msbox_.transform.position;
        Vector3 pos2 = questionTextGO_.transform.position;
        if (appear)
        {
            pos1.z = -3;
            pos2.z = -4;
        }
        else
        {
            pos1.z = 3;
            pos2.z = 3;
        }
        GOquestion_msbox_.transform.position = pos1;
        questionTextGO_.transform.position = pos2;
    }
    //選択肢オブジェクトを表示・隠す
    void quizRYB(bool appear)
    {
        Vector3 pos1 = GOquestion_red_.transform.position;
        Vector3 pos2 = questionAnsTextGO1_.transform.position;
        Vector3 pos3 = GOquestion_yellow_.transform.position;
        Vector3 pos4 = questionAnsTextGO2_.transform.position;
        Vector3 pos5 = GOquestion_blue_.transform.position;
        Vector3 pos6 = questionAnsTextGO3_.transform.position;
        if (appear)
        {
            pos1.z = -3;
            pos2.z = -4;
            pos3.z = -3;
            pos4.z = -4;
            pos5.z = -3;
            pos6.z = -4;
        }
        else
        {
            pos1.z = 3;
            pos2.z = 3;
            pos3.z = 3;
            pos4.z = 3;
            pos5.z = 3;
            pos6.z = 3;
        }
        GOquestion_red_.transform.position = pos1;
        questionAnsTextGO1_.transform.position = pos2;
        GOquestion_yellow_.transform.position = pos3;
        questionAnsTextGO2_.transform.position = pos4;
        GOquestion_blue_.transform.position = pos5;
        questionAnsTextGO3_.transform.position = pos6;
    }
    //商品解説オブジェクトを表示・隠す
    void quizInfoMs(bool appear)
    {
        Vector3 pos1 = GOquizBeforeInfo_msbox_.transform.position;
        Vector3 pos2 = GOquizBeforeInfo_button_.transform.position;
        Vector3 pos3 = quizBeforeInfoTextGO_.transform.position;
        //Vector3 pos4 = productGO_.transform.position;
        if (appear)
        {
            pos1.z = -3;
            pos2.z = -4;
            pos3.z = -5;
          //  pos4.z = -3;
        }
        else
        {
            pos1.z = 3;
            pos2.z = 3;
            pos3.z = 3;
            //pos4.z = 3;
        }
        GOquizBeforeInfo_msbox_.transform.position = pos1;
        GOquizBeforeInfo_button_.transform.position = pos2;
        quizBeforeInfoTextGO_.transform.position = pos3;
        //productGO_.transform.position = pos4;
    }
    //○画像を表示・隠す
    void circle_hyouzi(bool appear,int ansnum)
    {
        Vector3 pos1 = GOseikai_circle_.transform.position;
        if (appear)
        {
            pos1.z = -6.5f;
            switch (ansnum)
            {
                case 0: pos1.z = 3; break;
                case 1: pos1.y=-4.5f;break;
                case 2: pos1.y = -6.5f; break;
                case 3: pos1.y = -8.5f; break;
            }
        }
        else
        {
            pos1.z = 3;
        }
        GOseikai_circle_.transform.position = pos1;
    }
    //×画像を表示・隠す
    void cross_hyouzi(bool appear, int ansnum)
    {
        Vector3 pos1 = GOhuseikai_circle_.transform.position;
        if (appear)
        {
            pos1.z = -6.5f;
            switch (ansnum)
            {
                case 0: pos1.z = 3; break;
                case 1: pos1.y = -4.5f; break;
                case 2: pos1.y = -6.5f; break;
                case 3: pos1.y = -8.5f; break;
            }
        }
        else
        {
            pos1.z = 3;
        }
        GOhuseikai_circle_.transform.position = pos1;
    }
    //選択されなかった選択肢用画像を表示・隠す
    void unselected_hyouzi(bool appear, int ansnum)
    {
        Vector3 pos1 = GOunselected_panel1_.transform.position;
        Vector3 pos2 = GOunselected_panel2_.transform.position;
        Vector3 pos3 = GOunselected_panel3_.transform.position;
        if (appear)
        {
            pos1.z = 3;
            pos2.z = 3;
            pos3.z = 3;
            switch (ansnum)
            {
                case 0:
                    pos1.z = -6;
                    pos2.z = -6;
                    pos3.z = -6;
                    break;
                case 1:
                    pos2.z = -6;
                    pos3.z = -6;
                    break;
                case 2:
                    pos1.z = -6;
                    pos3.z = -6;
                    break;
                case 3:
                    pos1.z = -6;
                    pos2.z = -6;
                    break;
            }
        }
        else
        {
            pos1.z = 3;
            pos2.z = 3;
            pos3.z = 3;
        }
        GOunselected_panel1_.transform.position = pos1;
        GOunselected_panel2_.transform.position = pos2;
        GOunselected_panel3_.transform.position = pos3;
    }
}
