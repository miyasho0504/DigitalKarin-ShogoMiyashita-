using UnityEngine;
using System.Collections;
using System.IO;
using System;
    public class LogOutput : MonoBehaviour
    {

        //ログを出力する(出力先：StreamingAssets/Log/LogText.txt)
        public void LogOutputTextfile(string mode, string question, string select, string result)
        {
            StreamWriter _sw,_sw_M;//_se=最初からずっとログを記述,_sw_M=1月分のログを取得
            //FileInfo _fi;
            //現在の年月日時刻を取得してnowdayに保存
            DateTime nowday = DateTime.Now;
            //文字列に直す「年月日時刻,モード,問題,選択,結果」
            string txt = nowday.Year.ToString()+"/"+nowday.Month.ToString()+"/"+nowday.Day.ToString()+","+nowday.DayOfWeek.ToString()+","+
                nowday.Hour.ToString()+":"+nowday.Minute.ToString()+":"+nowday.Second.ToString()+ 
                "," + mode + "," + question + "," + select + "," + result;

            //ファイルを開く
            try
            {
                //ファイルの場所を取得
                //書き方1
                //_fi = new FileInfo(Application.streamingAssetsPath + "/Log/LogText.csv");
                //_sw = _fi.AppendText();
                //書き方2
                string logFilePath = Application.streamingAssetsPath + "/Log/LogText_ALL.csv";
                //文字列を追加するように設定(true=追加,false=上書き)
                _sw = new StreamWriter(logFilePath, true);
                //一月分のログファイル
                string logFilePath2 = Application.streamingAssetsPath + "/Log/LogText_"+nowday.Year+"_"+nowday.Month+".csv";
                _sw_M=new StreamWriter(logFilePath2,true);
            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.Log("ログファイルが見つかりませんでした。ファイルを作成します。");
                Debug.Log(ex.Message);
                return;
            }
            _sw.WriteLine(txt);
            _sw_M.WriteLine(txt);
            _sw.Flush();
            _sw.Close();
            _sw_M.Flush();
            _sw_M.Close();
        }
    }

