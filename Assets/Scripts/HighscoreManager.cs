using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class Score
{
    public string name;
    public int score;
}

[System.Serializable]
public class Highscores
{
    public List<Score> highscores;
}

public static class HighscoreManager
{
    static public List<Score> highscores;
    static private string URL = "https://whosinternetapi.herokuapp.com";

    static public IEnumerator GetHighscores()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(URL + "/get_list");

        yield return webRequest.SendWebRequest();

        string json = webRequest.downloadHandler.text;
        Highscores data = JsonUtility.FromJson<Highscores>(@"{""highscores"":" + json + "}");
        highscores = data.highscores;
    }

    static public IEnumerator AddHighscore(string name, int score)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(URL + "/add_score/" + name + "/" + score);
        yield return webRequest.SendWebRequest();
    }
}
