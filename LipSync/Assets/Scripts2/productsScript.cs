using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class productsScript : MonoBehaviour {
    int step = 0;
    //int time = 0;
    bool once = false;
    bool once2 = false;
    int ans1 = 0;
    int ans2 = 0;
    int ans3 = 0;
    int ansT = 0;

    //音声ファイル格納用変数
    public AudioClip voice_product_top_;
    public AudioClip voice_product_Q1_;
    public AudioClip voice_product_Q2Q3_;
    public AudioClip voice_product_Q1Q2A_;
    public AudioClip voice_product_Q3A_;
    public AudioClip voice_product_result_;
    public AudioClip voice_product_detail1_;
    public AudioClip voice_product_detail2_;

    private AudioSource audioSource_;

	// Use this for initialization
	void Start () {
        step = 0;
        once = false;
        audioSource_=gameObject.GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        #region TOP説明
	    if(step==0){
            if(once==false){
                Debug.Log("TOP説明");
                audioSource_.PlayOneShot(voice_product_top_);
                Debug.Log("T.Oかボタン押下で次のステップへ");
                once = true;
            }
            //ボタン押下で次のステップへ
            if(audioSource_.isPlaying==false){
                step=1;
                once = false;
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                audioSource_.Stop();
                step=1;
                once = false;
            }
        }
        #endregion

        #region 商品オススメQ1
        else if(step==1){
            if (once == false)
            {
                Debug.Log("Q1説明");
                audioSource_.PlayOneShot(voice_product_Q1_);
                Debug.Log("ボタン(A,S,D)押下で次のステップへ");
                once = true;
            }
            if (Input.GetKeyDown(KeyCode.A)){
                audioSource_.Stop();
                ans1 = 1;
                step++;
                once = false;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                audioSource_.Stop();
                ans1 = 2;
                step++;
                once = false;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                audioSource_.Stop();
                ans1 = 3;
                step++;
                once = false;
            }
        }
        #endregion
        #region 商品おすすめQ1解答
        else if(step==2){
            switch(ans1){
                //メモ：AudioSourceのisPlaying変数で再生されているかどうかtrue/falseで返ってくる
                case 1:
                    if (once == false)
                    {
                        audioSource_.PlayOneShot(voice_product_Q1Q2A_);
                        Debug.Log("1ですね");
                        once = true;
                    }
                    if (audioSource_.isPlaying == false)
                    {
                        once = false;
                        step++;
                    }
                    break;
                case 2: 
                    if (once == false)
                    {
                        audioSource_.PlayOneShot(voice_product_Q1Q2A_);
                        Debug.Log("2ですね");
                        once = true;
                    }
                    if (audioSource_.isPlaying == false)
                    {
                        once = false;
                        step++;
                    }
                    break;
                case 3:
                    if (once == false)
                    {
                        audioSource_.PlayOneShot(voice_product_Q1Q2A_);
                        Debug.Log("3ですね");
                        once = true;
                    }
                    if (audioSource_.isPlaying == false)
                    {
                        once = false;
                        step++;
                    }
                    break;
                default: break;
            }
        }
        #endregion

        #region 商品オススメQ2
        else if (step == 3)
        {
            if (once == false)
            {
                Debug.Log("Q2説明");
                audioSource_.PlayOneShot(voice_product_Q2Q3_);
                Debug.Log("ボタン(A,S,D)押下で次のステップへ");
                once = true;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                audioSource_.Stop();
                ans2 = 1;
                step++;
                once = false;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                audioSource_.Stop();
                ans2 = 2;
                step++;
                once = false;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                audioSource_.Stop();
                ans2 = 3;
                step++;
                once = false;
            }
        }
        #endregion
        #region 商品おすすめQ2解答
        else if (step == 4)
        {
            switch (ans2)
            {
                //メモ：AudioSourceのisPlaying変数で再生されているかどうかtrue/falseで返ってくる
                case 1:
                    if (once == false)
                    {
                        audioSource_.PlayOneShot(voice_product_Q1Q2A_);
                        Debug.Log("1ですね");
                        once = true;
                    }
                    if (audioSource_.isPlaying == false)
                    {
                        once = false;
                        step++;
                    }
                    break;
                case 2:
                    if (once == false)
                    {
                        audioSource_.PlayOneShot(voice_product_Q1Q2A_);
                        Debug.Log("2ですね");
                        once = true;
                    }
                    if (audioSource_.isPlaying == false)
                    {
                        once = false;
                        step++;
                    }
                    break;
                case 3:
                    if (once == false)
                    {
                        audioSource_.PlayOneShot(voice_product_Q1Q2A_);
                        Debug.Log("3ですね");
                        once = true;
                    }
                    if (audioSource_.isPlaying == false)
                    {
                        once = false;
                        step++;
                    }
                    break;
                default: break;
            }
        }
        #endregion

        #region 商品オススメQ3
        else if (step == 5)
        {
            if (once == false)
            {
                Debug.Log("Q3説明");
                audioSource_.PlayOneShot(voice_product_Q2Q3_);
                Debug.Log("ボタン(A,S,D)押下で次のステップへ");
                once = true;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                audioSource_.Stop();
                ans3 = 1;
                step++;
                once = false;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                audioSource_.Stop();
                ans3 = 2;
                step++;
                once = false;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                audioSource_.Stop();
                ans3 = 3;
                step++;
                once = false;
            }
        }
        #endregion
        #region 商品おすすめQ3解答
        else if (step == 6)
        {
            switch (ans3)
            {
                //メモ：AudioSourceのisPlaying変数で再生されているかどうかtrue/falseで返ってくる
                case 1:
                    if (once == false)
                    {
                        audioSource_.PlayOneShot(voice_product_Q3A_);
                        Debug.Log("1ですね");
                        once = true;
                    }
                    if (audioSource_.isPlaying == false)
                    {
                        once = false;
                        step++;
                    }
                    break;
                case 2:
                    if (once == false)
                    {
                        audioSource_.PlayOneShot(voice_product_Q3A_);
                        Debug.Log("2ですね");
                        once = true;
                    }
                    if (audioSource_.isPlaying == false)
                    {
                        once = false;
                        step++;
                    }
                    break;
                case 3:
                    if (once == false)
                    {
                        audioSource_.PlayOneShot(voice_product_Q3A_);
                        Debug.Log("3ですね");
                        once = true;
                    }
                    if (audioSource_.isPlaying == false)
                    {
                        once = false;
                        step++;
                    }
                    break;
                default: break;
            }
        }
        #endregion

        #region 商品おすすめ結果表示
        else if(step==7){
            if (once == false)
            {
                once = true;
                Debug.Log("現在結果表示中･･･T.Oかボタン押下で次へ");
                audioSource_.PlayOneShot(voice_product_result_);
                ansT = ans1 * 100 + ans2 * 10 + ans3;
                switch (ansT)
                {
                    case 111: Debug.Log("111"); break;
                    case 112: Debug.Log("112"); break;
                    case 113: Debug.Log("113"); break;
                    case 121: Debug.Log("121"); break;
                    case 122: Debug.Log("122"); break;
                    case 123: Debug.Log("123"); break;
                    case 131: Debug.Log("131"); break;
                    case 132: Debug.Log("132"); break;
                    case 133: Debug.Log("133"); break;

                    case 211: Debug.Log("211"); break;
                    case 212: Debug.Log("212"); break;
                    case 213: Debug.Log("213"); break;
                    case 221: Debug.Log("221"); break;
                    case 222: Debug.Log("222"); break;
                    case 223: Debug.Log("223"); break;
                    case 231: Debug.Log("231"); break;
                    case 232: Debug.Log("232"); break;
                    case 233: Debug.Log("233"); break;

                    case 311: Debug.Log("311"); break;
                    case 312: Debug.Log("312"); break;
                    case 313: Debug.Log("313"); break;
                    case 321: Debug.Log("321"); break;
                    case 322: Debug.Log("322"); break;
                    case 323: Debug.Log("323"); break;
                    case 331: Debug.Log("331"); break;
                    case 332: Debug.Log("332"); break;
                    case 333: Debug.Log("333"); break;
                    default: break;
                }
            }
            
            //T.Oかボタン押下で次のステップへ
            if (audioSource_.isPlaying==false)
            {
                step++;
                once = false;
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                audioSource_.Stop();
                step++;
                once = false;
            }
        }
        #endregion

        #region 商品おすすめ結果詳細表示
        else if (step == 8)
        {
            if (once == false)
            {
                once = true;
                Debug.Log("現在結果詳細表示中･･･T.Oかボタン押下でメインメニューへ");
                audioSource_.PlayOneShot(voice_product_detail1_);
                ansT = ans1 * 100 + ans2 * 10 + ans3;
                switch (ansT)
                {
                    case 111: Debug.Log("111"); break;
                    case 112: Debug.Log("112"); break;
                    case 113: Debug.Log("113"); break;
                    case 121: Debug.Log("121"); break;
                    case 122: Debug.Log("122"); break;
                    case 123: Debug.Log("123"); break;
                    case 131: Debug.Log("131"); break;
                    case 132: Debug.Log("132"); break;
                    case 133: Debug.Log("133"); break;

                    case 211: Debug.Log("211"); break;
                    case 212: Debug.Log("212"); break;
                    case 213: Debug.Log("213"); break;
                    case 221: Debug.Log("221"); break;
                    case 222: Debug.Log("222"); break;
                    case 223: Debug.Log("223"); break;
                    case 231: Debug.Log("231"); break;
                    case 232: Debug.Log("232"); break;
                    case 233: Debug.Log("233"); break;

                    case 311: Debug.Log("311"); break;
                    case 312: Debug.Log("312"); break;
                    case 313: Debug.Log("313"); break;
                    case 321: Debug.Log("321"); break;
                    case 322: Debug.Log("322"); break;
                    case 323: Debug.Log("323"); break;
                    case 331: Debug.Log("331"); break;
                    case 332: Debug.Log("332"); break;
                    case 333: Debug.Log("333"); break;
                    default: break;
                }
            }
            else if (once2 == false&&audioSource_.isPlaying==false)
            {
                Debug.Log("お役にたてましたか？");
                audioSource_.PlayOneShot(voice_product_detail2_);
                once2 = true;
            }
            else
            {
                //T.Oかボタン押下でメインメニューへ戻る
                if (audioSource_.isPlaying==false)
                {
                    step = 0;
                    once = false;
                    once2 = false;
                    SceneManager.LoadScene("main");
                }
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    audioSource_.Stop();
                    step = 0;
                    once = false;
                    once2 = false;
                    SceneManager.LoadScene("main");
                }
            }
        }
        #endregion
    }
}