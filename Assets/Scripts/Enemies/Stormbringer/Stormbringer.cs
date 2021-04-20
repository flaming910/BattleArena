using System;
using System.Collections;
using Unity.VisualScripting;
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
        Meleeing,
        Null
    }

    //State management
    private BossState bossState;


    [SerializeField] private GameObject stormCall;
    [SerializeField] private GameObject thunderWave;

    //Attack Weightings
    [SerializeField] private int stormCallWeighting;
    [SerializeField] private int thunderWaveWeighting;
    [SerializeField] private int leapWeighting;
    [SerializeField] private int meleeWeighting; //Chance of melee if player is in range[HIGH]
    private int weightingWithMelee;
    private int weightingWithoutMelee;

    private int leapDamage;
    private float timeBetweenAttacks;
    private float initialWait;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        timeBetweenAttacks = 0.25f;
        initialWait = timeBetweenAttacks;
        weightingWithMelee = stormCallWeighting + thunderWaveWeighting + leapWeighting + meleeWeighting;
        weightingWithoutMelee = stormCallWeighting + thunderWaveWeighting + leapWeighting;
    }

    // Update is called once per frame
    private void Update()
    {
        var playerPos = GameManager.Instance.PlayerPosition;
        playerPos.y = transform.position.y;
        transform.LookAt(playerPos);
        if (Input.GetKeyDown(KeyCode.U))
        {
            Leap();
        }
//        return; //This basically removes its brain

        if (initialWait > 0)
        {
            initialWait -= Time.deltaTime;
            return;
        }

        if (bossState == BossState.StartingAnAttack)
        {
            DetermineAttack();
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
                ThunderWave(0);
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
                ThunderWave(0);
            }
            else {
                Leap();
            }
        }
    }

    private void StormCall()
    {
        int damage = 4;
        int stormsToCall = 0;
        float windup = 0;
        float delay = 0;
        float attackDuration = 2.75f;
        switch (phase)
        {
            case 1:
                stormsToCall = 3;
                windup = 0.885f;
                delay = attackDuration/stormsToCall;
                break;
            case 2:
                stormsToCall = 5;
                windup = 0.685f;
                delay = attackDuration/stormsToCall;
                break;
            case 3:
                stormsToCall = 8;
                windup = 0.485f;
                delay = attackDuration/stormsToCall;
                break;
            case 4:
                stormsToCall = 12;
                windup = 0.285f;
                delay = attackDuration/stormsToCall;
                break;
        }

        bossState = BossState.StormCalling;
        StartCoroutine(StormCallAttack(damage, stormsToCall, windup, delay));
    }

    private void ThunderWave(int phaseOverride)
    {
        int damage = 8;
        int waves = 1;
        float speed = 1;
        float delay = 1;
        if (phaseOverride == 0)
        {
            phaseOverride = phase;
        }
        switch (phaseOverride)
        {
            case 1:
                damage = 8;
                waves = 1;
                speed = 1;
                delay = 0.65f;
                break;
            case 2:
                damage = 12;
                waves = 2;
                delay = 1.1f;
                speed = 1.2f;
                break;
            case 3:
                damage = 16;
                waves = 3;
                delay = 0.95f;
                speed = 1.45f;
                break;
            case 4:
                damage = 20;
                waves = 4;
                delay = 0.7f;
                speed = 1.8f;
                break;
        }

        bossState = BossState.ThunderWaving;
        StartCoroutine(ThunderWaveAttack(damage, waves, speed, delay));
    }

    private void Leap()
    {
        switch (phase)
        {
            case 1:
                leapDamage = 10;
                break;
            case 2:
                leapDamage = 13;
                break;
            case 3:
                leapDamage = 17;
                break;
            case 4:
                leapDamage = 22;
                break;
        }

        bossState = BossState.Leaping;
        StartCoroutine(LeapAttack());
    }

    private void Melee()
    {
        int damage = 16;
    }
    #endregion

    //These are to handle the attacks over multiple frames
    #region Attacks
    private IEnumerator StormCallAttack(int damage, int stormsToCall, float windup, float delay)
    {
        while (stormsToCall > 0)
        {
            stormsToCall--;
            Vector3 playerPos = GameManager.Instance.PlayerPosition;
            playerPos.y = 0;
            var storm = Instantiate(stormCall, playerPos, Quaternion.identity);
            storm.GetComponent<StormCall>().SetValues(damage, windup);
            //TODO: Play an animation here
            yield return new WaitForSeconds(delay);
        }
        yield return new WaitForSeconds(windup - delay);
        yield return new WaitForSeconds(timeBetweenAttacks);
        bossState = BossState.StartingAnAttack;
    }

    private IEnumerator ThunderWaveAttack(int damage, int waves, float speed, float delay)
    {
        float lifetime = 1.75f;
        while (waves > 0)
        {
            waves--;
            Vector3 stormBringerPos = transform.position;
            stormBringerPos.y = 1;
            var wave = Instantiate(thunderWave, stormBringerPos, Quaternion.identity);
            wave.GetComponent<ThunderWave>().SetValues(damage, speed, lifetime);
            //TODO: Play an animation here
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(timeBetweenAttacks + lifetime - delay);
        bossState = BossState.StartingAnAttack;
    }

    private IEnumerator LeapAttack()
    {
        var startingPos = transform.position;

        var halfWayPos = startingPos;
        halfWayPos.y += 8;
        float timePassed = 0;
        rigidBody.useGravity = false;
        while (Vector3.Distance(transform.position, halfWayPos) > 0.02f)
        {
            timePassed += Time.deltaTime;
            transform.position = Vector3.Lerp(startingPos, halfWayPos, timePassed / 0.52f);
            if (timePassed >= 0.5f)
            {
                transform.position = halfWayPos;
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        timePassed = 0;
        var targetPos = GameManager.Instance.PlayerPosition;
        targetPos.y = startingPos.y;
        startingPos = transform.position;
        while (Vector3.Distance(transform.position, targetPos) > 0.02f)
        {
            timePassed += Time.deltaTime;
            transform.position = Vector3.Slerp(startingPos, targetPos, timePassed / 0.33f);
            if (timePassed >= 0.33f)
            {
                transform.position = targetPos;
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        rigidBody.useGravity = true;
        switch (phase)
        {
            case 1:
                yield return new WaitForSeconds(timeBetweenAttacks);
                bossState = BossState.StartingAnAttack;
                break;
            case 2:
                ThunderWave(1);
                break;
            case 3:
                ThunderWave(1);
                break;
            case 4:
                ThunderWave(1);
                break;
        }
    }

    private void MeleeAttack()
    {

    }
    #endregion


    //Handle contact damage
    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        foreach (var contact in other.contacts)
        {
            if (contact.thisCollider.transform.parent == this.transform)
            {
                var player = other.gameObject.GetComponent<PlayerControl>();
                var stormBringerPos = transform.position;
                var playerPos = player.gameObject.transform.position;
                stormBringerPos.y = 0;
                playerPos.y = 0;
                if (bossState == BossState.Leaping)
                {
                    player.TakeDamage(leapDamage, false);

                    Vector3 direction = (stormBringerPos - playerPos).normalized;
                    if (direction == Vector3.zero)
                    {
                        var unitCircle = Random.insideUnitCircle.normalized;
                        direction = new Vector3(unitCircle.x, 0, unitCircle.y);
                    }
                    player.Dislocate(direction * -1.5f, 5);
                    bossState = BossState.Null;
                }
                else
                {
                    Vector3 direction = (stormBringerPos - playerPos).normalized;
                    player.Dislocate(direction * -1f, 2);
                    player.TakeDamage(contactDamage, false);
                }
            }
        }
    }

}
