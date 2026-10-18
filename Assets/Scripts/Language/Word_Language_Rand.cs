using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Word_Language_Rand : BaseMono
{

    public bool isAlphabetBased;
    public bool isRandLetterBased;
    public bool isWordBased;


    [SerializeField] LetterBuilder alphaBuild, randLetterBuild, wordBasedBuild;
    public LetterBuilder usedBuilder;


    [SerializeField] List<string> tones, vowels, clicks, miscs;
    List<string> usedTones = new List<string>(), usedVowels = new List<string>(), usedClicks = new List<string>(), usedMiscs = new List<string>();
    [SerializeField] int usedTonesCount, usedVowelsCount, usedClicksCount, usedMiscsCount;

    List<string> generatedWords = new List<string>();
    List<string> preffix = new List<string>(), suffix = new List<string>();

    [SerializeField] int minLetterCount = 12, maxLetterCount = 35, minWordCount = 5, maxWordCount = 15, minLetterInGenWord = 1, maxLettersInGenWord = 10,
        min_ffix_Count = 3, max_ffix_Count = 6;

    float spaceChance;

    int letterCount, wordCount, preffixCount, suffixCount;

    bool hasPhraseSuffix, hasPhrasePreffix, hasNameSuffix, hasNamePreffix, hasMagicSuffix, hasMagicPreffix;

    public override void Begin()
    {
        base.Begin();

        float AlphSTR = subrandom(0, 1f);
        float RandSTR = subrandom(0, 1f);
        float WordSTR = subrandom(0, 1f);

        spaceChance = subrandom(0, 1f);

        hasPhraseSuffix = decider5050();
        hasPhrasePreffix = decider5050();
        hasNameSuffix = decider5050();
        hasNamePreffix = decider5050();
        hasMagicSuffix = decider5050();
        hasMagicPreffix = decider5050();

        if(AlphSTR > RandSTR && AlphSTR > WordSTR)
        {
            isAlphabetBased = true;
        }
        if(RandSTR > AlphSTR && RandSTR > WordSTR)
        {
            isRandLetterBased = true;
        }
        if(WordSTR > AlphSTR && WordSTR > RandSTR)
        {
            isWordBased = true;
        }
        if(isWordBased==false && isRandLetterBased==false)
        {
            isAlphabetBased = true;//this is a little check
        }

        preffixCount = (int)subrandom(min_ffix_Count, max_ffix_Count + 1);
        suffixCount = (int)subrandom(min_ffix_Count, max_ffix_Count + 1);

        if (isAlphabetBased)
            usedBuilder = alphaBuild;
        if(isRandLetterBased)
            usedBuilder = randLetterBuild;
        if(isWordBased)
            usedBuilder = wordBasedBuild;

        letterCount = (int)subrandom(minLetterCount, maxLetterCount+1);
        wordCount = (int)subrandom(minWordCount, maxWordCount+1);
        wordCount += 2;//prefix and sufix

        float toneSTR = subrandom(0f,1f), vowelSTR = subrandom(0f, 1f),clickSTR = subrandom(0f, 1f),miscSTR = subrandom(0f, 1f);
        float totalSTR = toneSTR + vowelSTR + clickSTR + miscSTR;

        toneSTR /= totalSTR;
        vowelSTR /= totalSTR;
        clickSTR /= totalSTR;
        miscSTR /= totalSTR;

        usedTonesCount = Mathf.RoundToInt(toneSTR * tones.Count);
        usedVowelsCount = Mathf.RoundToInt(vowelSTR * vowels.Count);
        usedClicksCount = Mathf.RoundToInt(clickSTR * clicks.Count);
        usedMiscsCount = Mathf.RoundToInt(miscSTR * miscs.Count);

        if (usedTonesCount == 0)
            usedTonesCount++;
        if(usedVowelsCount == 0)
            usedVowelsCount++;
        if (usedClicksCount == 0)
            usedClicksCount++;
        if (usedMiscsCount == 0)
            usedMiscsCount++;

        letterCount = usedTonesCount + usedVowelsCount + usedClicksCount + usedMiscsCount;


        usedTones = shuffelString(tones, usedTonesCount);
        usedVowels = shuffelString(vowels, usedVowelsCount);
        usedClicks = shuffelString(clicks, usedClicksCount);
        usedMiscs = shuffelString(miscs, usedMiscsCount);


        for(int i = 0; i < wordCount; i++)
        {
            generatedWords.Add(makeRandomWord((int)subrandom(minLetterInGenWord, maxLettersInGenWord+1), false));
        }

        for(int i = 0; i < preffixCount; i++)
        {
            preffix.Add(makeRandomWord((int)subrandom(minLetterInGenWord, maxLettersInGenWord + 1), false));
        }
        for (int i = 0; i < suffixCount; i++)
        {
            suffix.Add(makeRandomWord((int)subrandom(minLetterInGenWord, maxLettersInGenWord + 1), false));
        }

        List<string> allSymbols = new List<string>();
        if (isRandLetterBased)
        {
            
            allSymbols.AddRange(usedVowels);
            allSymbols.AddRange(usedTones);
            allSymbols.AddRange(usedClicks);
            allSymbols.AddRange(usedMiscs);

            usedBuilder.letterStrings = allSymbols;
        }
        if(isWordBased)
        {
            allSymbols.AddRange(generatedWords);
            allSymbols.AddRange(preffix);
            allSymbols.AddRange(suffix);
            usedBuilder.letterStrings = allSymbols;
        }

        usedBuilder.Begin();
    }

    public void convertWord_or_Phrase(string convWord, int wordSeed, out string pronunciation)
    {
        pronunciation = heartOfLanguage(wordSeed, convWord, hasPhrasePreffix, hasPhraseSuffix);
    }
    public void CreateName(string baseName_optional, int wordSeed, out string pronunciation)
    {
        if(baseName_optional == null || baseName_optional == "")
        {
            baseName_optional = makeRandomWord((int)subrandom(minLetterInGenWord, maxLettersInGenWord), false);
        }
        pronunciation = heartOfLanguage(wordSeed, baseName_optional, hasNamePreffix, hasNameSuffix);

    }

    public void CreateMagicWords(string magicParameters, int wordSeed, out string pronunciation)
    {
        //may need to de-code some magic stuff

        pronunciation = heartOfLanguage(wordSeed, magicParameters, hasMagicPreffix, hasMagicSuffix);

    }

    string heartOfLanguage(int wordSeed, string convWord, bool hasPrefix, bool hasSuffix)
    {
        string pronunciation = "";

        if (isAlphabetBased)
            pronunciation = convWord;
        else
        {
            ResetRandomize(wordSeed);

            pronunciation = "";

            if (hasPrefix)
            {
                pronunciation += preffix[(int)subrandom(0, preffix.Count)];
            }

            if (isRandLetterBased)
            {
                int wordLength = Mathf.RoundToInt((convWord.Length / 26f) * letterCount);
                if (wordLength <= 0)
                    wordLength++;

                pronunciation += makeRandomWord(wordLength, true);

            }
            if (isWordBased)
            {

                int numOfWords = convWord.Length / generatedWords.Count;
                if (numOfWords <= 0)
                    numOfWords++;

                for (int i = 0; i < numOfWords; i++)
                {
                    pronunciation += generatedWords[(int)subrandom(0, generatedWords.Count)];
                }

            }
            if (hasSuffix)
            {
                pronunciation += suffix[(int)subrandom(0, suffix.Count)];
            }

        }
        return pronunciation;
    }

    void ResetRandomize(int newSeed)
    {
        randomIndex = 0;
        usedSeed = newSeed;
        usedSeed = (int)subrandom(0, 999999999);

    }

    string makeRandomWord(int lCount, bool hasSpaceChance)
    {

        string s = "";

        bool vowelAlt = decider5050();
        for (int i = 0;i < lCount;i++)
        {
            if (vowelAlt)
            {
                
                s += usedVowels[(int)subrandom(0, usedVowels.Count)];
            }
            else
            {
                bool toneToBe = decider5050();
                if (decider5050() && decider5050())
                {
                    s += miscs[(int)subrandom(0, usedMiscs.Count)];
                }
                else
                {
                    if (toneToBe)
                    {
                        s += usedTones[(int)subrandom(0, usedTones.Count)];
                    }
                    else
                    {
                        s += usedClicks[(int)subrandom(0, usedClicks.Count)];
                    }
                }
            }


            vowelAlt = !vowelAlt;
            if (subrandom(0,1f) < spaceChance && hasSpaceChance)
            {
                s += " ";
                vowelAlt = decider5050();
            }
        }

        return s;
    }

    List<string> shuffelString(List<string> listToShuffel, int howManyToInclude)
    {

        List<float> scores = new List<float>();
        List<int> indexes = new List<int>();
        foreach(string s in listToShuffel)
        {
            scores.Add(subrandom(0, 1f));
        }
        foreach(float f in scores)
        {
            int inde = 0;
            foreach(float s in scores)
            {
                if(f>s)
                {
                    inde++;
                }
          
            }
            indexes.Add(inde);
        }

        List<string> outS = new List<string>();

        for(int i = 0; i < howManyToInclude; i++)
        {
            int ind = 0;
            foreach(int p in indexes)
            {

                if(i == p)
                {
                    break;
                }

                ind++;
            }
            outS.Add(listToShuffel[ind]);

        }

        return outS;

    }

}
