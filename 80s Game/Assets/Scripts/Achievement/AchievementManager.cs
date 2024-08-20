using System.Collections.Generic;
using UnityEngine;
using static AchievementData;

public static class AchievementManager
{
    public static Dictionary<string, int> requirements;
    private static Dictionary<string, AchievementData> lookupTable;
    private static Queue<string> rewards;
    private static bool originalSetup = false;
    private static Dictionary<int, int> fullyChargedByPlayer;
    private static bool countingUnstable = false;
    private static int unstableCount = 0;
    private static UnstableBatStateMachine counterOrigin;
    private static UnstableDefenseBatStateMachine counterDefenseOrigin;

    /// <summary>
    /// Register the requirement values for unlocking achievements.
    /// </summary>
    /// <param name="achievements">The list of achievements in the game</param>
    public static void RegisterRequirements(List<AchievementData> achievements)
    {

        if (!originalSetup)
        {
            requirements = new Dictionary<string, int>();
            lookupTable = new Dictionary<string, AchievementData>();
            rewards = new Queue<string>();
            fullyChargedByPlayer = new Dictionary<int, int>();
            for (int i = 0; i < achievements.Count; i++)
            {
                AchievementData achievement = achievements[i];
                requirements[achievement.internalAchivementKey] = achievement.testValue;
                lookupTable[achievement.internalAchivementKey] = achievement;
            }
            originalSetup = true;
        }
    }

    public static AchievementData GetNextReward()
    {
        if (rewards.Count == 0)
        {
            return null;
        }

        string achievementKey = rewards.Dequeue();
        return lookupTable[achievementKey];
    }

    /// <summary>
    /// Returns whether an achievement has been unlocked
    /// </summary>
    /// <param name="name">The key of the achievement to test</param>
    /// <returns>If this achievement has been unlocked in the past</returns>
    public static bool HasBeenUnlocked(string name)
    {
        int value = PlayerPrefs.GetInt(name, 0);
        return value == 1;
    }

    /// <summary>
    /// Register tracking data for achievement purposes
    /// </summary>
    /// <param name="name">The key of data to track</param>
    /// <param name="value">The value to register</param>
    public static void RegisterData(string name, int value)
    {
        PlayerPrefs.SetInt(name, value);
    }

    /// <summary>
    /// Get data from the register
    /// </summary>
    /// <param name="name">The key to fetch</param>
    /// <returns>The value currently stored in the register</returns>
    public static int GetData(string name) { 
    
        return PlayerPrefs.GetInt(name, 0);
    }

    /// <summary>
    /// Procedure to permanently mark an achievement as unlocked
    /// </summary>
    /// <param name="name">The key of the achievement to unlock. Will get saved to PlayerPrefs</param>
    public static void UnlockAchievement(string name)
    {
        /*
        if (HasBeenUnlocked(name))
        {
            return;
        }
        
        Debug.Log("Unlocking Achievement " + name);
        GameManager.Instance.UIManager.EnqueueAchievementNotification(GetAchievementByKey(name));
        PlayerPrefs.SetInt(name , 1);
        rewards.Enqueue(name);
        TestForPlat();*/
    }

    /// <summary>
    /// Look up an achievement by use of its key
    /// </summary>
    /// <param name="key">Key for lookup. Should come from the list found in AchievementConstants</param>
    /// <returns>The AchivementData ScriptableObject</returns>
    public static AchievementData GetAchievementByKey(string key)
    {
        return lookupTable[key];
    }

    /// <summary>
    /// Tests whether to unlock the plat achievement or not
    /// </summary>
    private static void TestForPlat()
    {
        bool plat = true;
        foreach (KeyValuePair<string, int> kvp in requirements)
        {
            if (GetData(kvp.Key) == 0)
            {
                plat = false;
                break;
            }
        }

        if (plat) {
            //UnlockAchievement(AchievementConstants.EMPLOYEE_OF_THE_MONTH);
        }
    }

    /// <summary>
    /// Test two values to see if an achievement meets unlock requirements
    /// </summary>
    /// <param name="testType">Type of test to perform</param>
    /// <param name="expectedValue">Value required to unlock</param>
    /// <param name="testValue">The value to test</param>
    /// <returns>Whether the test determines an achievement should be unlocked </returns>
    private static bool TestUnlock(TestType testType, int expectedValue, int testValue)
    {
        bool testSuccessful = false;
        switch (testType)
        {
            case TestType.GreaterThan:
                testSuccessful = expectedValue < testValue;
                break;
            case TestType.LessThan:
                testSuccessful = expectedValue > testValue;
                break;
            case TestType.LessThanOrEqual:
                testSuccessful = expectedValue >= testValue;
                break;
            case TestType.GreaterThanOrEqual:
                testSuccessful = expectedValue <= testValue;
                break;
            default:
                testSuccessful = expectedValue == testValue;
                break;
        }

        return testSuccessful;
    }

    /// <summary>
    /// Test two values to see if an achievement meets unlock requirements
    /// </summary>
    /// <param name="testType">Type of test to perform</param>
    /// <param name="expectedValue">Value required to unlock</param>
    /// <param name="testValue">The value to test</param>
    /// <returns>Whether the test determines an achievement should be unlocked </returns>
    public static bool TestUnlock(TestType testType, float expectedValue, float testValue)
    {
        bool testSuccessful = false;
        switch (testType)
        {
            case TestType.GreaterThan:
                testSuccessful = expectedValue < testValue;
                break;
            case TestType.LessThan:
                testSuccessful = expectedValue > testValue;
                break;
            case TestType.LessThanOrEqual:
                testSuccessful = expectedValue >= testValue;
                break;
            case TestType.GreaterThanOrEqual:
                testSuccessful = expectedValue <= testValue;
                break;
            default:
                testSuccessful = expectedValue == testValue;
                break;
        }

        return testSuccessful;
    }

    /// <summary>
    /// Test the end-of-game-mode achievements
    /// </summary>
    /// <param name="gameMode">Game Mode to guide tests by</param>
    /// <param name="currentRound">The current round</param>
    /// <param name="score">The score the player has achieved</param>
    public static void TestEndGameAchievements(EGameMode gameMode, int currentRound, int score)
    {
        TestType greaterTest = TestType.GreaterThanOrEqual;
        string key;
        switch (gameMode)
        {
            case EGameMode.Classic:
                key = AchievementConstants.TIMELESS_CLASSIC;
                RegisterData(lookupTable[key].requirementTrackingKey, score);
                TestAndUnlock(key, requirements[key], score, greaterTest);
                key = AchievementConstants.CLASSIC_ENJOYER;
                RegisterData(lookupTable[key].requirementTrackingKey, score);
                TestAndUnlock(key, requirements[key], score, greaterTest);
                key = AchievementConstants.CLASSIC_FAN;
                RegisterData(lookupTable[key].requirementTrackingKey, score);
                TestAndUnlock(key, requirements[key], score, greaterTest);
                key = AchievementConstants.PROUD_OF_YOU;
                TestAndUnlock(key, requirements[key], score, TestType.EqualTo);
                break;
            case EGameMode.Competitive:
                key = AchievementConstants.PROUD_OF_YOU;
                TestAndUnlock(key, requirements[key], score, TestType.EqualTo);
                break;
            case EGameMode.Defense:
                key = AchievementConstants.BULWARK_OF_RESISTANCE;
                RegisterData(lookupTable[key].requirementTrackingKey, currentRound);
                TestAndUnlock(key, requirements[key], currentRound, greaterTest);
                key = AchievementConstants.STAUNCH_DEFENDER;
                RegisterData(lookupTable[key].requirementTrackingKey, currentRound);
                TestAndUnlock(key, requirements[key], currentRound, greaterTest);
                key = AchievementConstants.PROMPT_PROTECTOR;
                RegisterData(lookupTable[key].requirementTrackingKey, currentRound);
                TestAndUnlock(key, requirements[key], currentRound, greaterTest);
                break;
        }
    }

    /// <summary>
    /// Handle all stun-related achievements
    /// </summary>
    /// <param name="type">The stunned target type</param>
    public static void HandleStunAchievements(TargetManager.TargetType type)
    {
        int stunnedBatsOfThisType = 0;
        string dataKey = "";
        HandleAccuracyAchievements();
        // Test for the highest thing first. This can bypass execution of all other checks
        switch (type)
        {
            /*case TargetManager.TargetType.Unstable:
                if (HasBeenUnlocked(AchievementConstants.UNSTABLE_EXPERT))
                {
                    return;
                }
                dataKey = lookupTable[AchievementConstants.UNSTABLE_EXPERT].requirementTrackingKey;
                stunnedBatsOfThisType = GetData(dataKey);
                stunnedBatsOfThisType++;
                break;*/
            case TargetManager.TargetType.Modifier:
                if (HasBeenUnlocked(AchievementConstants.MOD_BAT_EXPERT))
                {
                    return;
                }
                dataKey = lookupTable[AchievementConstants.MOD_BAT_EXPERT].requirementTrackingKey;
                stunnedBatsOfThisType = GetData(dataKey);
                stunnedBatsOfThisType++;
                break;
            /*case TargetManager.TargetType.LowBonus:
            case TargetManager.TargetType.HighBonus:
                if (HasBeenUnlocked(AchievementConstants.BONUS_EXPERT))
                {
                    return;
                }
                dataKey = lookupTable[AchievementConstants.BONUS_EXPERT].requirementTrackingKey;
                stunnedBatsOfThisType = GetData(dataKey);
                stunnedBatsOfThisType++;
                break;
            case TargetManager.TargetType.Regular:
                if (HasBeenUnlocked(AchievementConstants.MK1_EXPERT))
                {
                    return;
                }
                dataKey = lookupTable[AchievementConstants.MK1_EXPERT].requirementTrackingKey;
                stunnedBatsOfThisType = GetData(dataKey);
                stunnedBatsOfThisType++;
                break;*/
        }
        TestStunUnlockByType(type, dataKey, stunnedBatsOfThisType);
    }

    /// <summary>
    /// Test whether the type of stunned bat has unlocked related achievements
    /// </summary>
    /// <param name="type">Type of the stunned bat</param>
    /// <param name="dataKey">The tracked key for this bat type</param>
    /// <param name="stunnedBatsOfThisType">The stun count for this type</param>
    private static void TestStunUnlockByType(TargetManager.TargetType type, string dataKey, int stunCount)
    {
        string key;
        switch (type)
        {
            /*case TargetManager.TargetType.Unstable:
                key = AchievementConstants.UNSTABLE_EXPERT;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                key = AchievementConstants.UNSTABLE_ADEPT;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                key = AchievementConstants.UNSTABLE_NOVICE;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                break;*/
            case TargetManager.TargetType.Modifier:
                key = AchievementConstants.MOD_BAT_EXPERT;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                key = AchievementConstants.MOD_BAT_ADEPT;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                key = AchievementConstants.MOD_BAT_NOVICE;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                break;
            /*case TargetManager.TargetType.LowBonus:
            case TargetManager.TargetType.HighBonus:
                key = AchievementConstants.BONUS_EXPERT;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                key = AchievementConstants.BONUS_ADEPT;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                key = AchievementConstants.BONUS_NOVICE;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                break;
            case TargetManager.TargetType.Regular:
                key = AchievementConstants.MK1_EXPERT;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                key = AchievementConstants.MK1_ADEPT;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                key = AchievementConstants.MK1_NOVICE;
                TestAndUnlock(key, requirements[key], stunCount, TestType.GreaterThanOrEqual);
                break;*/
        }
        RegisterData(dataKey, stunCount);
    }

    /// <summary>
    /// A more complete wrapper for the TestUnlock function
    /// </summary>
    /// <param name="key">The achievement key to test</param>
    /// <param name="expected">Required or expected value for unlock</param>
    /// <param name="actual">Actual value obtained by the player</param>
    /// <param name="testType">Which type of test to use</param>
    private static void TestAndUnlock(string key, int expected, int actual, TestType testType)
    {
        /*if (TestUnlock(testType, expected, actual))
        {
            UnlockAchievement(key);
        }*/
    }

    /// <summary>
    /// Handle accuracy achievements
    /// </summary>
    private static void HandleAccuracyAchievements()
    {
        string key = AchievementConstants.HAWKEYE;
        int currentKillCount = GetData(lookupTable[key].requirementTrackingKey);
        currentKillCount++;
        TestAndUnlock(key, requirements[key], currentKillCount, TestType.GreaterThanOrEqual);
        key = AchievementConstants.SHARPSHOOTER;
        TestAndUnlock(key, requirements[key], currentKillCount, TestType.GreaterThanOrEqual);
        key = AchievementConstants.MARKSMAN;
        TestAndUnlock(key, requirements[key], currentKillCount, TestType.GreaterThanOrEqual);
        RegisterData(lookupTable[AchievementConstants.HAWKEYE].requirementTrackingKey, currentKillCount);
    }

    /// <summary>
    /// Handle the test for the Unfazed achievement
    /// </summary>
    /// <param name="stunningPlayerOrder"></param>
    public static void UnfazedTest(int stunningPlayerOrder)
    {
        int confusionCount = GetData("confmod-pi-" + stunningPlayerOrder.ToString());
        string key = AchievementConstants.UNFAZED;
        if (TestUnlock(TestType.GreaterThanOrEqual, requirements[key], confusionCount+1))
        {
            //UnlockAchievement(key);
            return;
        }
        RegisterData("confmod-pi-" + stunningPlayerOrder.ToString(), confusionCount + 1);
    }

    /// <summary>
    /// Handle the test for the Bullseye! achievement
    /// </summary>
    /// <param name="player">The player controller to test</param>
    public static void BullseyeTest(PlayerController player)
    {
        PlayerScoreController scoreController = player.scoreController;
        int accuracy = (int)(scoreController.GetAccuracy()*100);
        TestAndUnlock(AchievementConstants.BULLSEYE, requirements[AchievementConstants.BULLSEYE], accuracy, TestType.GreaterThanOrEqual);
    }

    public static void ResetOverchargedByPlayer(int player)
    {
        fullyChargedByPlayer[player] = 0;
    }

    public static void AddToFCCount(int player)
    {
        fullyChargedByPlayer[player]++;
        if (TestUnlock(TestType.GreaterThanOrEqual, requirements[AchievementConstants.FULLY_CHARGED], fullyChargedByPlayer[player]))
        {
            //UnlockAchievement(AchievementConstants.FULLY_CHARGED);
        }
    }

    public static void StartCountingUnstable(UnstableBatStateMachine machine)
    {
        countingUnstable = true;
        unstableCount = 1;
        counterOrigin = machine;
    }

    public static void StopCountingUnstable(UnstableBatStateMachine machine)
    {
        if (machine == counterOrigin)
        {
            countingUnstable = false;
            if(TestUnlock(TestType.GreaterThanOrEqual, requirements[AchievementConstants.GREASED_LIGHTNING], unstableCount))
            {
                //UnlockAchievement(AchievementConstants.GREASED_LIGHTNING);
            }
            counterOrigin = null;
        }
    }

    public static void StartCountingDefenseUnstable(UnstableDefenseBatStateMachine machine)
    {
        countingUnstable = true;
        unstableCount = 1;
        counterDefenseOrigin = machine;
    }

    public static void StopCountingDefenseUnstable(UnstableDefenseBatStateMachine machine)
    {
        if (machine == counterDefenseOrigin)
        {
            countingUnstable = false;
            if(TestUnlock(TestType.GreaterThanOrEqual, requirements[AchievementConstants.GREASED_LIGHTNING], unstableCount))
            {
                //UnlockAchievement(AchievementConstants.GREASED_LIGHTNING);
            }
            counterDefenseOrigin = null;
        }
    }

    public static bool isCountingUnstable()
    {
        return countingUnstable;
    }

    public static void AddToUnstCount() {
        unstableCount++;
    }

    public static void ResetTrackingKeys()
    {
        foreach(KeyValuePair<string, AchievementData> kvp_achiev in lookupTable)
        {
            if(kvp_achiev.Value.requirementTrackingKey == null || kvp_achiev.Value.requirementTrackingKey == ""){
                continue;
            }
            RegisterData(kvp_achiev.Value.requirementTrackingKey, 0);
        }
    }

    public static void SetAchievementStatus(int value)
    {
        if (value == 0)
        {
            ResetTrackingKeys();
        }
        PlayerPrefs.SetInt(AchievementConstants.CLASSIC_FAN,value);
        PlayerPrefs.SetInt(AchievementConstants.CLASSIC_ENJOYER,value);
        PlayerPrefs.SetInt(AchievementConstants.TIMELESS_CLASSIC,value);
        PlayerPrefs.SetInt(AchievementConstants.PROMPT_PROTECTOR,value);
        PlayerPrefs.SetInt(AchievementConstants.STAUNCH_DEFENDER,value);
        PlayerPrefs.SetInt(AchievementConstants.BULWARK_OF_RESISTANCE,value);
        PlayerPrefs.SetInt(AchievementConstants.MK1_NOVICE,value);
        PlayerPrefs.SetInt(AchievementConstants.MK1_ADEPT,value);
        PlayerPrefs.SetInt(AchievementConstants.MK1_EXPERT,value);
        PlayerPrefs.SetInt(AchievementConstants.UNSTABLE_NOVICE,value);
        PlayerPrefs.SetInt(AchievementConstants.UNSTABLE_ADEPT,value);
        PlayerPrefs.SetInt(AchievementConstants.UNSTABLE_EXPERT,value);
        PlayerPrefs.SetInt(AchievementConstants.BONUS_NOVICE,value);
        PlayerPrefs.SetInt(AchievementConstants.BONUS_ADEPT,value);
        PlayerPrefs.SetInt(AchievementConstants.BONUS_EXPERT,value);
        PlayerPrefs.SetInt(AchievementConstants.MOD_BAT_NOVICE,value);
        PlayerPrefs.SetInt(AchievementConstants.MOD_BAT_ADEPT,value);
        PlayerPrefs.SetInt(AchievementConstants.MOD_BAT_EXPERT,value);
        PlayerPrefs.SetInt(AchievementConstants.UNFAZED,value);
        PlayerPrefs.SetInt(AchievementConstants.KITTED_OUT,value);
        PlayerPrefs.SetInt(AchievementConstants.FULLY_CHARGED,value);
        PlayerPrefs.SetInt(AchievementConstants.BOMB_VOYAGE,value);
        PlayerPrefs.SetInt(AchievementConstants.GREASED_LIGHTNING,value);
        PlayerPrefs.SetInt(AchievementConstants.WELL_THATS_AWKWARD,value);
        PlayerPrefs.SetInt(AchievementConstants.BULLSEYE,value);
        PlayerPrefs.SetInt(AchievementConstants.CAREFUL_FRAGILE,value);
        PlayerPrefs.SetInt(AchievementConstants.PROUD_OF_YOU,value);
        PlayerPrefs.SetInt(AchievementConstants.MARKSMAN,value);
        PlayerPrefs.SetInt(AchievementConstants.SHARPSHOOTER,value);
        PlayerPrefs.SetInt(AchievementConstants.HAWKEYE,value);
        PlayerPrefs.SetInt(AchievementConstants.EMPLOYEE_OF_THE_MONTH,value);
    }
}