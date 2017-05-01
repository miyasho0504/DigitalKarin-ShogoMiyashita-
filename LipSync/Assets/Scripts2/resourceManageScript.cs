using UnityEngine;
using System.Collections;
using System;//add
using System.IO;//add

public class resourceManageScript : MonoBehaviour {

    #region クイズボイス用変数
    //基本ボイスが保存されている場所のパス
    string voiceFolderNameBasic = "Voices/quiz/basic2/";
    //前説文ボイスが保存されている場所のパス
    string voiceFolderNameQuizBeforeInfo = "Voices/quiz/beforeInfo/";
    //問題文ボイスが保存されている場所のパス
    string voiceFolderNameQuestion = "Voices/quiz/question/";
    //解説文ボイスが保存されている場所のパス
    string voiceFolderNameQuizInfo = "Voices/quiz/Info/";

    private AudioSource audioSource_;
    public AudioClip[] voice_quiz_basic_;
    public AudioClip[] voice_quiz_beforeinfo_;
    public AudioClip[] voice_quiz_question_;
    public AudioClip[] voice_quiz_info_;
    #endregion

    #region クイズ画像用変数
        //システムメッセージ画像
        public Sprite[] quizSystemSprites = new Sprite[14];
        //前説文画像
        public Sprite[] quizBeforeInfoSprites = new Sprite[31];
        //問題文画像
        public Sprite[] questionTextSprites = new Sprite[31];
        //選択肢画像
        public Sprite[] questionAnsTextSprites = new Sprite[91];
        //解説文画像
        public Sprite[] quizInfoSprites = new Sprite[31];
    #endregion

    #region 商品おすすめボイス用変数
        //基本ボイスが保存されている場所のパス
        string voiceFolderNameProductBasic = "Voices/products/basic2/";
        //基本ボイスの名前
        string[] basicVoiceName = new string[9]
    {
        "product-top",
        "product-Q1",
        "product-Q2Q3",
        "product-AnsA",
        "product-AnsB",
        "product-AnsC",
        "product-understand",
        "product-useful",
        "product-finish",
    };
        //質問ボイスが保存されている場所のパス
        string voiceFolderNameProductQuestion = "Voices/products/question/";
        //商品解説ボイスが保存されている場所のパス
        string voiceFolderNameProductInfo = "Voices/products/Info/";
        public AudioClip[] voice_product_basic_;
        public AudioClip[] voice_product_question_;
        public AudioClip[] voice_product_info_;
    #endregion

    #region 商品おすすめ画像ファイル
        //質問画像
        public Sprite[] productquestionTextSprites = new Sprite[14];
        //選択肢画像
        public Sprite[] productquestionAnsTextSprites = new Sprite[40];
        //商品画像
        public Sprite[] productSprites = new Sprite[28];
        //商品説明画像
        public Sprite[] productInfoSprites = new Sprite[28];
    #endregion

    // Use this for initialization
	void Awake () {
        audioSource_ = gameObject.GetComponent<AudioSource>();
        Texture2D texture_;

        #region　クイズボイス読み込み
        //基本ボイスの読み込み
        voice_quiz_basic_ = new AudioClip[21];
        for (int i = 0; i < voice_quiz_basic_.Length; i++)
        {
            voice_quiz_basic_[i] = Resources.Load<AudioClip>(voiceFolderNameBasic + "quiz_basic" + i);
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
        #endregion クイズボイス読み込み終わり
        #region クイズ画像読み込み
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
            //Debug.Log(quizSystemSprites[i]);
        }

        //前説文画像
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
            //Debug.Log(quizBeforeInfoSprites[i]);
        }
        //問題文画像
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
        //解説文画像
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
        #endregion クイズ画像読み込み終わり

        #region　商品おすすめボイス読み込み
        //基本ボイスの読み込み
        voice_product_basic_ = new AudioClip[basicVoiceName.Length];
        for (int i = 0; i < basicVoiceName.Length; i++)
        {
            voice_product_basic_[i] = Resources.Load<AudioClip>(voiceFolderNameProductBasic + basicVoiceName[i]);
        }
        //質問ボイスの読み込み
        voice_product_question_ = new AudioClip[14];
        for (int i = 0; i < voice_product_question_.Length; i++)
        {
            voice_product_question_[i] = Resources.Load<AudioClip>(voiceFolderNameProductQuestion + "question" + i);
        }
        //商品説明ボイス
        voice_product_info_ = new AudioClip[28];
        for (int i = 0; i < voice_product_info_.Length; i++)
        {
            voice_product_info_[i] = Resources.Load<AudioClip>(voiceFolderNameProductInfo + "product" + i);
        }
        #endregion
        #region 商品おすすめ画像読み込み
        //質問
        for (int i = 0; i < 14; i++)
        {
            string filename = Application.streamingAssetsPath + "/sprites/productsQuestion/question" + i + ".png";
            if (ReadPng(filename) != null)
            {
                texture_ = ReadPng(filename);
                productquestionTextSprites[i] = Sprite.Create(texture_, new Rect(0, 0, 920, 310), Vector2.zero);
            }
            else
            {
                //NoImage用画像にすりかえ
                productquestionTextSprites[i] = null;
            }

        }
        //選択肢
        for (int i = 0; i < 40; i++)
        {
            string filename = Application.streamingAssetsPath + "/sprites/productsQuestionAns/questionAns" + i + ".png";
            if (ReadPng(filename) != null)
            {
                texture_ = ReadPng(filename);
                productquestionAnsTextSprites[i] = Sprite.Create(texture_, new Rect(0, 0, 780, 60), Vector2.zero);
            }
            else
            {
                //NoImage用画像にすりかえ
                productquestionAnsTextSprites[i] = null;
            }

        }
        //商品画像
        for (int i = 0; i < 28; i++)
        {
            string filename = Application.streamingAssetsPath + "/sprites/products/product" + i + ".png";
            if (ReadPng(filename) != null)
            {
                texture_ = ReadPng(filename);
                productSprites[i] = Sprite.Create(texture_, new Rect(0, 0, 370, 550), Vector2.zero);
            }
            else
            {
                //NoImage用画像にすりかえ
                productSprites[i] = null;
            }

        }
        //商品説明画像
        for (int i = 0; i < 28; i++)
        {
            string filename = Application.streamingAssetsPath + "/sprites/productsInfo/info" + i + ".png";
            if (ReadPng(filename) != null)
            {
                texture_ = ReadPng(filename);
                productInfoSprites[i] = Sprite.Create(texture_, new Rect(0, 0, 940, 620), Vector2.zero);
            }
            else
            {
                //NoImage用画像にすりかえ
                productInfoSprites[i] = null;
            }
        }
        #endregion
    }

    void Start()
    {
        //読み込まれているかどうかの確認用
        //audioSource_.PlayOneShot(voice_quiz_basic_[0]);
    }

	// Update is called once per frame
	void Update () {
        Cursor.visible = false;
        /*
        #region マウスカーソル表示・非表示
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (Cursor.visible == true)
            {
                Cursor.visible = false;
            }
            else
            {
                Cursor.visible = true;
            }
        }
        #endregion
         */ 
    }

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
}
