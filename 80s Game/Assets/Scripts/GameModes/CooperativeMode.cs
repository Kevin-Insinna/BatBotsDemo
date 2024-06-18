using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooperativeMode : AbsGameMode
{
    float modifierChance = 0.3f;

    public CooperativeMode() : base()
    {
        ModeType = EGameMode.Competitive;

        // Initial round parameters
        NumRounds = 15;
        maxTargetsOnScreen = 15;
        currentRoundTargetCount = 8;
    }

    protected override void StartNextRound(bool isFirstRound = false)
    {
        if (!isFirstRound)
            UpdateRoundParams();

        // Iterate through and spawn the next set of targets
        for (int i = 0; i < currentRoundTargetCount; i++)
        {
            int targetIndex = GetNextAvailableBat();

            if (targetIndex == -1)
                continue;

            targetManager.SpawnTarget(targetIndex);

            // Check if screen is now full
            if (targetManager.ActiveTargets.Count == maxTargetsOnScreen)
                return;
        }

        // Spawn a modifier bat and increment target count
        targetManager.SpawnTarget(targetManager.GetNextAvailableTargetOfType<ModifierBatStateMachine>());
        currentRoundTargetCount++;
    }

    protected override void UpdateRoundParams()
    {
        CurrentRound++;
        currentRoundTargetCount += 4;
        maxTargetsOnScreen += 1;
        // Keep max targets on screen to at most two fewer than object pool
        if (maxTargetsOnScreen >= targetManager.targets.Count)
        {
            maxTargetsOnScreen = targetManager.targets.Count - 2;
        }

        targetManager.numStuns = 0;
        targetManager.UpdateTargetParams();
        if (GameManager.Instance.roundEndTheme != null)
            SoundManager.Instance.PlayNonloopMusic(GameManager.Instance.roundEndTheme);
    }

    protected override int GetNextAvailableBat()
    {
        List<Target> bats = targetManager.targets;

        // Iterate through the targets until you
        // find one that isn't already on screen
        for (int i = 0; i < bats.Count; i++)
        {
            if (bats[i].FSM.bIsActive)
                continue;

            ModifierBatStateMachine comp = bats[i].GetComponent<ModifierBatStateMachine>();
            if (comp != null)
                continue;

            // If default bat, return index if no bonus bats
            // Otherwise continue
            if (bats[i].FSM.IsDefault && numBonusBats == 0)
            {
                return i;
            }
            else if (numBonusBats > 0) // Bonus bat spawning
            {
                numBonusBats--;
                return i;
            }
        }

        return -1;
    }

    public override void OnTargetReset()
    {
        // Increment bonus bats every 3 stuns
        if (targetManager.totalStuns % 3 == 0)
            numBonusBats++;

        // Chance to spawn a modifier bat every 5 stuns
        if(targetManager.totalStuns % 5 == 0)
        {
            if(Random.Range(0.0f, 1.0f) < modifierChance)
            {
                // Spawn a modifier bat and increment target count
                targetManager.SpawnTarget(targetManager.GetNextAvailableTargetOfType<ModifierBatStateMachine>());
                currentRoundTargetCount++;
            }
        }

        if (targetManager.numStuns >= currentRoundTargetCount)
        {

            // If last round completed
            if (CurrentRound == NumRounds)
            {
                GameOver = true;
                EndGame();
            }
                
            // Otherwise start next round
            else
                StartNextRound();

            return;
        }

        // If not the end of a round, check if more targets can be spawned
        SpawnMoreTargets();
    }

    private void SpawnMoreTargets()
    {
        // Check if the player still needs stuns for the round
        int neededStuns = currentRoundTargetCount - targetManager.numStuns;
        if (neededStuns <= 0)
            return;

        // If maximum number of targets isn't on screen
        if (
            targetManager.ActiveTargets.Count < maxTargetsOnScreen
            && targetManager.ActiveTargets.Count < neededStuns
        )
        {
            int targetIndex = GetNextAvailableBat();

            if (targetIndex >= 0)
                targetManager.SpawnTarget(targetIndex);
        }
    }
}
