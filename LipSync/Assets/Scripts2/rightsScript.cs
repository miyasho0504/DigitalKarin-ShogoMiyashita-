using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class rightsScript : MonoBehaviour {

    float time_=0.0f;
    float fade_time_ = 5.0f;
    float fade_out_time_ = 5.0f;

    public GameObject Text_;
    private SpriteRenderer sp_;

    //シーン非同期読み込み
    private AsyncOperation m_AsyncOpe = null;
    private bool m_sceneChanged = false;//次のシーンに移行したかどうかのフラグ

	// Use this for initialization
	void Start () {
        sp_ = Text_.GetComponent<SpriteRenderer>();
        //非同期でリソースマネージャーを読み込み
        m_AsyncOpe = SceneManager.LoadSceneAsync("resourceManager");
        //読み込み完了後自動でシーン切り替えを行わないように設定
        m_AsyncOpe.allowSceneActivation = false;
        //マウスカーソルを非表示
        Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update () {
        time_ += Time.deltaTime;
        if(time_<6.0f){
            // フェードイン
            float alpha = time_ / fade_time_;
            var color = sp_.color;
            color.a = alpha;
            sp_.color = color;
        }
        if(time_>7.0f){
            fade_out_time_ -= Time.deltaTime;
            // フェードイン
            float alpha = fade_out_time_ / fade_time_;
            var color = sp_.color;
            color.a = alpha;
            sp_.color = color;
        }
        if (!m_sceneChanged && m_AsyncOpe.progress >= 0.9f&&fade_out_time_ < 0.0f)
        {
            // 次のシーンに移行.
            m_AsyncOpe.allowSceneActivation = true;
            m_sceneChanged = true;
        }
        //Debug.Log("now_Load_progress:"+m_AsyncOpe.progress);
	}
}
