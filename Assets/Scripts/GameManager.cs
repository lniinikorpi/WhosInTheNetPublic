using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public GameObject pickedKnob;
    public Button m_playButton, m_exitButton;
    public GameObject m_Main, m_Pause, m_End;
    public GameObject timeText;

    public AudioClip knobSound;

    public float maxValue = 10;
    public float minValue = -10;
    public float valueMultiplier = 100;

    private Vector3 mouseInit;

    public float x = 0;
    public float y = 0;

    public float targetX;
    public float targetY;
    public float signalValue;
    public float signalAmplitude;

    public float satisfactionTick = 0.1f;

    public bool targetIsX;
    public float targetLerpValue;
    public bool targetIsLerping = false;

    public Material satisfaction;

    private float brightnessTick = 1;
    private float signalTick = 0;
    private bool wireInitial = true;

    public float currentSatisfaction = 5;
    public float lastSatisfaction;

    public Material onMaterial;
    public Material offMaterial;

    public GameObject antenna;
    public GameObject wire;
    public GameObject signalRenderer;

    public bool wireBroken = false;
    public bool antennaBroken = false;

    public Animator animator;

    public List<Renderer> satisfactionLights = new List<Renderer>();

    public int breakTimeMax;
    public int breakTimeMin;
    private float canBreak = 4;

    public ParticleSystem smoke;
    public ParticleSystem sparks;

    private float startX;
    private float startY;

    int timerS = 0;
    int timerM = 0;

    public GameObject signal;

    public bool paused = true;
    private float canGiveNewValue = -1;

    private float signalTickAmount = 0.05f;
    private float moreHardness = 5;

    private float gameTime;
    private float initTime = 0;

    public float goodThreshold = .1f;
    public float badThreshold = .3f;
    public Text highScoreText;

    public AudioListener audio;
    public GameObject SoundON;
    public GameObject SoundOFF;

    public InputField inputField;
    public string playerName;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        StartCoroutine(HighscoreManager.GetHighscores());

    }

    private void Start()
    {
        StartCoroutine(Timer());
        canBreak = Random.Range(breakTimeMin, breakTimeMax);
        sparks.Stop();
        smoke.Play();
        smoke.gameObject.SetActive(false);
        SoundSystem();
    }

    void Update()
    {
        #if UNITY_EDITOR //debug mutefeaturen säätöön
        if (Input.GetKeyDown(KeyCode.F12))
        {
            PlayerPrefs.DeleteAll();
        }
        #endif

        if (!paused)
        {
            if(initTime == 0)
            {
                initTime = Time.time;
            }
            gameTime = Time.time - initTime;
            if (Input.GetKeyDown(KeyCode.Escape) && m_Main.gameObject.activeSelf == false)
            {
                m_Pause.SetActive(true);
                Time.timeScale = 0;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) && m_Main.gameObject.activeSelf == false && m_Pause.gameObject.activeSelf == false && m_End.gameObject.activeSelf == false)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                mouseInit = Input.mousePosition;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.gameObject.tag == "Antenna")
                    {
                        animator.Play("antenna");
                        antennaBroken = false;
                    }
                    else if (hit.transform.gameObject.tag == "Wire")
                    {
                        wireBroken = false;
                        StartCoroutine(DetachWire());
                    }
                    else
                    {
                        pickedKnob = hit.transform.gameObject;
                    }
                }
            }
            if(moreHardness <= gameTime)
            {
                signalTickAmount += 0.01f;
                moreHardness = gameTime + 5;
            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                pickedKnob = null;
            }

            if (pickedKnob != null)
            {
                RotateKnob();
            }

            CalculateSignal();
            CalculateNewTarget();
            if (canBreak < gameTime)
            {
                canBreak = gameTime + Random.Range(breakTimeMin, breakTimeMax); ;
                RaffleBreak();
            }
            BreakAntenna();
        }

    }

    private void RaffleBreak()
    {
        int i = Random.Range(0, 2);
        if(i == 0)
        {
            if (wireBroken == false)
            {
                wireBroken = true;
                StartCoroutine(DetachWire());
            }
        }
        else
        {
            if (antennaBroken == false)
            {
                antennaBroken = true;
            }
        }
    }

    private void CalculateNewTarget()
    {
        if (targetIsLerping)
        {
            if (targetIsX)
            {
                targetX = Mathf.Lerp(startX, targetLerpValue, signalTick);
            }
            else
            {
                targetY = Mathf.Lerp(startY, targetLerpValue, signalTick);
            }
            if (signalTick > 0.95f)
            {
                targetIsLerping = false;
            }
            signalTick += signalTickAmount * Time.deltaTime;
        }
        else if (signalAmplitude < badThreshold && canGiveNewValue <= gameTime)
        {
            canGiveNewValue = gameTime + 2;
            targetIsLerping = true;
            targetLerpValue = Random.Range(minValue, maxValue);
            targetIsX = !targetIsX;
            startX = targetX;
            startY = targetY;
            signalTick = 0;
        }
    }

    private IEnumerator DetachWire()
    {
        if (wireBroken && wireInitial)
        {
            wireInitial = false;
            wire.GetComponent<Animator>().Play("Detach");
            signal.SetActive(false);
            sparks.Play();
        }
        else if (!wireBroken && !wireInitial)
        {
            wireInitial = true;
            wire.GetComponent<Animator>().Play("Attach");
            yield return new WaitForSeconds(0.5f);
            signal.SetActive(true);
            sparks.Stop();
            signal.GetComponent<AudioSource>().Play();
        }
    }
    
    private void BreakAntenna()
    {
        if (antennaBroken && brightnessTick >= 0)
        {
            if (!smoke.isPlaying)
            {
                smoke.gameObject.SetActive(true);
            }
            brightnessTick -= 0.1f * Time.deltaTime;
            signalRenderer.GetComponent<SignalRenderer>().brightness = Mathf.Lerp(0, 1, brightnessTick);
        }
        else if (!antennaBroken && brightnessTick <= 1)
        {
            if (smoke.isPlaying)
            {
                smoke.gameObject.SetActive(false);  
            }
            brightnessTick += 0.2f * Time.deltaTime;
            signalRenderer.GetComponent<SignalRenderer>().brightness = Mathf.Lerp(0, 1, brightnessTick);
        }
    }

    private void RotateKnob()
    {
        Vector3 mouseMovement = new Vector3();
        mouseMovement.z = Input.mousePosition.x - mouseInit.x;
        pickedKnob.transform.Rotate(mouseMovement);
        Vector3 rot = pickedKnob.transform.localRotation.eulerAngles;
        float zrot = rot.z;
        if (mouseMovement.z <= 0)
        {
            if(rot.z < 320 && !(rot.z <= 220 || rot.z > 320))
            {
                zrot = Mathf.Clamp(pickedKnob.transform.localRotation.eulerAngles.z, 320, 360);
            }
        }
        else if(rot.z >= 320)
        {
            zrot = Mathf.Clamp(pickedKnob.transform.localRotation.eulerAngles.z, 320, 360);
        }
        else
        {
            zrot = Mathf.Clamp(pickedKnob.transform.localRotation.eulerAngles.z, 0, 220);
        }
        rot.z = zrot;
        pickedKnob.transform.localRotation = Quaternion.Euler(rot);
        pickedKnob.GetComponent<Knob>().AdjustValue(pickedKnob.transform.localEulerAngles.z);
        mouseInit = Input.mousePosition;
    }

    public void CalculateSignal()
    {
        signalValue = Vector3.Distance(new Vector3(x, y), new Vector3(targetX, targetY));
        signalAmplitude = Mathf.Abs((signalValue) / 5.0f);
        CalculateSatisfaction();
    }

    public void CalculateSatisfaction()
    {
        lastSatisfaction = currentSatisfaction;
        if (signalAmplitude > badThreshold)
        {
            currentSatisfaction -= satisfactionTick * Time.deltaTime;
        }
        else if (signalAmplitude < goodThreshold)
        {
            currentSatisfaction += satisfactionTick * Time.deltaTime;
        }
        currentSatisfaction = Mathf.Clamp(currentSatisfaction, 0, 1);

        float satisfactionInterval = 1.0f / satisfactionLights.Count;
        for (int i = 0; i < satisfactionLights.Count; i++)
        {
            if (currentSatisfaction > (float)i * satisfactionInterval)
            {
                satisfactionLights[i].material = onMaterial;
            }
            else
            {
                satisfactionLights[i].material = offMaterial;
            }
        }

        if (currentSatisfaction == 0 && m_End.activeSelf == false)
        {
            EndScreen();
        }
    }


    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
             Application.Quit();
        #endif
    }

    public void UnPause()
    {
        m_Pause.SetActive(false);
        Time.timeScale = 1;
    }

    IEnumerator Timer()
    {
        while (true)
        {
            if (m_End.activeSelf == true)
            {
                break;
            }
            yield return new WaitForSeconds(1);
            timerS++;
            if (timerS == 60)
            {
                timerS = 0;
                timerM++;
            }

        }

    }

    void EndScreen()
    {
        m_End.SetActive(true);
        if (timerS >= 10)
        {
            timeText.GetComponent<Text>().text = timerM.ToString() + ":" + timerS.ToString();

        }
        else timeText.GetComponent<Text>().text = timerM.ToString() + ":0" + timerS.ToString();

        int peopleInNet = Mathf.FloorToInt(gameTime * Mathf.PI * 1000);
        timeText.GetComponent<Text>().text = peopleInNet.ToString();
        highScoreText.text = "";
        StartCoroutine(GetAddShowScore(peopleInNet));
         

    }

    private IEnumerator GetAddShowScore(int score)
    {
        yield return HighscoreManager.AddHighscore(playerName, score);
        yield return HighscoreManager.GetHighscores();
        for (int i = 0; i < HighscoreManager.highscores.Count; i++)
        {
            highScoreText.text += HighscoreManager.highscores[i].name + ": " + HighscoreManager.highscores[i].score + "\n";
        }

    }

    public void EndScreenButton()
    {
        currentSatisfaction = 0.5f;
        timerS = 0;
        timerM = 0;
        StartCoroutine(Timer());
        SceneManager.LoadScene(0);
    }

    public void StartGame()
    {
        paused = false;
        signalRenderer.GetComponent<AudioSource>().Play();
    }

    public void ChangeSound() {
        if (PlayerPrefs.GetInt("Sound", 1) == 1)
        {
            PlayerPrefs.SetInt("Sound", 0);
            AudioListener.volume = 0;
        }
        else {
            PlayerPrefs.SetInt("Sound", 1);
            AudioListener.volume = 1;
        }
    }

    void SoundSystem() {
        if(PlayerPrefs.GetInt("Sound", 1) == 1) {
            AudioListener.volume = 1;
            SoundON.SetActive(true);
            SoundOFF.SetActive(false);
        }
        else {
            AudioListener.volume = 0;
            SoundON.SetActive(false);
            SoundOFF.SetActive(true);
        }
    }

    public void GetPlayerName()
    {
        playerName = inputField.text;
    }

}
