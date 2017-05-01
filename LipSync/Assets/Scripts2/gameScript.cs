using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;//add
using System.IO;//add
using live2d;//add
using live2d.framework;

public class gameScript : MonoBehaviour {
    int step = 0;
    bool once = false;
    float escTime = 0;//Escキー(メインへ強制遷移)
    float resetTime = 0;

    public GameObject select_janken_;
    public GameObject select_mainmenu_;
    public GameObject select_shuffle_;

    //音声ファイル格納用変数
    public AudioClip voice_game_top_;
    public AudioClip voice_game_janken_top_;
    public AudioClip voice_game_shuffle_top_;
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
        bool lipsync = true;//リップシンクするかどうか

        //目パチ用変数
        private int blink_interval = 3500;//インターバルの間隔単位(ミリ秒)

        public TextAsset mocFile;
        public TextAsset modelJson;
        public Texture2D[] textureFiles;
        public TextAsset[] motionFiles=new TextAsset[1];
        public TextAsset physicsFile;
        public TextAsset poseFile;
        public TextAsset[] expressionFiles;
        //public bool stop = false;//キーが押されるとストップする
        private L2DPhysics physics;
        private L2DPose pose;
    #endregion

        LogOutput LogO=new LogOutput();

	// Use this for initialization
	void Start () {
        
        audioSource_ = gameObject.GetComponent<AudioSource>();
        resetTime = 0;
        #region Live2D用記述
            Live2D.init();

            //モデル設定ファイルの読み込み
            ModelSettingJson modelSettingJson = new ModelSettingJson(modelJson.text);

            //モデルのロード
            live2DModel = Live2DModelUnity.loadModel(mocFile.bytes);
            //live2DModel = Live2DModelUnity.loadModel(modelSettingJson.GetModelFile());
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
        //Debug.Log("resetTime="+resetTime);
        if(resetTime>300){
            resetTime = 0;
            SceneManager.LoadScene("main");
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Escape))
        {
            resetTime = 0;
        }
        #endregion

        if (step == 0)
        {
            motionPlayLoop(0);
            if (once == false)
            {
                Debug.Log("どちらのゲームにしますか？");
                audioSource_.PlayOneShot(voice_game_top_);
                once = true;
            }
            //ボタン1押下→ゲームモード(ジャンケンへ)
            if (Input.GetKeyDown(KeyCode.A))
            {
                //ログを出力
                LogO.LogOutputTextfile("GAME", "0", "JYANKEN", "0");

                audioSource_.Stop();
                lipsync = false;
                audioSource_.PlayOneShot(se_kettei_);
                hide_select(1);
                once = false;
                step = 1;
            }
            //ボタン3押下→ゲームモード(シャッフルあてへ)
            else if (Input.GetKeyDown(KeyCode.D))
            {
                //ログを出力
                LogO.LogOutputTextfile("GAME", "0", "SHUFFLE", "0");

                audioSource_.Stop();
                lipsync = false;
                audioSource_.PlayOneShot(se_kettei_);
                hide_select(3);
                once = false;
                step = 2;
            }
            //ボタン2押下→メインメニューへ
            else if (Input.GetKeyDown(KeyCode.S))
            {
                //ログを出力
                LogO.LogOutputTextfile("GAME", "0", "BACK", "0");

                audioSource_.Stop();
                hide_select(2);
                once = false;
                SceneManager.LoadScene("main");
            }
        }else if(step==1){
            if (once == false && audioSource_.isPlaying == false)
            {
                lipsync = true;
                audioSource_.PlayOneShot(voice_game_janken_top_);
                once = true;
            }
            if (audioSource_.isPlaying == false)
            {
                Debug.Log("じゃんけんですね。");
                once = false;
                SceneManager.LoadScene("game1");
            }
        }else if(step==2){
            if (once == false && audioSource_.isPlaying == false)
            {
                lipsync = true;
                audioSource_.PlayOneShot(voice_game_shuffle_top_);
                once = true;
            }
            if (audioSource_.isPlaying == false)
            {
                Debug.Log("シャッフルあてですね。");
                once = false;
                SceneManager.LoadScene("game2");
            }
        }

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
            if (lipsync == true)
            {
                live2DModel.setParamFloat("PARAM_MOUTH_FORM",0);
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
        #endregion
	}

    void hide_select(int num)
    {
        Vector3 pos1 = select_janken_.transform.position;
        Vector3 pos2 = select_mainmenu_.transform.position;
        Vector3 pos3 = select_shuffle_.transform.position;
        if(num==2||num==3){
            pos1.z = 3;
        }
        if (num == 1 || num == 3)
        {
            pos2.z = 3;
        }
        if (num == 1 || num == 2)
        {
            pos3.z = 3;
        }
        select_janken_.transform.position=pos1;
        select_mainmenu_.transform.position = pos2;
        select_shuffle_.transform.position = pos3;
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
