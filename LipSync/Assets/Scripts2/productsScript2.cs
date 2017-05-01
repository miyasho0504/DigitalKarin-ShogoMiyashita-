using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;
using System;//add
using System.IO;//add
using live2d;//add
using live2d.framework;

public class productsScript2 : MonoBehaviour
{
    #region 変数宣言
    int step = 0;
    float escTime = 0;//Escキー(メインへ強制遷移)
    float resetTime = 0;
    bool once = false;
    bool once2 = false;
    bool once3 = false;
    int ansNum = 0;
    int ansABC = 0;
    int questionNum = 0;
    int ansCount = 0;//現在が何問目か

    //ログの出力
    LogOutput _LogO = new LogOutput();
    int _LogQuestionNum = 0;

    public GameObject GOquestion_msbox_;
    public GameObject GOquestion_text_;
    
    public GameObject GOquestion_red_;
    public GameObject GOquestion_red_text_;
    public GameObject GOquestion_yellow_;
    public GameObject GOquestion_yellow_text_;
    public GameObject GOquestion_blue_;
    public GameObject GOquestion_blue_text_;

    public GameObject GOproductInfo_msbox_;
    public GameObject GOproductInfo_button_;
    public GameObject GOproductInfo_text_;

    #region ボイス用変数
    //基本ボイスの名前
    private AudioSource audioSource_;
    private AudioClip[] voice_product_basic_;
    private AudioClip[] voice_product_question_;
    private AudioClip[] voice_product_info_;
    #endregion

    #region CSV
    private class QuestionData
    {
        [CsvColumnAttribute(0, 0)]
        private int no;

        [CsvColumnAttribute(1, 0)]
        private int next1;

        [CsvColumnAttribute(2, 0)]
        private int next2;

        [CsvColumnAttribute(3, 0)]
        private int next3;

        public override string ToString()
        {
            return string.Format("No={0} next1={1}, next2={2}, next3={3}", no, next1, next2, next3);
        }

    }
    #endregion

    #region 画像ファイル
    
    //質問画像
    public GameObject questionTextGO_;
    Sprite[] questionTextSprites = new Sprite[14];
    private SpriteRenderer questionTextSpriteRenderer_;
    //選択肢画像
    public GameObject questionAnsTextGO1_;
    public GameObject questionAnsTextGO2_;
    public GameObject questionAnsTextGO3_;
    Sprite[] questionAnsTextSprites=new Sprite[40];
    private SpriteRenderer questionAnsTextSpriteRenderer1_;
    private SpriteRenderer questionAnsTextSpriteRenderer2_;
    private SpriteRenderer questionAnsTextSpriteRenderer3_;
    //商品画像
    public GameObject productGO_;
    Sprite[] productSprites = new Sprite[28];
    private SpriteRenderer productSpriteRenderer_;
    //商品説明画像
    public GameObject productInfoGO_;
    Sprite[] productInfoSprites = new Sprite[28];
    private SpriteRenderer productInfoSpriteRenderer_;

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

        //リソースを管理しているオブジェクトを呼び出す
        GameObject go = GameObject.Find("ResourceManager");
        resourceManageScript resScript = go.GetComponent<resourceManageScript>();

        #region　ボイス読み込み
        audioSource_ = gameObject.GetComponent<AudioSource>();
        //基本ボイスの読み込み
        voice_product_basic_ = resScript.voice_product_basic_;
        //質問ボイスの読み込み
        voice_product_question_ = resScript.voice_product_question_;
        //商品説明ボイス
        voice_product_info_ = resScript.voice_product_info_;
        #endregion

        //CSV
        //readerをList<TestEnemyData>化して、それぞれ表示
        using (var reader = new CSVReader<QuestionData>("Texts/questionData", true))
        {
            reader.ToList().ForEach(question => Debug.Log(question.ToString()));
        }

        #region 画像読み込み
        //質問
        questionTextSpriteRenderer_ = questionTextGO_.GetComponent<SpriteRenderer>();
        questionTextSprites = resScript.productquestionTextSprites;
        //選択肢
        questionAnsTextSpriteRenderer1_ = questionAnsTextGO1_.GetComponent<SpriteRenderer>();
        questionAnsTextSpriteRenderer2_ = questionAnsTextGO2_.GetComponent<SpriteRenderer>();
        questionAnsTextSpriteRenderer3_ = questionAnsTextGO3_.GetComponent<SpriteRenderer>();
        questionAnsTextSprites = resScript.productquestionAnsTextSprites;
        //商品画像
        productSpriteRenderer_ = productGO_.GetComponent<SpriteRenderer>();
        productSprites = resScript.productSprites;
        //商品説明画像
        productInfoSpriteRenderer_ = productInfoGO_.GetComponent<SpriteRenderer>();
        productInfoSprites = resScript.productInfoSprites;
        #endregion

        productInfoMs(false);
        questionObject(false);
        questionRYB(false);
    }
	
	// Update is called once per frame
	void Update () {
        live2d_update1();
        /*
        time += 0.1f;
        questionTextSpriteRenderer_.sprite = questionTextSprites[(int)time%13];
        questionAnsTextSpriteRenderer_.sprite = questionAnsTextSprites[(int)time % 39];
        */
        #region メインへの強制遷移処理
        if (Input.GetKey(KeyCode.Escape))
        {
            escTime += Time.deltaTime;
            Debug.Log(escTime);
            if (escTime > 5.0f)
            {
                SceneManager.LoadScene("main");
            }
        }
        else
        {
            escTime = 0.0f;
        }
        #endregion
        #region メインへの強制遷移処理(T.O)
        resetTime += Time.deltaTime;
        //Debug.Log("resetTime=" + resetTime);
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


        #region TOP説明
        if (step == 0)
        {
            motionPlayLoop(3);
            if (once == false)
            {
                Debug.Log("TOP説明");
                //Debug.Log(voice_product_basic_[0]);
                audioSource_.PlayOneShot(voice_product_basic_[0]);
                
                Debug.Log("T.Oかボタン押下で次のステップへ");
                once = true;
                //質問の番号をQ1にする&現在の状態を1問目にする
                questionNum = 1;
                ansNum = 1;
                ansCount = 0;
            }
            //ボタン押下で次のステップへ
            if (audioSource_.isPlaying == false)
            {
                step = 1;
                once = false;
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                audioSource_.Stop();
                step = 1;
                once = false;
            }
        }
        #endregion

        #region 商品オススメQ
        else if (step == 1)
        {
            motionPlayLoop(2);
            if (once == false)
            {
                //現在のクイズ番号のスプライトを表示する
                questionTextSpriteRenderer_.sprite = questionTextSprites[questionNum];
                questionAnsTextSpriteRenderer1_.sprite=questionAnsTextSprites[questionNum*3-2];
                questionAnsTextSpriteRenderer2_.sprite = questionAnsTextSprites[questionNum * 3 - 1];
                questionAnsTextSpriteRenderer3_.sprite = questionAnsTextSprites[questionNum * 3];
                if (ansCount == 0)
                {
                    //Debug.Log("Q1");
                    audioSource_.PlayOneShot(voice_product_basic_[1]);
                }
                else
                {
                    //Debug.Log("Q2Q3");
                    audioSource_.PlayOneShot(voice_product_basic_[2]);
                }
                //Debug.Log("ボタン(A,S,D)押下で次のステップへ");
                once = true;
                questionRYB(true);
                questionObject(true);
            }else if(once2==false&&audioSource_.isPlaying==false){
                once2 = true;
                Debug.Log("QNum="+questionNum);
                audioSource_.PlayOneShot(voice_product_question_[questionNum]);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                audioSource_.Stop();
                //ansCount++;
                //ログ用にクイズの番号を保存しておく
                _LogQuestionNum = questionNum;
                Debug.Log("LogQNum=" + _LogQuestionNum);
                ansNum = questionNum * 3 - 2;
                questionNum = questionNum * 3 - 1;
                ansABC=1;
                questionRYB(false, ansABC);//回答した選択肢以外は非表示にする
                step=2;
                once = false;
                once2 = false;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                audioSource_.Stop();
                //ansCount++;
                //ログ用にクイズの番号を保存しておく
                _LogQuestionNum = questionNum;
                Debug.Log("LogQNum=" + _LogQuestionNum);
                ansNum = questionNum * 3 - 1;
                questionNum = questionNum * 3;
                ansABC=2;
                questionRYB(false, ansABC);//回答した選択肢以外は非表示にする
                step=2;
                once = false;
                once2 = false;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                audioSource_.Stop();
                //ansCount++;
                //ログ用にクイズの番号を保存しておく
                _LogQuestionNum = questionNum;
                Debug.Log("LogQNum=" + _LogQuestionNum);
                ansNum = questionNum * 3;
                questionNum = questionNum * 3 +1;
                ansABC=3;
                questionRYB(false, ansABC);//回答した選択肢以外は非表示にする
                step=2;
                once = false;
                once2 = false;
            }
        }
        #endregion

        #region 商品オススメA
        else if (step == 2)
        {
            motionPlayLoop(3);
            if (once == false)
            {
                ansCount++;
                //ログの出力
                if (ansCount == 1)
                {
                    _LogO.LogOutputTextfile("OSUSUME1", _LogQuestionNum.ToString(), (ansABC - 1).ToString(), "0");
                }
                else if (ansCount == 2)
                {
                    _LogO.LogOutputTextfile("OSUSUME2", _LogQuestionNum.ToString(), (ansABC - 1).ToString(), "0");
                }
                else if (ansCount == 3)
                {
                    //3問目の時だけは商品IDを最後に出力
                    _LogO.LogOutputTextfile("OSUSUME3", _LogQuestionNum.ToString(), (ansABC - 1).ToString(), (ansNum-12).ToString());
                }

                //現在のクイズ番号のスプライトを表示する
                //questionTextSpriteRenderer_.sprite = questionTextSprites[questionNum - 1];
                //questionAnsTextSpriteRenderer1_.sprite = questionAnsTextSprites[questionNum * 3 - 3];
                //questionAnsTextSpriteRenderer2_.sprite = questionAnsTextSprites[questionNum * 3 - 2];
                //questionAnsTextSpriteRenderer3_.sprite = questionAnsTextSprites[questionNum * 3 - 1];
                //回答選択ボイスの再生「～ですね」ABCなどのほうがボイスが少なくすむ？
                audioSource_.PlayOneShot(voice_product_basic_[ansABC+2]);
                once = true;
            }else if(audioSource_.isPlaying==false){
                //ansCount==3→3問目の回答、つまりオススメ商品が確定したとき
                if (ansCount == 3)
                {
                    if (once2 == false)
                    {
                        once2 = true;
                        //ボイス：分かりました
                        audioSource_.PlayOneShot(voice_product_basic_[6]);
                    }
                    else if(audioSource_.isPlaying==false)
                    {
                        //オススメ商品の紹介へ移る
                        step++;
                        once2 = false;
                        once = false;
                    }
                }
                else
                {
                    //3問目の回答でない場合、再度質問へ戻る
                    step = 1;
                    once = false;
                }
                
            }
        }
        #endregion

        #region オススメ商品紹介
        if (step == 3)
        {
            motionPlayLoop(2);
            if (once == false)
            {
                //質問用MSBOXと選択肢を隠す,商品画像だけ先に表示する
                questionObject(false);
                questionRYB(false);
                //productInfoMs(true);
                productImageAppear();
                //「あなたにお薦めの商品はこちらです」
                audioSource_.PlayOneShot(voice_product_basic_[8]);
                //オススメ商品画像表示
                productSpriteRenderer_.sprite = productSprites[ansNum - 12];
                once = true;
            }
            else if (once2 == false && audioSource_.isPlaying == false)
            {
                //商品おすすめ文MSBOXを表示
                productInfoMs(true);

                Debug.Log("オススメ商品");
                //オススメ商品ボイス再生
                Debug.Log(ansNum);
                audioSource_.PlayOneShot(voice_product_info_[ansNum - 12]);
                
                //オススメ商品解説文表示
                productInfoSpriteRenderer_.sprite = productInfoSprites[ansNum - 12];
                once2 = true;
            }
            else if (once3 == false && audioSource_.isPlaying == false)
            {
                audioSource_.PlayOneShot(voice_product_basic_[7]);
                once3 = true;
            }
            //ボタン押下でメインメニューへ
            if (once3 == true && audioSource_.isPlaying == false)
            {
                productInfoButtonAppear();
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Escape))
                {
                    audioSource_.Stop();
                    step = 0;
                    once = false;
                    once2 = false;
                    once3 = false;
                    SceneManager.LoadScene("main");
                }
            }
            //time += Time.deltaTime;
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

        //Live2Dモデルの口のY座標を変更する(リップシンクを行う)
        live2DModel.setParamFloat("PARAM_MOUTH_FORM", 0);
        live2DModel.setParamFloat("PARAM_MOUTH_OPEN_Y", volume * scaleVolume);
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
    void questionObject(bool appear)
    {
        Vector3 pos1 = GOquestion_msbox_.transform.position;
        Vector3 pos2 = GOquestion_text_.transform.position;
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
        GOquestion_text_.transform.position=pos2;
    }
    //選択肢オブジェクトを表示・隠す
    void questionRYB(bool appear ,int num=0)
    {
        Vector3 pos1 = GOquestion_red_.transform.position;
        Vector3 pos2 = GOquestion_red_text_.transform.position;
        Vector3 pos3 = GOquestion_yellow_.transform.position;
        Vector3 pos4 = GOquestion_yellow_text_.transform.position;
        Vector3 pos5 = GOquestion_blue_.transform.position;
        Vector3 pos6 = GOquestion_blue_text_.transform.position;
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
            if (num == 0 || num == 2||num==3)
            {
                pos1.z = 3;
                pos2.z = 3;
            }
            if (num == 0 || num == 1 || num == 3)
            {
                pos3.z = 3;
                pos4.z = 3;
            }
            if (num == 0 || num == 1 || num == 2)
            {
                pos5.z = 3;
                pos6.z = 3;
            }
        }
        GOquestion_red_.transform.position=pos1;
        GOquestion_red_text_.transform.position = pos2;
        GOquestion_yellow_.transform.position = pos3;
        GOquestion_yellow_text_.transform.position = pos4;
        GOquestion_blue_.transform.position = pos5;
        GOquestion_blue_text_.transform.position = pos6;
    }
    //商品解説オブジェクトを表示・隠す
    void productInfoMs(bool appear)
    {
        Vector3 pos1 = GOproductInfo_msbox_.transform.position;
        Vector3 pos2 = GOproductInfo_button_.transform.position;
        Vector3 pos3 = GOproductInfo_text_.transform.position;
        Vector3 pos4 = productGO_.transform.position;
        if (appear)
        {
            pos1.z = -3;
            //pos2.z = -4;
            pos3.z = -5;
            pos4.z = -3;
        }
        else
        {
            pos1.z = 3;
            pos2.z = 3;
            pos3.z = 3;
            pos4.z = 3;
        }
        GOproductInfo_msbox_.transform.position = pos1;
        GOproductInfo_button_.transform.position = pos2;
        GOproductInfo_text_.transform.position = pos3;
        productGO_.transform.position=pos4;
    }
    //商品解説時のボタン・テキストを表示
    void productInfoButtonAppear()
    {
        Vector3 pos = GOproductInfo_button_.transform.position;
        pos.z = -5;
        GOproductInfo_button_.transform.position = pos;
    }
    //商品画像のオブジェクトを表示する
    void productImageAppear()
    {
        Vector3 pos = productGO_.transform.position;
        pos.z = -3;
        productGO_.transform.position = pos;
    }
}
