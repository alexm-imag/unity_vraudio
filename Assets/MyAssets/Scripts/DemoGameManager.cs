using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public partial class DemoGameManager : MonoBehaviour
{
    [SerializeField] Vector3 talkerPos;
    [SerializeField] Vector3 distractorPos1;
    [SerializeField] Vector3 distractorPos2;
    [SerializeField] Vector3 distractorPos3;

    private Sentence sent;
    //private audioFiles aFiles;
    private LiSN_database lisnData;

    private AudioClip[] sentenceAudio;
    private int clipCount = 9;
    private int wordCount = 6;
    // keep track of which word of a sentence has already been played
    private int wordIx = 0;
    private bool sentenceReady = false;

    private bool sceneEntered = false;

    public GameObject PlayerCamera;
    public GameObject TalkerObj;
    public GameObject DistractorObj;
    public AudioSource targetSource;
    public AudioSource distracterSource;
    public GameObject sentenceUI;

    // Start is called before the first frame update
    void Start()
    {
        sent = new Sentence(wordCount);
        //aFiles = new audioFiles(1);
        lisnData = new LiSN_database(1);

        // make sure to disable UI at load.
        sentenceUI.SetActive(false);        
        sentenceAudio = new AudioClip[wordCount];

        showObjects(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!sceneEntered)
            return;

        // wait for user input to play new sentence
        if (!targetSource.isPlaying)
        {
            // play full sentence
            if (wordIx < wordCount && sentenceReady)
            {
                targetSource.PlayOneShot(sent.audio[wordIx++]);
            }
            // open UI element after playing last word
            else if(sentenceReady)
            {
                // a new sentence has to be created...
                sentenceReady = false;
                Debug.Log("Set UI active");
                // show UI element
                sentenceUI.SetActive(true);
            }
            // wait for user input before creating new sentence
            else if (Input.GetKeyDown(KeyCode.Space))
            {            
                sent.createSentence(lisnData);
                wordIx = 0;
                sentenceReady = true;
            }
        }
    }

    /*
    // make this prettier with references... (word class or word[][] or something...)
    // Return count different words from the selected group (via wordIndex [e.g. verbs], excluding the correct one<
    public string[] getFalseWordsFromGroup(int groupIndex, int count)
    {
        if (groupIndex > wordCount || count > wordCount - 1)
            return null;

        string[] retString = new string[count];
        string correctWord = sent.getWordFromSentence(groupIndex);
        string tmp;
        int[] usedWords = new int[count];

        switch (groupIndex)
        {
            case 0:
                // get count DIFFERENT words, that are not the correct word
                for (int i = 0; i < count; i++)
                {
                    // generate new rand int to access word
                    usedWords[i] = Random.Range(0, clipCount);
                    
                    for(int j = 0; j < i; j++)
                    {
                        if(usedWords[j] == usedWords[i])
                        {
                            // reroll and set j back to 0
                            usedWords[i] = Random.Range(0, clipCount);
                            j = 0;
                        }
                    }
                    // get random word
                    tmp = lisnData.subjects[i].ToString();
                    if(tmp != correctWord)
                    {
                        retString[i] = tmp;
                    }
                    else
                    {
                        i--;
                    }
                }
                break;
        }


        for(int i = 0; i < count; i++)
        {
            retString[i] = sentenceAudio[groupIndex].ToString();
        }

        return retString;
    }
    */

    /**
     * Return 'count' words of a group (determined by 'wordIndex') from the current database.
     * The "correct" word (which is used in the current sentence) will always be the first one (index 0 of returned string array)
     * There are no doubling of words allowed.
     * 
     * @param wordIndex - determines word group by index (e.g. 'adjective' or 'object') 
     * @param count     - determines how many words shall be returned
     * return           - array of strings, with correct word a index 0
     *                    OR: null if wordIndex or count was invalid.
     * 
     */
    public string[] getUserWordSelection(int wordIndex, int count)
    {
        // invalid parameters (also exclude 'the')
        if (wordIndex >= lisnData.getLen() || count >= lisnData.getOptions() || wordIx == 0)
        {
            return null;
        }

        // hold selected strings to be returned
        string[] retStr = new string[count];
        // keep track of already selected words via their indices
        int[] wordIxs = new int[count];
        bool match = false;

        // write the correct word at index 0
        retStr[0] = sent.getWordFromSentence(wordIndex);
        wordIxs[0] = sent.getWordIxFromSentence(wordIndex);
        // copy other words to [1...count-1]
        retStr.CopyTo(lisnData.getWordsByGroup(wordIndex, count), 1);
        return retStr;
    }

    public string[] getCurrentSentence()
    {
        return sent.getSentenceString();
    }


    public void NextWordBtn()
    {
        Debug.Log("NextWord pressed");
        sentenceUI.SetActive(false);
    }


    //// Seperate spawning talker/distractor from the whole Audio/word selection stuff...
    ///
    // for now: same as different voice due to lack of assets...
    public void loadSameVoice()
    {

    }

    public void loadDifferentVoice()
    {

    }

    public void setObjectPositions(int selector)
    {
        DistractorObj.transform.position = PlayerCamera.transform.position + talkerPos;

        switch (selector)
        {
            case 0:
                DistractorObj.transform.position = PlayerCamera.transform.position + distractorPos1;
                break;
            case 1:
                DistractorObj.transform.position = PlayerCamera.transform.position + distractorPos2;
                break;
            case 2:
                DistractorObj.transform.position = PlayerCamera.transform.position + distractorPos3;
                break;
        }
    }

    public void showObjects(bool show)
    {
        TalkerObj.SetActive(show);
        DistractorObj.SetActive(show);
        sceneEntered = show;
    }

}
