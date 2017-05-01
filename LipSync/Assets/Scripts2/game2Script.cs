using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;//add
using System.IO;//add
using live2d;//add
using live2d.framework;

public class game2Script : MonoBehaviour
{
    bool lipsync = true;//リップシンクするかどうか(SEでリップシンクする問題対策)
    //ログ出力
    LogOutput _LogO=new LogOutput();
    //プレイヤーが正解したかどうか
    int _LogWinP=0;

#region シャッフルゲーム用変数
    int step = 0;
    float time = 0;
    float time_shuffle = 0;
    bool shuffle_jud = false;
    bool once = false;
    bool once2 = false;
    int ans_num = 0;//ボールの場所１～３
    int shuffle_num = 0;
    int rand;
    int player_choice;

    //シャッフルイラスト
    public GameObject button1_red;
    public GameObject button2_yellow;
    public GameObject button3_blue;
    public GameObject card_success;
    public GameObject card_miss1;
    public GameObject card_miss2;
    public GameObject card1;//シャッフルに使う用のハリボテ
    public GameObject card2;//  　        〃
    public GameObject card3;//　　        〃
    public GameObject yes_sprite;
    public GameObject no_sprite;
    public GameObject continue_msbox_sprite;
    public GameObject circle_;
    public GameObject cross_;

    //テキスト関連の変数
    //public GameObject textRenderObject_;        //テキストを表示するオブジェクト
    public GameObject continue_textRenderObject_;//再戦時のテキスト表示用オブジェクト
    //public Sprite[] jankenText_ = new Sprite[8];  //テキストのスプライト管理する配列
    private SpriteRenderer textspriteRenderer_;         //スプライトを変更するために使用するもの
    private SpriteRenderer continue_textspriteRenderer_;


    //音声ファイル格納用変数
    public AudioClip voice_game_shuffle_wait_;
    public AudioClip voice_game_shuffle_complete_;
    public AudioClip voice_game_shuffle_correct_;
    public AudioClip voice_game_shuffle_incorrect_;
    public AudioClip voice_game_shuffle_replaywait_;
    public AudioClip se_pinpon_;
    public AudioClip se_bubu_;
    private AudioSource audioSource_;

    //回転のために必要な変数
    Quaternion rot_face = new Quaternion(0.0f,1.0f,0.0f,0.0f);
    Quaternion rot_back = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

    //シャッフルのための変数
    Vector3 left_pos  = new Vector3(-3.25f,-5.15f,-3);
    Vector3 center_pos= new Vector3(0, -5.15f, -3);
    Vector3 right_pos = new Vector3(3.25f, -5.15f, -3);
#endregion
#region Live2D用変数
    private Live2DModelUnity live2DModel;
    private EyeBlinkMotion eyeBlink;
    private Live2DMotion[] motion;
    private L2DExpressionMotion[] expression;
    private MotionQueueManager motionMgr;
    private MotionQueueManager expressionMgr;

    private Matrix4x4 live2DCanvasPos;

    float escTime = 0;//Escキー(メインへ強制遷移)
    float resetTime = 0;

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
    public bool stop = false;//キーが押されるとストップする
    private L2DPhysics physics;
    private L2DPose pose;
#endregion

    // Use this for initialization
	void Start () {
	    audioSource_=gameObject.GetComponent<AudioSource>();
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

        #region シャッフルあて解説
        if (step == 0)
        {
            motionPlayLoop(1);
            if (once == false)
            {
                //ボールの初期位置を設定
                ans_num = UnityEngine.Random.Range(1, 4);
                if (ans_num < 1) ans_num = 1;
                if (ans_num > 3) ans_num = 3;
                //当たりと外れのカードを配置
                card_set(ans_num,true);
                //使用するシャッフルの番号を決定
                shuffle_num = UnityEngine.Random.Range(1,6);
                //shuffle_num = 5;
                Debug.Log("シャッフル番号："+shuffle_num);
                Debug.Log("どこにあるか当ててください。");
                audioSource_.PlayOneShot(voice_game_shuffle_wait_);
                Debug.Log("T.Oで次へ");
                once = true;
                hide_fakecard();
                appear_truecard();
                hide_choice();
                hide_all_button();
                hide_answer_pic();
                time = 0;
            }
            if (audioSource_.isPlaying == false)
            {
                time += Time.deltaTime;
                card_success.transform.rotation = Quaternion.Slerp(rot_face, rot_back, time / 2);
                card_miss1.transform.rotation = Quaternion.Slerp(rot_face, rot_back, time / 2);
                card_miss2.transform.rotation = Quaternion.Slerp(rot_face, rot_back, time / 2);
                if (time > 2.0f)
                {
                    hide_truecard();
                    appear_fakecard();
                    step++;
                    once = false;
                    time = 0;
                }
            }
        }
        #endregion

        #region シャッフル中
        else if(step==1){
            motionPlayLoop(1);
            if (once == false)
            {
                Debug.Log("シャッフル中･･･");
                Debug.Log("T.Oで次へ");
                once = true;
            }
            //使用するシャッフル番号にしたがってシャッフルを行う
            switch(shuffle_num){
                case 1: shuffle1(); break;
                case 2: shuffle2(); break;
                case 3: shuffle3(); break;
                case 4: shuffle4(); break;
                case 5: shuffle5(); break;
                default: shuffle1(); break;
            }
            time+=Time.deltaTime;
        }
        #endregion

        #region シャッフル完了
        else if (step == 2)
        {
            if (once == false)
            {
                motionPlayOnce_stop(0);
                Debug.Log("さて、どこに入っているでしょうか？");
                audioSource_.PlayOneShot(voice_game_shuffle_complete_);
                hide_fakecard();
                card_set(ans_num, false);
                appear_truecard();
                
                Debug.Log("ボタンで解答して次へ");
                once = true;
            }
            else if (audioSource_.isPlaying == false)
            {
                appear_button();
                #region 1を選んだ場合
                if (Input.GetKeyDown(KeyCode.A))
                {
                    player_choice = 1;
                    step = 20;
                    once = false;
                }
                #endregion

                #region 2を選んだ場合
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    player_choice = 2;
                    step = 20;
                    once = false;
                }
                #endregion

                #region 3を選んだ場合
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    player_choice = 3;
                    step = 20;
                    once = false;
                }
                #endregion
            }
        }
        #endregion
        //カードをめくる
        else if(step==20){
            hide_button();
            card_turn(player_choice);
            if(time>2.0f){
                time = 0;
                lipsync = false;
                if (player_choice == ans_num)
                {
                    audioSource_.PlayOneShot(se_pinpon_);
                    step = 21;
                }
                else
                {
                    audioSource_.PlayOneShot(se_bubu_);
                    step = 22;
                }
            }
        }
        //正解
        else if(step==21){
            if(once==false){
                //ログを出力
                _LogO.LogOutputTextfile("SHUFFLE", shuffle_num.ToString(), (player_choice - 1).ToString(), "1");
                //ログ用に正解したことを記録
                _LogWinP = 1;

                motionPlayOnce_stop(2);
                appear_answer_pic(true);
                once = true;
            }else
            if(once==true&&once2==false&&audioSource_.isPlaying==false){
                lipsync = true;
                audioSource_.PlayOneShot(voice_game_shuffle_correct_);
                once2 = true;
            }else
            if(once==true&&once2==true&&audioSource_.isPlaying==false){
                step = 3;
                once = false;
                once2 = false;
            }
        }
        //はずれ
        else if (step == 22)
        {
            if (once == false)
            {
                //ログを出力
                _LogO.LogOutputTextfile("SHUFFLE", shuffle_num.ToString(), (player_choice - 1).ToString(), "0");
                //ログ用に外れたことを記録
                _LogWinP = 0;

                motionPlayOnce_stop(4);
                appear_answer_pic(false);
                once = true;
            }else if (once == true){
                if (once2 == false && audioSource_.isPlaying == false)
                {
                    lipsync = true;
                    audioSource_.PlayOneShot(voice_game_shuffle_incorrect_);
                    once2 = true;
                }
                time += Time.deltaTime;
                card_success.transform.rotation = Quaternion.Slerp(rot_back, rot_face, time);
                if (time > 2.0f)
                {
                    time=0;
                    step = 3;
                    once = false;
                    once2 = false;
                }
            }
            
        }
        #region シャッフルあてリプレイ入力待ち
        else if (step == 3)
        {
            motionPlayLoop(3);
            time += Time.deltaTime;
            if (time > 1)
            {
                if (once == false)
                {
                    hide_all_button();
                    hide_fakecard();
                    hide_truecard();
                    hide_answer_pic();
                    appear_choice();
                    Debug.Log("もう一度プレイしますか？");
                    audioSource_.PlayOneShot(voice_game_shuffle_replaywait_);
                    Debug.Log("ボタン1→リプレイ、ボタン3→ゲームモードTOP");
                    once = true;
                }
                else if (audioSource_.isPlaying == false)
                {
                    appear_choice_button();
                    //ボタン1押下→リプレイ
                    if (Input.GetKeyDown(KeyCode.A))
                    {
                        //ログの出力
                        _LogO.LogOutputTextfile("SHUFFLEREP", "0", "SHUFFLE", _LogWinP.ToString());
                        once = false;
                        step = 0;
                        time = 0;
                    }
                    //ボタン3押下→ゲームモードTOP
                    else if (Input.GetKeyDown(KeyCode.D))
                    {
                        //ログの出力
                        _LogO.LogOutputTextfile("SHUFFLEREP", "0", "BACK", _LogWinP.ToString());
                        once = false;
                        SceneManager.LoadScene("game");
                        time = 0;
                    }
                }
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
            if (lipsync==true)
            {
                live2DModel.setParamFloat("PARAM_MOUTH_FORM", 0);
                live2DModel.setParamFloat("PARAM_MOUTH_OPEN_Y", volume * scaleVolume);
            }
            #endregion

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

#region シャッフルゲーム用関数
    //ボタンを隠す
    void hide_button()
    {
        Vector3 pos1 = button1_red.transform.position;
        Vector3 pos2 = button2_yellow.transform.position;
        Vector3 pos3 = button3_blue.transform.position;
        pos1.z=3;
        pos2.z=3;
        pos3.z=3;
        if(player_choice==1){
            pos1.z = -3;
        }
        else if (player_choice == 2)
        {
            pos2.z = -3;
        }
        else
        {
            pos3.z = -3;
        }
        button1_red.transform.position = pos1;
        button2_yellow.transform.position = pos2;
        button3_blue.transform.position = pos3;
    }
    //全てのボタンを隠す
    void hide_all_button() {
        Vector3 pos1 = button1_red.transform.position;
        Vector3 pos2 = button2_yellow.transform.position;
        Vector3 pos3 = button3_blue.transform.position;
        pos1.z = 3;
        pos2.z = 3;
        pos3.z = 3;
        button1_red.transform.position = pos1;
        button2_yellow.transform.position = pos2;
        button3_blue.transform.position = pos3;
    }
    //ボタンを前面に持ってくる
    void appear_button()
    {
        Vector3 pos1 = button1_red.transform.position;
        Vector3 pos2 = button2_yellow.transform.position;
        Vector3 pos3 = button3_blue.transform.position;
        pos1.z = -3;
        pos2.z = -3;
        pos3.z = -3;
        button1_red.transform.position = pos1;
        button2_yellow.transform.position = pos2;
        button3_blue.transform.position = pos3;
    }
    //ハリボテのカードを隠す
    void hide_fakecard()
    {
        Vector3 pos1 = card1.transform.position;
        Vector3 pos2 = card2.transform.position;
        Vector3 pos3 = card3.transform.position;
        pos1.z = 3;
        pos2.z = 3;
        pos3.z = 3;
        card1.transform.position = pos1;
        card2.transform.position = pos2;
        card3.transform.position = pos3;
    }
    //ハリボテのカードを前面にもってくる
    void appear_fakecard()
    {
        Vector3 pos1 = card1.transform.position;
        Vector3 pos2 = card2.transform.position;
        Vector3 pos3 = card3.transform.position;
        pos1.x = -3.25f;
        pos2.x = 0;
        pos3.x = 3.25f;
        pos1.z = -3;
        pos2.z = -3;
        pos3.z = -3;
        card1.transform.position = pos1;
        card2.transform.position = pos2;
        card3.transform.position = pos3;
    }

    //カードを隠す
    void hide_truecard()
    {
        Vector3 pos1 = card_success.transform.position;
        Vector3 pos2 = card_miss1.transform.position;
        Vector3 pos3 = card_miss2.transform.position;
        pos1.z = 3;
        pos2.z = 3;
        pos3.z = 3;
        card_success.transform.position = pos1;
        card_miss1.transform.position = pos2;
        card_miss2.transform.position = pos3;
    }
    //カードを前面に持ってくる
    void appear_truecard()
    {
        Vector3 pos1 = card_success.transform.position;
        Vector3 pos2 = card_miss1.transform.position;
        Vector3 pos3 = card_miss2.transform.position;
        pos1.z = -3;
        pos2.z = -3;
        pos3.z = -3;
        card_success.transform.position = pos1;
        card_miss1.transform.position = pos2;
        card_miss2.transform.position = pos3;
    }

    //当たりと外れのカードを配置(num=当たりカードの位置,face=表をこちらに向けるか) 
    void card_set(int num,bool face)
    {
        Vector3 pos1 = card_success.transform.position;
        Vector3 pos2 = card_miss1.transform.position;
        Vector3 pos3 = card_miss2.transform.position;
        var rot1 = card_success.transform.rotation;
        var rot2 = card_miss1.transform.rotation;
        var rot3 = card_miss2.transform.rotation;
        if(num==1){
            pos1.x = -3.25f;
            pos2.x = 0;
            pos3.x = 3.25f;
        }
        else if (num == 2)
        {
            pos2.x = -3.25f;
            pos1.x = 0;
            pos3.x = 3.25f;
        }
        else if(num==3)
        {
            pos2.x = -3.25f;
            pos3.x = 0;
            pos1.x = 3.25f;
        }
        card_success.transform.position = pos1;
        card_miss1.transform.position = pos2;
        card_miss2.transform.position = pos3;

        if (face == true)
        {
            rot1.y = 180;
            rot2.y = 180;
            rot3.y = 180;
        }
        else
        {
            rot1.y = 0;
            rot2.y = 0;
            rot3.y = 0;
        }
        card_success.transform.rotation = rot1;
        card_miss1.transform.rotation = rot2;
        card_miss2.transform.rotation = rot3;
    }

    //カードをめくる
    void card_turn(int num)
    {
        time += Time.deltaTime;
        if (num == ans_num)
        {
            card_success.transform.rotation = Quaternion.Slerp(rot_back, rot_face, time / 2);
        }
        else
        {
            if (ans_num == 3)
            {
                if (num == 1)
                {
                    card_miss1.transform.rotation = Quaternion.Slerp(rot_back, rot_face, time / 2);
                }
                else
                {
                    card_miss2.transform.rotation = Quaternion.Slerp(rot_back, rot_face, time / 2);
                }
            }
            else
            {
                if (num == 3)
                {
                    card_miss2.transform.rotation = Quaternion.Slerp(rot_back, rot_face, time / 2);
                }
                else
                {
                    card_miss1.transform.rotation = Quaternion.Slerp(rot_back, rot_face, time / 2);
                }
            }
        }
        
    }

    //○と×の画像表示
    void appear_answer_pic(bool true_)
    {
        Vector3 pos1 = circle_.transform.position;
        Vector3 pos2 = cross_.transform.position;
        if(player_choice==1){
            pos1.x = -3.25f;
            pos2.x = -3.25f;
        }else if(player_choice==2){
            pos1.x = 0;
            pos2.x = 0;
        }else if(player_choice==3){
            pos1.x = 3.25f;
            pos2.x = 3.25f;
        }
        if(true_==true){
            pos1.z = -4;
            pos2.z = 4;
        }else
        if(true_==false){
            pos1.z = 4;
            pos2.z = -4;
        }
        circle_.transform.position = pos1;
        cross_.transform.position = pos2;
    }
    void hide_answer_pic()
    {
        Vector3 pos1 = circle_.transform.position;
        Vector3 pos2 = cross_.transform.position;
        pos1.z = 3;
        pos2.z = 3;
        circle_.transform.position = pos1;
        cross_.transform.position = pos2;
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
    #endregion

    #region シャッフルパターン
    //シャッフル番号１
    void shuffle1()
    {
        Vector3 pos1 = card1.transform.position;
        Vector3 pos2 = card2.transform.position;
        Vector3 pos3 = card3.transform.position;

        //Debug.Log("1←→2 123→213");
        if(time<2.1f){
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            if(time_shuffle<1)time_shuffle+=Time.deltaTime/2;
            pos1 = Vector3.Lerp(left_pos, center_pos,time_shuffle);
            pos2 = Vector3.Lerp(center_pos,left_pos,time_shuffle);
        }
        //Debug.Log("1←→3 213→231");
        if (2.1f<=time&&time<3.2f)
        {
            if (shuffle_jud == true)
            {
                shuffle_jud = false;
                time_shuffle = 0;
            }
            time_shuffle+=Time.deltaTime;
            pos1 = Vector3.Lerp(center_pos, right_pos, time_shuffle);
            pos3 = Vector3.Lerp(right_pos, center_pos, time_shuffle);
        }
        //Debug.Log("1←→2 231→132");
        if (3.2f<=time&&time<4.3f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle+=Time.deltaTime;
            pos2 = Vector3.Lerp(left_pos, right_pos, time_shuffle);
            pos1 = Vector3.Lerp(right_pos, left_pos, time_shuffle);
        }
        //Debug.Log("3←→2 132→123");
        if (4.3f<=time&&time<4.9f)
        {
            if (shuffle_jud == true)
            {
                shuffle_jud = false;
                time_shuffle = 0;
            }
            time_shuffle+=Time.deltaTime*2;
            pos3 = Vector3.Lerp(center_pos, right_pos, time_shuffle);
            pos2 = Vector3.Lerp(right_pos, center_pos, time_shuffle);
        }
        //Debug.Log("1←→3 123→321");
        if (4.9f<=time&&time<5.5f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle+=Time.deltaTime*3;
            pos1 = Vector3.Lerp(left_pos, right_pos, time_shuffle);
            pos3 = Vector3.Lerp(right_pos, left_pos, time_shuffle);
        }
        if (time >=6)
        {
            if (ans_num == 1) ans_num = 3;
            else if (ans_num == 3) ans_num = 1;
            Debug.Log("ans_num="+ans_num);
            Debug.Log("シャッフル終了");
            time = 0;
            step++;
            time_shuffle = 0;
            once = false;
            shuffle_jud = false;
        }
        card1.transform.position = pos1;
        card2.transform.position = pos2;
        card3.transform.position = pos3;
    }

    //シャッフル番号2
    void shuffle2()
    {
        Vector3 posA = card1.transform.position;
        Vector3 posB = card2.transform.position;
        Vector3 posC = card3.transform.position;

        //Debug.Log("1←→3 ABC→CBA");
        if (time < 2.1f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            if (time_shuffle < 1) time_shuffle += Time.deltaTime / 2;
            posA = Vector3.Lerp(left_pos, right_pos, time_shuffle);
            posC = Vector3.Lerp(right_pos, left_pos, time_shuffle);
        }
        //Debug.Log("2←→3 CBA→CAB");
        if (2.1f <= time && time < 3.2f)
        {
            if (shuffle_jud == true)
            {
                shuffle_jud = false;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime;
            posB = Vector3.Lerp(center_pos, right_pos, time_shuffle);
            posA = Vector3.Lerp(right_pos, center_pos, time_shuffle);
        }
        //Debug.Log("1←→2 CAB→ACB");
        if (3.2f <= time && time < 4.3f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime;
            posC = Vector3.Lerp(left_pos, center_pos, time_shuffle);
            posA = Vector3.Lerp(center_pos, left_pos, time_shuffle);
        }
        //Debug.Log("3←→1 ACB→BCA");
        if (4.3f <= time && time < 4.9f)
        {
            if (shuffle_jud == true)
            {
                shuffle_jud = false;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime * 2;
            posA = Vector3.Lerp(left_pos, right_pos, time_shuffle);
            posB = Vector3.Lerp(right_pos, left_pos, time_shuffle);
        }
        //Debug.Log("1←→3 BCA→ACB");
        if (4.9f <= time && time < 5.5f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime * 3;
            posB = Vector3.Lerp(left_pos, right_pos, time_shuffle);
            posA = Vector3.Lerp(right_pos, left_pos, time_shuffle);
        }
        if (time >= 6)
        {
            if (ans_num == 2) ans_num = 3;
            else if (ans_num == 3) ans_num = 2;
            Debug.Log("ans_num=" + ans_num);
            Debug.Log("シャッフル終了");
            time = 0;
            step++;
            time_shuffle = 0;
            once = false;
            shuffle_jud = false;
        }
        card1.transform.position = posA;
        card2.transform.position = posB;
        card3.transform.position = posC;
    }

    //シャッフル番号3
    void shuffle3()
    {
        Vector3 posA = card1.transform.position;
        Vector3 posB = card2.transform.position;
        Vector3 posC = card3.transform.position;

        //Debug.Log("2←→3 ABC→ACB");
        if (time < 1.1f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime;
            posB = Vector3.Lerp(center_pos, right_pos, time_shuffle);
            posC = Vector3.Lerp(right_pos, center_pos, time_shuffle);
        }
        //Debug.Log("1←→2 ACB→CAB");
        if (1.1f <= time && time < 1.7f)
        {
            if (shuffle_jud == true)
            {
                shuffle_jud = false;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime*2;
            posC = Vector3.Lerp(center_pos, left_pos, time_shuffle);
            posA = Vector3.Lerp(left_pos, center_pos, time_shuffle);
        }
        //Debug.Log("2←→3 CAB→CBA");
        if (1.7f <= time && time < 2.8f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime;
            posB = Vector3.Lerp(right_pos, center_pos, time_shuffle);
            posA = Vector3.Lerp(center_pos, right_pos, time_shuffle);
        }
        //Debug.Log("3←→1 CBA→ABC");
        if (2.8f <= time && time < 3.4f)
        {
            if (shuffle_jud == true)
            {
                shuffle_jud = false;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime * 2;
            posC = Vector3.Lerp(left_pos, right_pos, time_shuffle);
            posA = Vector3.Lerp(right_pos, left_pos, time_shuffle);
        }
        //Debug.Log("2←→3 ABC→ACB");
        if (3.4f <= time && time < 4.5f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime;
            posB = Vector3.Lerp(center_pos, right_pos, time_shuffle);
            posC = Vector3.Lerp(right_pos, center_pos, time_shuffle);
        }
        if (time >= 5)
        {
            if (ans_num == 2) ans_num = 3;
            else if (ans_num == 3) ans_num = 2;
            Debug.Log("ans_num=" + ans_num);
            Debug.Log("シャッフル終了");
            time = 0;
            step++;
            time_shuffle = 0;
            once = false;
            shuffle_jud = false;
        }
        card1.transform.position = posA;
        card2.transform.position = posB;
        card3.transform.position = posC;
    }

    //シャッフル番号4
    void shuffle4()
    {
        Vector3 posA = card1.transform.position;
        Vector3 posB = card2.transform.position;
        Vector3 posC = card3.transform.position;

        //Debug.Log("1:1←→2 ABC→BAC");
        if (time < 1.1f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime;
            posB = Vector3.Lerp(center_pos, left_pos, time_shuffle);
            posA = Vector3.Lerp(left_pos, center_pos, time_shuffle);
        }
        //Debug.Log("2:1→2 2→3 3→1 BAC→CBA");
        if (1.1f <= time && time < 2.2f)
        {
            if (shuffle_jud == true)
            {
                shuffle_jud = false;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime ;
            posA = Vector3.Lerp(center_pos, right_pos, time_shuffle);
            posB = Vector3.Lerp(left_pos, center_pos, time_shuffle);
            posC = Vector3.Lerp(right_pos, left_pos, time_shuffle);
        }
        //Debug.Log("3:1←→3 CBA→ABC");
        if (2.2f <= time && time < 2.8f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime*2;
            posA = Vector3.Lerp(right_pos, left_pos, time_shuffle);
            posC = Vector3.Lerp(left_pos, right_pos, time_shuffle);
        }
        //Debug.Log("4:3←→2 ABC→ACB");
        if (2.8f <= time && time < 3.4f)
        {
            if (shuffle_jud == true)
            {
                shuffle_jud = false;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime * 2;
            posB = Vector3.Lerp(center_pos, right_pos, time_shuffle);
            posC = Vector3.Lerp(right_pos, center_pos, time_shuffle);
        }
        //Debug.Log("5:1→3 2→1 3→2 ACB→CBA");
        if (3.4f <= time && time < 3.9f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime*2;
            posC= Vector3.Lerp(center_pos, left_pos, time_shuffle);
            posB = Vector3.Lerp(right_pos, center_pos, time_shuffle);
            posA = Vector3.Lerp(left_pos, right_pos, time_shuffle);
        }
        if (time >= 4.5f)
        {
            if (ans_num == 1) ans_num = 3;
            else if (ans_num == 3) ans_num = 1;
            Debug.Log("ans_num=" + ans_num);
            Debug.Log("シャッフル終了");
            time = 0;
            step++;
            time_shuffle = 0;
            once = false;
            shuffle_jud = false;
        }
        card1.transform.position = posA;
        card2.transform.position = posB;
        card3.transform.position = posC;
    }

    //シャッフル番号5
    void shuffle5()
    {
        Vector3 posA = card1.transform.position;
        Vector3 posB = card2.transform.position;
        Vector3 posC = card3.transform.position;

        //Debug.Log("1:1←→3 ABC→CBA");
        if (time < 1.1f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime;
            posC = Vector3.Lerp(right_pos, left_pos, time_shuffle);
            posA = Vector3.Lerp(left_pos, right_pos, time_shuffle);
        }
        //Debug.Log("2:1→3 2→1 3→2 CBA→BAC");
        if (1.1f <= time && time < 1.7f)
        {
            if (shuffle_jud == true)
            {
                shuffle_jud = false;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime*2;
            posA = Vector3.Lerp(right_pos, center_pos, time_shuffle);
            posB = Vector3.Lerp(center_pos, left_pos, time_shuffle);
            posC = Vector3.Lerp(left_pos, right_pos, time_shuffle);
        }
        //Debug.Log("3:1→3 2→1 3→2  BAC→ACB");
        if (1.7f <= time && time < 2.3f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime * 2;
            posA = Vector3.Lerp(center_pos, left_pos, time_shuffle);
            posC = Vector3.Lerp( right_pos,center_pos, time_shuffle);
            posB = Vector3.Lerp(left_pos, right_pos, time_shuffle);
        }
        //Debug.Log("4:3←→1 ACB→BCA");
        if (2.3f <= time && time < 2.9f)
        {
            if (shuffle_jud == true)
            {
                shuffle_jud = false;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime * 2;
            posA = Vector3.Lerp(left_pos, right_pos, time_shuffle);
            posB = Vector3.Lerp(right_pos, left_pos, time_shuffle);
        }
        //Debug.Log("5:3→1 2→3 1→2 BCA→ABC");
        if (2.9f <= time && time < 4.0f)
        {
            if (shuffle_jud == false)
            {
                shuffle_jud = true;
                time_shuffle = 0;
            }
            time_shuffle += Time.deltaTime;
            posC = Vector3.Lerp(center_pos, right_pos, time_shuffle);
            posB = Vector3.Lerp(left_pos, center_pos, time_shuffle);
            posA = Vector3.Lerp(right_pos, left_pos, time_shuffle);
        }
        if (time >= 4.5f)
        {
            Debug.Log("ans_num=" + ans_num);
            Debug.Log("シャッフル終了");
            time = 0;
            step++;
            time_shuffle = 0;
            once = false;
            shuffle_jud = false;
        }
        card1.transform.position = posA;
        card2.transform.position = posB;
        card3.transform.position = posC;
    }

    #endregion

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
            case 9:
                if (motionMgr.startMotion(motion[9]) == -1) Debug.Log("モーションを再生できません");
                else motionMgr.startMotion(motion[9]);
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
}
