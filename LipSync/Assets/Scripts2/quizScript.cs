using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class quizScript : MonoBehaviour {
    int step = 0;
    int time = 0;
    bool once = false;
    bool once2 = false;
    bool once3 = false;
    int ansSum=0;
    //問題の答えを管理する配列
    int[] ansers = new int[10]{1,2,3,1,2,3,1,2,3,1};

    int Q_num;
    int Q1;
    int Q2;
    int Q_sum = 0;
    int Q_max = 2;//答えさせるクイズの数

     //音声ファイル格納用変数
    public AudioClip[] voice_quiz_=new AudioClip[10];
    public AudioClip voice_quiz_top_;
    public AudioClip voice_quiz_awaji_top_;
    public AudioClip voice_quiz_oko_top_;
    public AudioClip voice_quiz_Q1_1_;
    public AudioClip voice_quiz_Q1_2_;
    public AudioClip voice_quiz_correct_;
    public AudioClip voice_quiz_incorrect_;
    public AudioClip voice_quiz_kaisetu_;
    public AudioClip voice_quiz_result_;
    public AudioClip voice_quiz_allcorrect_;
    public AudioClip voice_quiz_notallcorrect_;

    private AudioSource audioSource_;

	// Use this for initialization
	void Start ()
    {
        audioSource_=gameObject.GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        #region クイズモードTOP
        if (step == 0)
        {
            if (once == false)
            {
                Debug.Log("どのテーマのクイズにしますか？(A→歴史、S→お香、D→メインへ)");
                audioSource_.PlayOneShot(voice_quiz_top_);
                once = true;
            }
            if (audioSource_.isPlaying == false)
            {
                //ボタン1押下→クイズモード(淡路島の歴史TOP)
                if (Input.GetKeyDown(KeyCode.A))
                {
                    step = 10;
                    once = false;
                }
                //ボタン2押下→クイズモード(お香の知識TOP)
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    step = 20;
                    once = false;
                }
                //ボタン3押下→メインメニューへ
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    step = 0;
                    once = false;
                    SceneManager.LoadScene("main");
                }
            }
        }
        #endregion

#region クイズモード淡路島の歴史(STEP10～)
        #region 淡路島の歴史TOP
        if (step==10)
        {
            if (once == false)
            {
                Debug.Log("それでは淡路島の歴史から問題です。");
                audioSource_.PlayOneShot(voice_quiz_awaji_top_);
                Debug.Log("説明中･･･");
                once = true;
            }

            //time++;
            //時間経過で次のステップ
            if (audioSource_.isPlaying==false)
            {
                time = 0;
                once = false;
                step++;
            }
        }
        #endregion

        #region 淡路島の歴史Q
        else if (step == 11)
        {
            if (once == false)
            {
                if (Q_sum == 0)
                {
                    //問題番号を選択
                    Q_num = Random.Range(0, 9);
                    Q1 = Q_num;
                    Debug.Log("Q_num="+Q_num);
                    Q_sum++;
                    Debug.Log("Q_sum="+Q_sum);
                    Debug.Log("それでは1問目～");
                    audioSource_.PlayOneShot(voice_quiz_Q1_1_);
                }else if(Q_sum==1){
                    do{
                        //問題番号を選択
                        Q_num = Random.Range(0, 9);
                        Q2 = Q_num;
                        Debug.Log("Q_num=" + Q_num);
                    }while(Q2==Q1);
                    Q_sum++;
                    Debug.Log("Q_sum=" + Q_sum);
                    Debug.Log("それでは2問目～");
                    audioSource_.PlayOneShot(voice_quiz_Q1_1_);
                }
                Debug.Log("解答待ち･･･");
                once = true;
            }
            else if (audioSource_.isPlaying == false&&once2==false)
            {
                audioSource_.PlayOneShot(voice_quiz_[Q_num]);
                once2 = true;
            }
            if(once2==true&&audioSource_.isPlaying==false&&once3==false){
                audioSource_.PlayOneShot(voice_quiz_Q1_2_);
                once3 = true;
            }
            if (once3 == true && audioSource_.isPlaying == false)
            {
                #region 答え1を選択
                if (Input.GetKeyDown(KeyCode.A))
                {
                    if (ansers[Q_num] == 1)
                    {
                        Debug.Log("正解です！");
                        ansSum++;
                        step=12;
                        time = 0;
                        once = false;
                        once2 = false;
                        once3 = false;
                    }
                    else
                    {
                        Debug.Log("うーん、残念。");
                        step=13;
                        time = 0;
                        once = false;
                        once2 = false;
                        once3 = false;
                    }
                }
                #endregion

                #region 答え2を選択
                if (Input.GetKeyDown(KeyCode.S))
                {
                    if (ansers[Q_num] == 2)
                    {
                        Debug.Log("正解です！");
                        ansSum++;
                        step=12;
                        time = 0;
                        once = false;
                        once2 = false;
                        once3 = false;
                    }
                    else
                    {
                        Debug.Log("うーん、残念。");
                        step=13;
                        time = 0;
                        once = false;
                        once2 = false;
                        once3 = false;
                    }
                }
                #endregion

                #region 答え3を選択
                if (Input.GetKeyDown(KeyCode.D))
                {
                    if (ansers[Q_num] == 3)
                    {
                        Debug.Log("正解です！");
                        ansSum++;
                        step=12;
                        time = 0;
                        once = false;
                        once2 = false;
                        once3 = false;
                    }
                    else
                    {
                        Debug.Log("うーん、残念。");
                        step=13;
                        time = 0;
                        once = false;
                        once2 = false;
                        once3 = false;
                    }
                }
                #endregion

                //時間切れ
                if (time > 300)
                {
                    Debug.Log("うーん、残念");
                    time = 0;
                    once = false;
                    once2 = false;
                    once3 = false;
                    step=13;
                }
                time++;
            }
        }
        #endregion

        #region 正解
        else if(step==12){
            if(once==false){
                audioSource_.PlayOneShot(voice_quiz_correct_);
                once = true;
            }
            if(audioSource_.isPlaying==false){
                step = 14;
                once = false;
            }
        }
        #endregion
        #region 不正解or時間切れ
        else if (step == 13)
        {
            if (once == false)
            {
                audioSource_.PlayOneShot(voice_quiz_incorrect_);
                once = true;
            }
            if (audioSource_.isPlaying == false)
            {
                step = 14;
                once = false;
            }
        }
        #endregion

        #region 淡路島の歴史Q解説
        else if (step == 14)
        {
            if (once == false)
            {
                Debug.Log("Q解説文");
                audioSource_.PlayOneShot(voice_quiz_kaisetu_);
                Debug.Log("T.Oかボタン押下で次のステップへ");
                once = true;
            }
            //T.Oかボタン押下で次のステップへ
            //time++;
            if (audioSource_.isPlaying==false)
            {
                time = 0;
                //クイズに答え終わっていたら結果発表へ
                if (Q_sum == Q_max)
                {
                    step++;
                }//まだクイズが残っていたらクイズへ戻る
                else
                {
                    step = 11;
                }
                once = false;
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                audioSource_.Stop();
                //クイズに答え終わっていたら結果発表へ
                if (Q_sum == Q_max)
                {
                    step++;
                }//まだクイズが残っていたらクイズへ戻る
                else
                {
                    step = 11;
                }
                time = 0;
                once = false;
            }
        }
        #endregion

        #region 淡路島の歴史　結果発表演出
        else if (step == 15)
        {
            if (once == false)
            {
                Debug.Log("それでは結果を発表します");
                audioSource_.PlayOneShot(voice_quiz_result_);
                Debug.Log("T.Oで次のステップへ");
                once = true;
            }
            //T.Oで次のステップへ
            //time++;
            if (audioSource_.isPlaying==false)
            {
                time = 0;
                step++;
                once = false;
            }

        }
        #endregion
        #region 淡路島の歴史　結果発表
        else if (step == 16)
        {
            if (once == false)
            {
                //全問正解
                if (ansSum == 2)
                {
                    Debug.Log("すごい！よく出来ました！");
                    audioSource_.PlayOneShot(voice_quiz_allcorrect_);
                    Debug.Log("T.Oかボタン押下でメインメニューへ戻る");
                }
                //それ以外
                else
                {
                    Debug.Log("頑張りましたね！");
                    audioSource_.PlayOneShot(voice_quiz_notallcorrect_);
                    Debug.Log("T.Oかボタン押下でメインメニューへ戻る");
                }
                once = true;
            }
            //T.Oかボタン押下でメインメニューへ戻る
            time++;
            if (time > 300)
            {
                time = 0;
                step = 0;
                once = false;
                SceneManager.LoadScene("quiz");
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                audioSource_.Stop();
                step = 0;
                time = 0;
                once = false;
                SceneManager.LoadScene("quiz");
            }
        }
        #endregion
#endregion

#region クイズモードお香の知識(STEP20～)
        #region お香の知識TOP
        if (step == 20)
        {
            if (once == false)
            {
                Debug.Log("それではお香の知識から問題です。");
                audioSource_.PlayOneShot(voice_quiz_oko_top_);
                Debug.Log("説明中･･･");
                once = true;
            }
            //time++;

            //時間経過で次のステップ
            if (audioSource_.isPlaying==false)
            {
                time = 0;
                once = false;
                step++;
            }
        }
        #endregion

        #region お香の知識Q
        else if (step == 21)
        {
            if (once == false)
            {
                if (Q_sum == 0)
                {
                    //問題番号を選択
                    Q_num = Random.Range(0, 9);
                    Q1 = Q_num;
                    Debug.Log("Q_num=" + Q_num);
                    Q_sum++;
                    Debug.Log("Q_sum=" + Q_sum);
                    Debug.Log("それでは1問目～");
                }
                else if (Q_sum == 1)
                {
                    do
                    {
                        //問題番号を選択
                        Q_num = Random.Range(0, 9);
                        Q2 = Q_num;
                        Debug.Log("Q_num=" + Q_num);
                    } while (Q2 == Q1);
                    Q_sum++;
                    Debug.Log("Q_sum=" + Q_sum);
                    Debug.Log("それでは2問目～");
                }
                Debug.Log("解答待ち･･･");
                once = true;
            }

            #region 答え1を選択
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (ansers[Q_num] == 1)
                {
                    Debug.Log("正解です！");
                    ansSum++;
                    step++;
                    time = 0;
                    once = false;
                }
                else
                {
                    Debug.Log("うーん、残念。");
                    step++;
                    time = 0;
                    once = false;
                }
            }
            #endregion

            #region 答え2を選択
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (ansers[Q_num] == 2)
                {
                    Debug.Log("正解です！");
                    ansSum++;
                    step++;
                    time = 0;
                    once = false;
                }
                else
                {
                    Debug.Log("うーん、残念。");
                    step++;
                    time = 0;
                    once = false;
                }
            }
            #endregion

            #region 答え3を選択
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (ansers[Q_num] == 3)
                {
                    Debug.Log("正解です！");
                    ansSum++;
                    step++;
                    time = 0;
                    once = false;
                }
                else
                {
                    Debug.Log("うーん、残念。");
                    step++;
                    time = 0;
                    once = false;
                }
            }
            #endregion

            //時間切れ
            if (time > 300)
            {
                Debug.Log("うーん、残念");
                time = 0;
                once = false;
                step++;
            }
            time++;
        }
        #endregion
        #region お香の知識Q解説
        else if (step == 22)
        {
            if (once == false)
            {
                Debug.Log("Q解説文");
                Debug.Log("T.Oかボタン押下で次のステップへ");
                once = true;
            }
            //T.Oかボタン押下で次のステップへ
            time++;
            if (time > 300)
            {
                time = 0;
                //クイズに答え終わっていたら結果発表へ
                if (Q_sum == Q_max)
                {
                    step++;
                }//まだクイズが残っていたらクイズへ戻る
                else
                {
                    step = 21;
                }
                once = false;
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                //クイズに答え終わっていたら結果発表へ
                if (Q_sum == Q_max)
                {
                    step++;
                }//まだクイズが残っていたらクイズへ戻る
                else
                {
                    step = 21;
                }
                time = 0;
                once = false;
            }
        }
        #endregion

        #region お香の知識　結果発表演出
        else if (step == 23)
        {
            if (once == false)
            {
                Debug.Log("それでは結果を発表します");
                Debug.Log("T.Oで次のステップへ");
                once = true;
            }
            //T.Oで次のステップへ
            time++;
            if (time > 60)
            {
                time = 0;
                step++;
                once = false;
            }

        }
        #endregion
        #region お香の知識　結果発表
        else if (step == 24)
        {
            if (once == false)
            {
                //全問正解
                if (ansSum == 2)
                {
                    Debug.Log("すごい！よく出来ました！");
                    Debug.Log("T.Oかボタン押下でメインメニューへ戻る");
                }
                //それ以外
                else
                {
                    Debug.Log("頑張りましたね！");
                    Debug.Log("T.Oかボタン押下でメインメニューへ戻る");
                }
                once = true;
            }
            //T.Oかボタン押下でメインメニューへ戻る
            time++;
            if (time > 300)
            {
                time = 0;
                step = 0;
                once = false;
                SceneManager.LoadScene("main");
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                step = 0;
                time = 0;
                once = false;
                SceneManager.LoadScene("main");
            }
        }
        #endregion
#endregion

    }
}
