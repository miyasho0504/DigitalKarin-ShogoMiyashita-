using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class resourceManageScript2 : MonoBehaviour {

    public GameObject go_;

	// Use this for initialization
	void Start () {
	
	}

    void Awake()
    {
        DontDestroyOnLoad(go_);
    }

	// Update is called once per frame
	void Update () {
        SceneManager.LoadScene("main");
	}
}
