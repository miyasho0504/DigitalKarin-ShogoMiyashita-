using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;//add
using System.IO;//add
using live2d;//add
using live2d.framework;

public class game1Script : MonoBehaviour
{
    #region 変数宣言
    int step = 0;
    float time = 0;
    float escTime = 0;//Escキー(メインへ強制遷移)
    float resetTime = 0;
    bool once = false;
    bool once2 = false;
    bool once3 = false;
    int player=0;//プレイヤーのじゃんけんの手　グー１チョキ２パー３
    int winP = 0;
    int winK = 0;
    int rand;
    int winVIC = 2;//何回勝ったら勝ちかを決める変数
    int nokori = 3;//あと何回か

    //ログの出力
    LogOutput _LogO = new LogOutput();

    //じゃんけんイラスト&はい・いいえ
    public GameObject janken_sprite1;
    public GameObject janken_sprite2;
    public GameObject janken_sprite3;
    public GameObject janken_msbox_sprite;
    public GameObject yes_sprite;
    public GameObject no_sprite;
    public GameObject continue_msbox_sprite;

    //音声ファイル格納用変数
    public AudioClip voice_game_janken_wait;
    public AudioClip voice_game_janken_result;
    public AudioClip voice_game_janken_aiko;
    public AudioClip voice_game_janken_aikoresult;
    public AudioClip voice_game_janken_replay;
    public AudioClip voice_game_janken_replaywait;
    public AudioClip voice_game_janken_winkarin;
    public AudioClip voice_game_janken_winplayer;

    private AudioSource audioSource_;

    //テキスト関連の変数
    public GameObject textRenderObject_;        //テキストを表示するオブジェクト
    public GameObject textRenderObject2_;
    public GameObject continue_textRenderObject_;//再戦時のテキスト表示用オブジェクト
    public Sprite[] jankenText_=new Sprite[11];  //テキストのスプライト管理する配列
    private SpriteRenderer textspriteRenderer_;         //スプライトを変更するために使用するもの
    private SpriteRenderer textspriteRenderer2_;
    private SpriteRenderer continue_textspriteRenderer_;
    //残り回数の表示用
    public GameObject numberText_;
    //public GameObject nokoriText_;
    public Sprite[] number_=new Sprite[4];
    private SpriteRenderer numberTextRenderer_;

    #region Live2D用変数
    private Live2DModelUnity live2DModel;
    private EyeBlinkMotion eyeBlink;
    private Live2DMotion[] motion;
    private L2DExpressionMotion[] expression;
    private MotionQueueManager motionMgr;
    private MotionQueueManager expressionMgr;

        private Matrix4x4 live2DCanvasPos;

        //リップシンク用変数
        private float scaleVolume =30; //音声の倍率（後で実値にかけて使う）
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
    #endregion　変数宣言終了
        // Use this for initialization
	void Start () {
        resetTime = 0;

	    audioSource_=gameObject.GetComponent<AudioSource>();
        textspriteRenderer_=textRenderObject_.GetComponent<SpriteRenderer>();
        textspriteRenderer2_=textRenderObject2_.GetComponent<SpriteRenderer>();
        numberTextRenderer_ = numberText_.GetComponent<SpriteRenderer>();
        continue_textspriteRenderer_ = continue_textRenderObject_.GetComponent<SpriteRenderer>();
        appear_hand();
        hide_choice();

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
	
	// Update is called once per frame
	void Update ()
    {
        #region Live2D
        eyeBlink.setInterval(blink_interval);
        if (live2DModel == null) return;
        live2DModel.setMatrix(transform.localToWorldMatrix * live2DCanvasPos);

        if (!Application.isPlaying)
        {
            eyeBlink.setParam(live2DModel);
            live2DModel.update();
            return;
        }
        #endregion

        #region メインへの強制遷移処理
        if (Input.GetKey(KeyCode.Escape))
        {
            escTime += Time.deltaTime;
            //Debug.Log(escTime);
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

        #region じゃんけん処理
        if (step == 0)
            {
                
                if (once == false)
                {
                    
                    //じゃんけん待機モーション再生
                    Debug.Log("じゃんけん･･･");
                    motionMgr.stopAllMotions();
                    textspriteRenderer_.sprite = jankenText_[0];//テキスト表示を変更
                    textspriteRenderer2_.sprite = null;
                    if(1<=nokori&&nokori<=3){
                        numberTextRenderer_.sprite = number_[nokori];
                    }
                    else
                    {
                        numberTextRenderer_.sprite = null;
                    }
                    Debug.Log(nokori);
                    audioSource_.PlayOneShot(voice_game_janken_wait);
                    once = true;
                    //じゃんけん用の乱数を生成
                    rand = UnityEngine.Random.Range(1, 4);
                    Debug.Log("かりんちゃんの手="+rand);
                    //ボイスの再生中は押せないようにする
                    appear_hand();
                    hide_hand_only();
                }
                else if (once2==false&&audioSource_.isPlaying == false)
                {
                    if (once2 == false)
                    {
                        //じゃんけんの画像を手前に表示
                        appear_hand();
                        if (Input.GetKeyDown(KeyCode.A))
                        {
                            motionPlayOnce_stop(rand);
                            once2 = true;
                            player = 1;
                        }
                        else if (Input.GetKeyDown(KeyCode.S))
                        {
                            motionPlayOnce_stop(rand);
                            once2 = true;
                            player = 2;
                        }
                        else if (Input.GetKeyDown(KeyCode.D))
                        {
                            motionPlayOnce_stop(rand);
                            once2 = true;
                            player = 3;
                        }
                    }
                }
                if (once2 == true)
                {
                    motionPlayLoop(rand + 3);
                    time +=Time.deltaTime;
                    if (time > 0.0f)
                    {
                        if (once3 == false)
                        {
                            once3 = true;
                            textspriteRenderer_.sprite = jankenText_[1];//テキスト表示を変更
                            audioSource_.PlayOneShot(voice_game_janken_result);
                            hide_hand(player);
                            winlose();
                        }
                    }
                    if(time>2.0f){
                       janken_pon();
                       time = 0;
                        once3=false;
                    }
                }
                motionPlayLoop(0);
            }
        #endregion
        #region あいこ
        else if (step == 1)
            {
                
                if (once == false)
                {
                    //Debug.Log("あいこで･･･");
                    motionMgr.stopAllMotions();
                    textspriteRenderer_.sprite = jankenText_[2];//テキスト表示を変更
                    textspriteRenderer2_.sprite = null;
                    audioSource_.PlayOneShot(voice_game_janken_aiko);
                    once = true;
                    //じゃんけん用の乱数を生成
                    rand = UnityEngine.Random.Range(1, 4);
                    Debug.Log("かりんちゃんの手="+rand);
                    //ボイスの再生中は押せないようにする
                    hide_hand_only();
                }
                else if (audioSource_.isPlaying == false)
                {
                    
                    if (once2 == false)
                    {
                        //じゃんけんの画像を手前に表示
                        appear_hand();
                        if (Input.GetKeyDown(KeyCode.A))
                        {
                            motionPlayOnce_stop(rand);
                            once2 = true;
                            player = 1;
                        }
                        else if (Input.GetKeyDown(KeyCode.S))
                        {
                            motionPlayOnce_stop(rand);
                            once2 = true;
                            player = 2;
                        }
                        else if (Input.GetKeyDown(KeyCode.D))
                        {
                            motionPlayOnce_stop(rand);
                            once2 = true;
                            player = 3;
                        }
                    }
                }
                if (once2 == true)
                {
                    motionPlayLoop(rand + 3);
                    time += Time.deltaTime;
                    if (time > 0.0f)
                    {
                        if (once3 == false)
                        {
                            once3 = true;
                            textspriteRenderer_.sprite = jankenText_[3];//テキスト表示を変更
                            audioSource_.PlayOneShot(voice_game_janken_aikoresult);
                            hide_hand(player);
                            winlose();
                        }
                    }
                    if (time > 2.0f)
                    {
                        aiko_sho();
                        time = 0;
                        once3 = false;
                    }
                }
                motionPlayLoop(0);
            }
            #endregion
        #region 勝ち
            else if (step == 11)
            {
                if (once == false)
                {
                    if (1 <= nokori && nokori <= 3)
                    {
                        numberTextRenderer_.sprite = number_[nokori];
                    }
                    else
                    {
                        numberTextRenderer_.sprite = null;
                    }
                    once = true;
                    motionPlayOnce_stop(7);
                    if (winP == 3) textspriteRenderer_.sprite = jankenText_[11];
                    else if (winP == 2) textspriteRenderer_.sprite = jankenText_[12];
                    textspriteRenderer2_.sprite = jankenText_[4];//テキスト表示を変更
                    //textspriteRenderer2_.sprite = null;
                    audioSource_.PlayOneShot(voice_game_janken_winplayer);
                }
                if (audioSource_.isPlaying == false)
                {
                    time += Time.deltaTime;
                    if(time>1.0f){
                        time=0;
                        once = false;
                        step = 2;
                    }
                }
            }
            #endregion 
        #region 負け
            else if (step == 12)
            {
                if (once == false)
                {
                    if (1 <= nokori && nokori <= 3)
                    {
                        numberTextRenderer_.sprite = number_[nokori];
                    }
                    else
                    {
                        numberTextRenderer_.sprite = null;
                    }
                    once = true;
                    motionPlayOnce_stop(8);
                    if (winP == 1) textspriteRenderer_.sprite = jankenText_[13];
                    else if (winP == 0) textspriteRenderer_.sprite = jankenText_[14];
                    textspriteRenderer2_.sprite = jankenText_[5];//テキスト表示を変更
                    //textspriteRenderer2_.sprite = null;
                    audioSource_.PlayOneShot(voice_game_janken_winkarin);
                }
                if (audioSource_.isPlaying == false)
                {
                    time += Time.deltaTime;
                    if (time > 1.0f)
                    {
                        time = 0;
                        once = false;
                        step = 2;
                    }
                }
            }
            #endregion
        #region リプレイ入力待ち
            else if (step == 2)
            {
                if (once == false)
                {
                    //Debug.Log("もう一度じゃんけんしますか？");
                    continue_textspriteRenderer_.sprite = jankenText_[6];//テキスト表示を変更
                    audioSource_.PlayOneShot(voice_game_janken_replaywait);
                    Debug.Log("ボタン1→リプレイ、ボタン3→ゲームモードTOP");
                    hide_all_hand();
                    appear_choice();
                    once = true;
                }
                else if (audioSource_.isPlaying == false)
                {
                    appear_choice_button();
                    //ボタン1押下→リプレイ
                    if (Input.GetKeyDown(KeyCode.A))
                    {
                        //ログの出力
                        _LogO.LogOutputTextfile("JYANKENREP", "0", "JYANKEN", winP.ToString());
                        
                        continue_textspriteRenderer_.sprite = jankenText_[7];//テキスト表示を変更
                        audioSource_.PlayOneShot(voice_game_janken_replay);
                        step++;
                        once = false;
                    }
                    //ボタン3押下→ゲームモードTOP
                    else if (Input.GetKeyDown(KeyCode.D))
                    {
                        //ログの出力
                        _LogO.LogOutputTextfile("JYANKENREP", "0", "BACK", winP.ToString());

                        once = false;
                        SceneManager.LoadScene("game");
                    }
                }
            }
            #endregion
        #region 再戦(ステップを0に戻す)
            else if (step == 3)
            {
                if (audioSource_.isPlaying == false)
                {
                    step = 0;
                    winP = 0;
                    winK = 0;
                    nokori = 3;
                    hide_choice();
                }
            }
            #endregion

        #region Live2Dその2
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
        live2DModel.setParamFloat("PARAM_MOUTH_FORM",0);
        volume=volume*scaleVolume;
        if(volume>1.0f){
            volume = 1.0f;
        }
        //Debug.Log("volume="+volume);
        live2DModel.setParamFloat("PARAM_MOUTH_OPEN_Y",volume);
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
        #endregion
    }
#region 関数定義
#region ジャンケン用関数
    void janken_pon()
    {
        #region グーの時
        if (player == 1)
        {
            Debug.Log("ぽん！(グー)");
            switch (rand)
            {
                case 1://あいこ
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "2");
                    step = 1;
                    once = false;
                    break;
                case 2://勝ち
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "1");

                    winP++;
                    nokori--;
                    Debug.Log("winP=" + winP);
                    if(nokori==0){
                        if (winP >= winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if(winK>=winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    once = false;
                    
                    break;
                case 3://負け
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "0");

                    winK++;
                    nokori--;
                    Debug.Log("winK=" + winK);
                    if (nokori == 0)
                    {
                        if (winP >= winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if (winK >= winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    once = false;
                    
                    break;
                default: break;
            }
        }
        #endregion

        #region チョキの時
        else if (player == 2)
        {
            Debug.Log("ぽん！(チョキ)");
            switch (rand)
            {
                case 2://あいこ
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "2");

                    step = 1;
                    once = false;
                    break;
                case 3://勝ち
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "1");

                    winP++;
                    nokori--;
                    Debug.Log("winP=" + winP);
                    if(nokori==0){
                        if (winP >= winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if (winK >= winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    once = false;
                    break;
                case 1://負け
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "0");

                    winK++;
                    nokori--;
                    Debug.Log("winK=" + winK);
                    if (nokori == 0)
                    {
                        if (winP >= winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if (winK >= winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    once = false;
                    break;
                default: break;
            }
        }
        #endregion

        #region パーの時
        else if (player == 3)
        {
            Debug.Log("ぽん！(パー)");
            switch (rand)
            {
                case 3://あいこ
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "2");

                    step = 1;
                    once = false;
                    break;
                case 1://勝ち
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "1");

                    winP++;
                    nokori--;
                    Debug.Log("winP=" + winP);
                    if(nokori==0){
                        if (winP >= winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if (winK >= winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    once = false;
                    break;
                case 2://負け
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "0");

                    winK++;
                    nokori--;
                    Debug.Log("winK=" + winK);
                    if (nokori == 0)
                    {
                        if (winP >= winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if (winK >= winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    once = false;
                    break;
                default: break;
            }
        }
        #endregion
        once2 = false;
    }
    void aiko_sho()
    {
        #region グーの時
        if (player == 1)
        {
            Debug.Log("しょ！(グー)");
            switch (rand)
            {
                case 1://あいこ
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "2");

                    step = 1;
                    once = false;
                    break;
                case 2://勝ち
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "1");

                    winP++;
                    nokori--;
                    Debug.Log("winP=" + winP);
                    if (nokori==0)
                    {
                        if (winP>=winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if (winK >= winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    else
                    {
                        step = 0;
                    }
                    once = false;
                    break;
                case 3://負け
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "0");

                    winK++;
                    nokori--;
                    Debug.Log("winK=" + winK);
                    if (nokori == 0)
                    {
                        if (winP  >=winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if (winK >= winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    else
                    {
                        step = 0;
                    }
                    once = false;
                    break;
                default: break;
            }
        }
        #endregion

        #region チョキの時
        else if (player == 2)
        {
            Debug.Log("しょ！(チョキ)");
            switch (rand)
            {
                case 2://あいこ
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "2");

                    step = 1;
                    once = false;
                    break;
                case 3://勝ち
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "1");

                    winP++;
                    nokori--;
                    Debug.Log("winP=" + winP);
                    if (nokori==0)
                    {
                        if (winP >= winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if (winK >= winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    else
                    {
                        step = 0;
                    }
                    once = false;
                    break;
                case 1://負け
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "0");

                    winK++;
                    nokori--;
                    Debug.Log("winK=" + winK);
                    if (nokori == 0)
                    {
                        if (winP >= winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if (winK >= winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    else
                    {
                        step = 0;
                    }
                    once = false;
                    break;
                default: break;
            }
        }
        #endregion

        #region パーの時
        else if (player == 3)
        {
            Debug.Log("しょ！(パー)");
            switch (rand)
            {
                case 3://あいこ
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "2");

                    step = 1;
                    once = false;
                    break;
                case 1://勝ち
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "1");

                    winP++;
                    nokori--;
                    Debug.Log("winP=" + winP);
                    if (nokori==0)
                    {
                        if (winP >= winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if (winK >= winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    else
                    {
                        step = 0;
                    }
                    once = false;
                    break;
                case 2://負け
                    //ログを出力
                    _LogO.LogOutputTextfile("JYANKEN", (4 - nokori).ToString(), (player - 1).ToString(), "0");

                    winK++;
                    nokori--;
                    Debug.Log("winK=" + winK);
                    if (nokori == 0)
                    {
                        if (winP >= winVIC)
                        {
                            Debug.Log("あなたの勝ち！");
                            step = 11;
                        }
                        else if (winK >= winVIC)
                        {
                            Debug.Log("わたしの勝ち！");
                            step = 12;
                        }
                    }
                    else
                    {
                        step = 0;
                    }
                    once = false;
                    break;
                default: break;
            }
        }
        #endregion
        once2 = false;
    }
#endregion ジャンケン用関数終了
    #region 表示系関数
    //全ての手を表示する
    void appear_hand(){
        Vector3 pos1 = janken_sprite1.transform.position;
        Vector3 pos2 = janken_sprite2.transform.position;
        Vector3 pos3 = janken_sprite3.transform.position;
        Vector3 pos4 = janken_msbox_sprite.transform.position;
        Vector3 pos5 = textRenderObject_.transform.position;
        Vector3 pos6 = textRenderObject2_.transform.position;
        //Vector3 pos7 = nokoriText_.transform.position;
        Vector3 pos8 = numberText_.transform.position;
        pos1.z = -1;
        pos2.z = -1;
        pos3.z = -1;
        pos4.z = -1;
        pos5.z = -2;
        pos6.z = -3;
        //pos7.z = -2;
        pos8.z = -3;
        janken_sprite1.transform.position = pos1;
        janken_sprite2.transform.position = pos2;
        janken_sprite3.transform.position = pos3;
        janken_msbox_sprite.transform.position = pos4;
        textRenderObject_.transform.position = pos5;
        textRenderObject2_.transform.position = pos6;
        //nokoriText_.transform.position = pos7;
        numberText_.transform.position = pos8;
    }
    //全ての手を隠す
    void hide_all_hand()
    {
        Vector3 pos1 = janken_sprite1.transform.position;
        Vector3 pos2 = janken_sprite2.transform.position;
        Vector3 pos3 = janken_sprite3.transform.position;
        Vector3 pos4 = janken_msbox_sprite.transform.position;
        Vector3 pos5 = textRenderObject_.transform.position;
        Vector3 pos6 = textRenderObject2_.transform.position;
        //Vector3 pos7 = nokoriText_.transform.position;
        Vector3 pos8 = numberText_.transform.position;
        pos1.z = 3;
        pos2.z = 3;
        pos3.z = 3;
        pos4.z = 3;
        pos5.z = 3;
        pos6.z = 3;
        //pos7.z = 3;
        pos8.z = 3;
        janken_sprite1.transform.position = pos1;
        janken_sprite2.transform.position = pos2;
        janken_sprite3.transform.position = pos3;
        janken_msbox_sprite.transform.position = pos4;
        textRenderObject_.transform.position=pos5;
        textRenderObject2_.transform.position = pos6;
        //nokoriText_.transform.position = pos7;
        numberText_.transform.position = pos8;
    }
    //出していない手を隠す
    void hide_hand(int n)
    {
        Vector3 pos1 = janken_sprite1.transform.position;
        Vector3 pos2 = janken_sprite2.transform.position;
        Vector3 pos3 = janken_sprite3.transform.position;
        if(n!=1){
            pos1.z = 3;
            janken_sprite1.transform.position =pos1;
        }
        if (n != 2)
        {
            pos2.z = 3;
            janken_sprite2.transform.position = pos2;
        }
        if (n != 3)
        {
            pos3.z = 3;
            janken_sprite3.transform.position = pos3;
        }

    }
    void hide_hand_only()
    {
        Vector3 pos1 = janken_sprite1.transform.position;
        Vector3 pos2 = janken_sprite2.transform.position;
        Vector3 pos3 = janken_sprite3.transform.position;
        pos1.z = 3;
        pos2.z = 3;
        pos3.z = 3;
        janken_sprite1.transform.position = pos1;
        janken_sprite2.transform.position = pos2;
        janken_sprite3.transform.position = pos3;
    }

    void appear_choice()
    {
        Vector3 pos1 = continue_msbox_sprite.transform.position;
        Vector3 pos2 = continue_textRenderObject_.transform.position;
        pos1.z = -2;
        pos2.z = -3;
        continue_msbox_sprite.transform.position = pos1;
        continue_textRenderObject_.transform.position = pos2;
    }
    void appear_choice_button()
    {
        Vector3 pos1 = yes_sprite.transform.position;
        Vector3 pos2 = no_sprite.transform.position;
        pos1.z = -3;
        pos2.z = -3;
        yes_sprite.transform.position = pos1;
        no_sprite.transform.position = pos2;
    }
    void hide_choice()
    {
        Vector3 pos1 = yes_sprite.transform.position;
        Vector3 pos2 = no_sprite.transform.position;
        Vector3 pos3 = continue_msbox_sprite.transform.position;
        Vector3 pos4 = continue_textRenderObject_.transform.position;
        pos1.z = 3;
        pos2.z = 3;
        pos3.z = 3;
        pos4.z = 3;
        yes_sprite.transform.position = pos1;
        no_sprite.transform.position = pos2;
        continue_msbox_sprite.transform.position = pos3;
        continue_textRenderObject_.transform.position = pos4;
    }
    //勝ち負けのテキストを表示
    void winlose()
    {
        if(player==1){
            if(rand==1){
                textspriteRenderer2_.sprite=jankenText_[10];
            }else if(rand==2){
                textspriteRenderer2_.sprite=jankenText_[8];
            }else if(rand==3){
                textspriteRenderer2_.sprite=jankenText_[9];
            }
        }
        else if (player == 2)
        {
            if (rand == 1)
            {
                textspriteRenderer2_.sprite = jankenText_[9];
            }
            else if (rand == 2)
            {
                textspriteRenderer2_.sprite = jankenText_[10];
            }
            else if (rand == 3)
            {
                textspriteRenderer2_.sprite = jankenText_[8];
            }
        }
        else if(player==3)
        {
            if (rand == 1)
            {
                textspriteRenderer2_.sprite = jankenText_[8];
            }
            else if (rand == 2)
            {
                textspriteRenderer2_.sprite = jankenText_[9];
            }
            else if (rand == 3)
            {
                textspriteRenderer2_.sprite = jankenText_[10];
            }
        }
    }

    #region Live2D用関数
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
#endregion 表示系関数終了
    #endregion 関数宣言終了
}
