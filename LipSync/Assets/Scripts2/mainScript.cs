using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;//add
using System.IO;//add
using live2d;//add
using live2d.framework;

public class mainScript : MonoBehaviour {
    int mode = 0;//0=接客待機モード,1=待機モード,2=スクリーンセーバー
    bool once = false;//1回だけ行いたい処理に使用
    bool once2 = false;
    float time = 0;//T.O用に使用
    int step = 0;
    bool lipsync = true;

    int move_direction_ = 1;//テキストの移動方向
    
    //シーンの非同期読み込み用
    private AsyncOperation Async_product_;

    //画像表示用オブジェクト格納用変数
    public GameObject msbox_;
    public GameObject select_product_;
    public GameObject select_quiz_;
    public GameObject select_game_;
    public GameObject mainT_;
    public GameObject ScreenSaverT_;
    public GameObject black_backG_;
    public GameObject moveScreenSaverT_;

    //音声ファイル格納用変数
    public AudioClip voice_main_mainmenu_;
    public AudioClip voice_main_wait_;
    public AudioClip voice_main_recommend_;
    public AudioClip voice_main_quiz_;
    public AudioClip voice_main_game_;
    public AudioClip se_kettei_;

    private AudioSource audioSource_;

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
    /*
    IEnumerator LoadScene()
    {
        //商品オススメシーンの非同期読み込み
        Async_product_ = SceneManager.LoadSceneAsync("products");
        Async_product_.allowSceneActivation = false;
        Debug.Log("ロード状況:"+Async_product_.progress);
        while (Async_product_.progress>=0.9f)
        {
            Debug.Log("SceneLoadComplete");
            yield return new WaitForSeconds(0);
        }
        yield return new WaitForSeconds(0);
    }*/

    //ログ出力
    LogOutput LogO=new LogOutput();


    // Use this for initialization
	void Start () {
        mode = 0;//0=接客待機モード,1=待機モード,2=スクリーンセーバー
        once = false;//1回だけ行いたい処理に使用
        time = 0;//T.O用に使用
        step = 0;
        lipsync = true;
        move_direction_ = 1;//テキストの移動方向
	    audioSource_=gameObject.GetComponent<AudioSource>();
        //StartCoroutine("LoadScene");
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
        //モーションのロード
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
	void Update () {
        
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

        switch (mode)
        {
            case 0: mode_main();
                screenSaverText(false);
                break;
            case 1: mode_wait1();
                screenSaverText(true);
                break;
            case 2: mode_wait2();
                screenSaverText(true);
                break;
            case 3: mode_wait3();
                screenSaverText(false);
                break;
            default: break;
        }
        #region Live2Dその2
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
        //開閉0～1　変形-1～1
        //Live2Dモデルの口のY座標を変更する(リップシンクを行う)
        if (lipsync)
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

    //接客待機モード
    void mode_main()
    {
        motionPlayLoop(3);
        if (step == 0)
        {
            //最初に1回だけ挨拶
            if (once == false&&audioSource_.isPlaying==false)
            {
                motionMgr.stopAllMotions();
                //挨拶処理ーーーーーーーーーーーーーー
                Debug.Log("接客待機モード挨拶");
                audioSource_.PlayOneShot(voice_main_mainmenu_);
                once = true;
                //選択肢を表示
                select_object_(true);
            }

            //Aキーが入力されたら商品オススメモード
            if (Input.GetKeyDown(KeyCode.A))
            {
                //Aキーが入力されたら商品おすすめモード
                audioSource_.Stop();
                lipsync = false;
                //ログを出力
                LogO.LogOutputTextfile("TOP","0", "OSUSUME", "0");
                //説明＋会釈
                once = false;
                step = 1;
            }else if (Input.GetKeyDown(KeyCode.S)){
                //Sキーが入力されたらクイズモード
                audioSource_.Stop();
                lipsync = false;
                //ログを出力
                LogO.LogOutputTextfile("TOP", "0", "QUIZ", "0");
                //説明＋笑顔
                once = false;
                step = 2;
            }else if (Input.GetKeyDown(KeyCode.D)){
                //Dキーが入力されたらゲームモード                
                audioSource_.Stop();
                lipsync = false;
                //ログを出力
                LogO.LogOutputTextfile("TOP", "0", "GAME", "0");
                //説明＋笑顔
                once = false;
                step = 3;
            }
            //T.0処理
            time+=Time.deltaTime;
            if (time > 20&&step==0)
            {
                time = 0;
                once = false;
                mode = 1;
                step = 0;
                //選択肢を非表示
                select_object_(false);
            }
        }else if(step==1){
            if (once == false && audioSource_.isPlaying == false)
            {
                hide_mode_sprite(1);
                audioSource_.PlayOneShot(se_kettei_);
                
                once = true;
            }
            if (once == true &&once2==false&& audioSource_.isPlaying == false)
            {
                lipsync = true;
                audioSource_.PlayOneShot(voice_main_recommend_);
                once2 = true;
            }
            if (audioSource_.isPlaying == false)
            {
                once = false;
                once2 = false;
                Debug.Log("商品オススメモードに移行");
                SceneManager.LoadScene("products");
                //SceneManager.LoadSceneAsync("products");
                //Async_product_.allowSceneActivation = true;
            }
        }
        else if (step == 2)
        {
            if (once == false && audioSource_.isPlaying == false)
            {
                hide_mode_sprite(2);
                audioSource_.PlayOneShot(se_kettei_);
                
                once = true;
            }
            if(once==true&&once2==false&&audioSource_.isPlaying==false){
                lipsync = true;
                audioSource_.PlayOneShot(voice_main_quiz_);
                once2 = true;
            }
            if (audioSource_.isPlaying == false)
            {
                once = false;
                once2 = false;
                Debug.Log("クイズモードに移行");
                SceneManager.LoadScene("quiz");
            }
        }
        else if (step == 3)
        {
            if (once == false && audioSource_.isPlaying == false)
            {
                hide_mode_sprite(3);
                audioSource_.PlayOneShot(se_kettei_);
                
                once = true;
            }
            if(once==true&&once2==false&&audioSource_.isPlaying==false){
                lipsync = true;
                audioSource_.PlayOneShot(voice_main_game_);
                once2 = true;
            }
            if (audioSource_.isPlaying == false)
            {
                once = false;
                once2 = false;
                Debug.Log("ゲームモードに移行");
                SceneManager.LoadScene("game");
            }
        }     
    }

    //待機モード(挨拶)
    void mode_wait1()
    {
        motionPlayLoop(1);
        //最初に1回だけ挨拶
        if (once == false)
        {
            //挨拶処理ーーーーーーーーーーーーーー
            Debug.Log("待機モード挨拶");
            audioSource_.PlayOneShot(voice_main_wait_);
            motionPlayOnce_stop(0);
            once = true;
        }

        //センサに反応があれば接客待機モードに移行
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            audioSource_.Stop();
            mode = 0;
            once = false;
            time = 0;
        }
        //T.0処理
        time+=Time.deltaTime;
        if (time > 20)
        {
            time = 0;
            once = false;
            mode = 2;
        }
    }

    //待機モード(アイドルモーション)
    void mode_wait2()
    {
        motionPlayLoop(1);
        if (once == false)
        {
            motionPlayOnce_stop(2);
            Debug.Log("スクリーンセーバー");
            once = true;
        }
        //センサに反応があれば接客待機モードに移行
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            mode = 0;
            time = 0;
            once = false;
        }
        //T.0処理
        time+=Time.deltaTime;
        if (time > 20)
        {
            time = 0;
            once = false;
            mode = 3;
        }
    }
    //待機モード(黒背景スクリーンセーバー)
    void mode_wait3()
    {
        if(once==false){
            blackBackG(true);
            moveText_appear(true);
            once = true;
        }
        moveText();
        //センサに反応があれば接客待機モードに移行
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            blackBackG(false);
            moveText_appear(false);
            mode = 0;
            time = 0;
            once = false;
        }
        //T.0処理
        time+=Time.deltaTime;
        if (time > 255)
        {
            blackBackG(false);
            moveText_appear(false);
            time = 0;
            once = false;
            mode = 1;
        }
    }


    //選択肢を表示
    void select_object_(bool appear)
    {
        Vector3 pos1 = msbox_.transform.position;
        Vector3 pos2 = select_product_.transform.position;
        Vector3 pos3 = select_quiz_.transform.position;
        Vector3 pos4 = select_game_.transform.position;
        Vector3 pos5 = mainT_.transform.position;
        if (appear == true)
        {
            pos1.z = -3;
            pos2.z = -3;
            pos3.z = -3;
            pos4.z = -3;
            pos5.z = -4;
        }
        else
        {
            pos1.z = 3;
            pos2.z = 3;
            pos3.z = 3;
            pos4.z = 3;
            pos5.z = 3;
        }
        msbox_.transform.position = pos1;
        select_product_.transform.position = pos2;
        select_quiz_.transform.position = pos3;
        select_game_.transform.position = pos4;
        mainT_.transform.position = pos5;
    }
    //スクリーンセーバー時のテキストを表示
    void screenSaverText(bool appear)
    {
        Vector3 pos = ScreenSaverT_.transform.position;
        if (appear)
        {
            pos.z = -4;
        }
        else
        {
            pos.z = 5;
        }
        ScreenSaverT_.transform.position = pos;
    }
    //黒背景を表示
    void blackBackG(bool appear)
    {
        Vector3 pos = black_backG_.transform.position;
        if (appear)
        {
            pos.z = -1;
        }
        else
        {
            pos.z = 5;
        }
        black_backG_.transform.position = pos;
    }
    //動くテキストを表示
    void moveText_appear(bool appear)
    {
        Vector3 pos = moveScreenSaverT_.transform.position;
        if (appear)
        {
            pos.x = 0;
            pos.y = 0;
            pos.z = -4;
        }
        else
        {
            pos.z = 5;
        }
        moveScreenSaverT_.transform.position = pos;
    }
    //スクリーンセーバーのテキストを動かす
    void moveText()
    {
        Vector3 pos = moveScreenSaverT_.transform.position;
        //右上
        if(move_direction_==1){
            pos.x+=Time.deltaTime;
            pos.y += Time.deltaTime;
            if(pos.x>3){
                move_direction_ = 4;
            }else if(pos.y>9.6){
                move_direction_ = 2;
            }
        //右下
        }else if(move_direction_==2){
            pos.x += Time.deltaTime;
            pos.y -= Time.deltaTime;
            if (pos.x > 3)
            {
                move_direction_ = 3;
            }
            else if (pos.y < -9.6)
            {
                move_direction_ = 1;
            }
        //左下
        }else if(move_direction_==3){
            pos.x -= Time.deltaTime;
            pos.y -= Time.deltaTime;
            if (pos.x < -3)
            {
                move_direction_ = 2;
            }
            else if (pos.y < -9.6)
            {
                move_direction_ = 4;
            }
        //左上
        }else if(move_direction_==4){
            pos.x-=Time.deltaTime;
            pos.y += Time.deltaTime;
            if (pos.x <-3)
            {
                move_direction_ = 1;
            }
            else if (pos.y >9.6)
            {
                move_direction_ = 3;
            }
        }
        moveScreenSaverT_.transform.position = pos;
    }
    //選択されなかったモード画像を
    void hide_mode_sprite(int mode)
    {
        Vector3 pos1 = select_product_.transform.position;
        Vector3 pos2 = select_quiz_.transform.position;
        Vector3 pos3 = select_game_.transform.position;
        if(mode==1){
            pos2.z = 3;
            pos3.z = 3;
        }else if(mode==2){
            pos1.z = 3;
            pos3.z = 3;
        }else if(mode==3)
        {
            pos1.z = 3;
            pos2.z = 3;
        }
        select_product_.transform.position = pos1;
        select_quiz_.transform.position = pos2;
        select_game_.transform.position = pos3;
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

}
