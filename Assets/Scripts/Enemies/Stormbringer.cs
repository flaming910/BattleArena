using UnityEngine;
using Random = UnityEngine.Random;

public class Stormbringer : BossBase
{
    private enum BossState
    {
        StartingAnAttack,
        StormCalling,
        ThunderWaving,
        Leaping,
        Meleeing
    }

    //State management
    private BossState bossState;

    //Attack Weightings
    [SerializeField] private int stormCallWeighting;
    [SerializeField] private int thunderWaveWeighting;
    [SerializeField] private int leapWeighting;
    [SerializeField] private int meleeWeighting; //Chance of melee if player is in range[HIGH]
    private int weightingWithMelee;
    private int weightingWithoutMelee;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        weightingWithMelee = stormCallWeighting + thunderWaveWeighting + leapWeighting + meleeWeighting;
        weightingWithoutMelee = stormCallWeighting + thunderWaveWeighting + leapWeighting;
    }

    // Update is called once per frame
    private void Update()
    {

        switch (bossState)
        {
            case BossState.StartingAnAttack:
                DetermineAttack();
                break;
            case BossState.StormCalling:
                StormCallAttack();
                break;
            case BossState.ThunderWaving:
                ThunderWaveAttack();
                break;
            case BossState.Leaping:
                LeapAttack();
                break;
            case BossState.Meleeing:
                MeleeAttack();
                break;
        }

    }

    //This gets the boss ready to do its attacks over the course of the frame
    #region Attack Set Up
    private void DetermineAttack()
    {
        if(playerInRange) {
            int attack = Random.Range(0, weightingWithMelee);
            if(attack <= stormCallWeighting) {
                StormCall();
            }
            else if(attack <= stormCallWeighting + thunderWaveWeighting) {
                ThunderWave();
            }
            else if(attack <= stormCallWeighting + thunderWaveWeighting + leapWeighting) {
                Leap();
            }
            else {
                Melee();
            }
        }
        else {
            int attack = Random.Range(0, weightingWithoutMelee);
            if(attack <= stormCallWeighting) {
                StormCall();
            }
            else if(attack <= stormCallWeighting + thunderWaveWeighting) {
                ThunderWave();
            }
            else {
                Leap();
            }
        }
    }

    private void StormCall()
    {
        int damage = 4;
        int stormsToCall;
        float windup;
        float delayBetweenStorms;
        switch (phase)
        {
            case 1:
                stormsToCall = 3;
                windup = 1;
                delayBetweenStorms = 0.7f;
                break;
            case 2:
                stormsToCall = 5;
                windup = 0.8f;
                delayBetweenStorms = 0.6f;
                break;
            case 3:
                stormsToCall = 8;
                windup = 0.6f;
                delayBetweenStorms = 0.45f;
                break;
            case 4:
                stormsToCall = 12;
                windup = 0.4f;
                delayBetweenStorms = 0.25f;
                break;
        }

    }

    private void ThunderWave()
    {
        int damage = 8;
    }

    private void Leap()
    {
        int damage = 10;
    }

    private void Melee()
    {
        int damage = 16;
    }
    #endregion

    //These are to handle the attacks over multiple frames
    #region Attacks
    private void StormCallAttack()
    {

    }

    private void ThunderWaveAttack()
    {

    }

    private void LeapAttack()
    {

    }

    private void MeleeAttack()
    {

    }


    #endregion

}
