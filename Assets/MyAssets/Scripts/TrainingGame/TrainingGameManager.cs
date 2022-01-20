using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class TrainingGameManager : MonoBehaviour
{

    [SerializeField] TrainingGameAudioManager audioManager;
    [SerializeField] TrainingGameSettings settingsManager;
    [SerializeField] LevelObjectManager levelManager;
    [SerializeField] WordSelectionManager selectionManager;
    [SerializeField] ResultManager resultManager;
    [SerializeField] RewardManager rewardManager;
    
    //TrainingGameSettings settings;
    // number of sentences (excluding practice rounds and repetitions) to be played within a session
    [SerializeField] int gameLength;
    // min number of practice rounds to be played (stops @ first mistake after a correct answer)
    [SerializeField] int min_practiceRounds;
    // number of consecutive correct answers to achieve a reward
    [SerializeField] int rewardHits;

    [SerializeField] float onHit_SNR = -1.5f;
    [SerializeField] float onMiss_SNR = 2.5f;
    [SerializeField] float onUnsure_SNR = 1.5f;


    // there can only be one wordList (even only one voice)
    [SerializeField] string male_targetAudioPath;
    [SerializeField] string female_targetAudioPath;
    [SerializeField] string iconsPath;
    [SerializeField] int targetWordGroups;
    [SerializeField] int[] targetSelectables;

    // maybe move them to AudioManager?
    [SerializeField] AudioClip distracterStory_male_left;
    [SerializeField] AudioClip distracterStory_male_right;
    [SerializeField] AudioClip distracterStory_female_left;
    [SerializeField] AudioClip distracterStory_female_right;

    // set result, reward etc managers as private variables through getcomponentincildren
    private Sentence sent;
    private LiSN_database lisnData;

    // ### maybe make this adjustable
    private int selectionOptions = 4;

    /// Control variables
    // start each session with practice mode
    private bool practiceMode = true;
    // keep track of practice rounds/sentences
    private int practiceRounds = 0;


    // keep track of rounds/sentences played within current session
    private int roundsPlayed = 0;
    // keep track of current consecutive hits
    private int rewardCount = 0;
    // repeat last sentence if 'unsure' was selected (do this only once!)
    private bool repeatSentence = false;

    // SNR ratio stored for each round (average over whole session will be used)
    private float[] SNR_values;
    private int hits = 0;
    private int misses = 0;
    private int rewards = 0;

    void Start()
    {
        settingsManager.voiceCallback = OnStart;
        settingsManager.gameObject.SetActive(true);

        selectionManager.onHitCallback = OnHit;
        selectionManager.onMissCallback = OnMiss;
        selectionManager.onUnsureCallback = OnUnsure;
        selectionManager.onContinueCallback = OnContinue;
        
        audioManager.onPlayingDoneCallback = OnPlayingDone;

        // place and show level objects
        levelManager.setLevelObjectPositions();
        levelManager.showLevelObjects(true);
    }


    void OnStart(int voice)
    {

        Debug.Log("Start Training Game");

        // create LiSN_database object
        // voices: male (0), female (1)
        if(voice == 0)
        {
            lisnData = new LiSN_database(male_targetAudioPath, iconsPath, targetWordGroups, targetSelectables);
            Debug.Log("attempt to set dist");
            audioManager.setDistracterSequences(distracterStory_male_left, distracterStory_male_right);
        }
        else
        {
            lisnData = new LiSN_database(female_targetAudioPath, iconsPath, targetWordGroups, targetSelectables);
            audioManager.setDistracterSequences(distracterStory_female_left, distracterStory_female_right);
        }
        

        // create sentence object
        sent = new Sentence(lisnData.getSentenceLen(), targetSelectables);

        SNR_values = new float[gameLength];

        // make sure to disable UI at load.
        selectionManager.showWordSelectionUI(false);

        // generate a new sentence
        sent.createSentence(lisnData);

        // move new sentence audio to audioManager
        audioManager.setTargetSentence(sent.audio);

        // start playing again
        audioManager.startPlaying();
    }

    void OnSessionDone()
    {
        Debug.Log("Training session done!");

        // calculate average SNR of the session
        float average_SNR = 0;
        for (int i = 0; i < roundsPlayed; i++)
        {
            average_SNR += SNR_values[i];
        }
        average_SNR /= roundsPlayed;
        // set current session data onto result UI
        resultManager.setTrainingGameResults(average_SNR, rewards, hits, misses, roundsPlayed);
        // show result UI
        resultManager.showResults();
        // store session data in userManagement
        UserManagement.selfReference.addUserResults(average_SNR, rewards);

    }


    /// Audio Manager Callbacks
    // when audio manager has finished playing, reset control variable
    void OnPlayingDone()
    {

        Debug.Log("OnPlayingDone");

        if (repeatSentence)
        {
            selectionManager.showWordSelectionUI(true);
            Debug.Log("Repeat sentence...");
            return;
        }

        // randomly chose one of the 'selectable' groups for this round
        int randGroup = Random.Range(0, targetSelectables.Length);
        string[] words;
        Sprite[] icons;


        if (!practiceMode)
        {
            SNR_values[roundsPlayed - 1] = audioManager.getTalkerVolume();
            Debug.Log("Stored SNR: " + SNR_values[roundsPlayed-1] + " round: " + roundsPlayed);
        }

        Debug.Log("randGroup: " + randGroup);
        lisnData.getSelectableWords(randGroup, selectionOptions, sent.getSelectableWordIndex(randGroup), out words, out icons);

        // show wordSelection UI elements
        selectionManager.startWordSelection(words, icons);

    }



    /// Word Selection UI Callbacks
    void OnHit()
    {

        audioManager.playOnHit();

        repeatSentence = false;

        if (practiceMode)
        {
            // decrease SNR by reducing talker volume by 3.0 dB
            audioManager.changeTalkerVolume(-3.0f);
            return;
        }

        // decrease SNR by reducing talker volume by -1.5 dB
        audioManager.changeTalkerVolume(onHit_SNR);

        hits++;
        rewardCount++;
        Debug.Log("Hits: " + hits + " Streak: " + rewardCount);

        if (rewardCount >= rewardHits)
        {
            Debug.Log("Player Reward achieved!");
            audioManager.playOnReward();
            rewardManager.showReward(rewards++);
            // reset rewardCount
            rewardCount = 0;
        }

    }

    // Called when the player selected a false word option
    // Increase SNR, play 'false' sound
    void OnMiss()
    {

        audioManager.playOnMiss();

        repeatSentence = false;

        if (practiceMode)
        {
            // decrease SNR by reducing talker volume by 3.0 dB
            //audioManager.changeTalkerVolume(-3.0f);
            if (practiceRounds >= min_practiceRounds)
            {
                Debug.Log("Leave practive mode");
                practiceMode = false;
            }

            return;
        }

        misses++;

        // improve SNR by increasing talker volume by 2.5 dB
        audioManager.changeTalkerVolume(onMiss_SNR);

        rewardCount = 0;

    }

    void OnUnsure()
    {
        if (practiceMode)
        {
            // decrease SNR by reducing talker volume by 3.0 dB
            //audioManager.changeTalkerVolume(-3.0f);
            if (practiceRounds >= min_practiceRounds)
            {
                Debug.Log("Leave practive mode");
                practiceMode = false;
            }
        }
        else
        {
            // improve SNR by increasing talker volume by 1.5 dB
            audioManager.changeTalkerVolume(onUnsure_SNR);
        }   
        

        if(!repeatSentence)
        {
            Debug.Log("Unsure: repeat sentence with increase SNR");
            repeatSentence = true;

            // start playing again
            audioManager.startPlaying();

            // hide wordSelection UI elements
            selectionManager.showWordSelectionUI(false);
        }
        else
        {
            repeatSentence = false;
        }

    }

    void OnContinue()
    {
        // hide wordSelection UI elements
        selectionManager.showWordSelectionUI(false);

        if (roundsPlayed >= gameLength)
        {
            OnSessionDone();
            return;
        }

        if(!repeatSentence)
        {
            // generate a new sentence
            sent.createSentence(lisnData);

            // move new sentence audio to audioManager
            audioManager.setTargetSentence(sent.audio);

            if(practiceMode)
            {
                practiceRounds++;
                Debug.Log("Practice round: " + practiceRounds + " of " + min_practiceRounds + " (min)");
            }
            else
            {
                roundsPlayed++;
                Debug.Log("Round: " + roundsPlayed + " of " + gameLength);
            }
            
        }

        // start playing again
        audioManager.startPlaying();

    }

}
