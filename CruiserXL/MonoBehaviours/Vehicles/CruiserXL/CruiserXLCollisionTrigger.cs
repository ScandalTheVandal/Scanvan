using CruiserXL.Patches;
using GameNetcodeStuff;
using System.ComponentModel;
using UnityEngine;

public class CruiserXLCollisionTrigger : MonoBehaviour
{
    public CruiserXLController mainScript = null!;
    public BoxCollider insideTruckNavMeshBounds = null!;
    public EnemyAI[] enemiesLastHit = null!;

    private float timeSinceHittingPlayer;
    private float timeSinceHittingEnemy;
    private int enemyIndex;

    public void Start()
    {
        enemiesLastHit = new EnemyAI[3];
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!mainScript.hasBeenSpawned || 
            (mainScript.magnetedToShip && mainScript.magnetTime > 0.8f))
            return;

        if (other.CompareTag("Player"))
        {
            PlayerControllerB playerComponent = other.GetComponent<PlayerControllerB>();
            if (playerComponent == null)
                return;

            // Prevent hitting players standing on/in the cruiser
            Transform physicsTransform = mainScript.physicsRegion.physicsTransform;
            if (playerComponent.physicsParent == physicsTransform || playerComponent.overridePhysicsParent == physicsTransform)
                return;

            if ((mainScript.localPlayerInPassengerSeat || mainScript.localPlayerInMiddlePassengerSeat) || Time.realtimeSinceStartup - timeSinceHittingPlayer < 0.25f)
                return;

            float velocityMagnitude = mainScript.averageVelocity.magnitude;
            if (velocityMagnitude < 2f)
                return;

            Vector3 directionToPlayer = playerComponent.transform.position - mainScript.mainRigidbody.position;
            float angle = Vector3.Angle(Vector3.Normalize(mainScript.averageVelocity * 1000f), Vector3.Normalize(directionToPlayer * 1000f));

            if (angle > 70f)
                return;

            if (angle < 30f && velocityMagnitude > 40f)
            {
                velocityMagnitude += 6f;
            }

            if ((playerComponent.gameplayCamera.transform.position - mainScript.mainRigidbody.position).y < -0.1f)
            {
                velocityMagnitude *= 2f;
            }

            timeSinceHittingPlayer = Time.realtimeSinceStartup;
            Vector3 impactForce = Vector3.ClampMagnitude(mainScript.averageVelocity, 55f);

            if (playerComponent == GameNetworkManager.Instance.localPlayerController)
            {
                if (physicsTransform == GameNetworkManager.Instance.localPlayerController.physicsParent)
                    return;

                if (velocityMagnitude > 20f)
                {
                    GameNetworkManager.Instance.localPlayerController.KillPlayer(impactForce, spawnBody: true, CauseOfDeath.Crushing);
                }
                else
                {
                    int damage = 0;
                    if (velocityMagnitude > 15f) damage = 80;
                    else if (velocityMagnitude > 12f) damage = 60;
                    else if (velocityMagnitude > 8f) damage = 40;

                    if (damage > 0)
                    {
                        GameNetworkManager.Instance.localPlayerController.DamagePlayer(damage, hasDamageSFX: true, callRPC: true, CauseOfDeath.Crushing, 0, fallDamage: false, impactForce);
                    }
                }

                if (!GameNetworkManager.Instance.localPlayerController.isPlayerDead &&
                    GameNetworkManager.Instance.localPlayerController.externalForceAutoFade.sqrMagnitude < mainScript.averageVelocity.sqrMagnitude)
                {
                    GameNetworkManager.Instance.localPlayerController.externalForceAutoFade = mainScript.averageVelocity;
                }
            }
            else if (mainScript.IsOwner && mainScript.averageVelocity.magnitude > 1.8f)
            {
                mainScript.CarReactToObstacle(mainScript.averageVelocity, playerComponent.transform.position, mainScript.averageVelocity, CarObstacleType.Player);
            }
        }
        else
        {
            if (!other.gameObject.CompareTag("Enemy"))
                return;

            EnemyAICollisionDetect enemyAIcollision = other.gameObject.GetComponent<EnemyAICollisionDetect>();
            if (enemyAIcollision == null)
                return;

            if (enemyAIcollision.mainScript == null)
                return;

            if (enemyAIcollision.mainScript.isEnemyDead)
                return;

            // prevent hitting and bouncing off unkillable small entities (e.g., bees, ghost girl, earth leviathan)
            if (!enemyAIcollision.mainScript.enemyType.canDie && enemyAIcollision.mainScript.enemyType.SizeLimit == NavSizeLimit.NoLimit) 
                return;

            // cooldown
            if (Time.realtimeSinceStartup - timeSinceHittingEnemy < 0.25f)
                return;

            // prevent hitting entities inside the truck, does this even work?
            Behaviour? navMeshOwner = enemyAIcollision.mainScript.agent.navMeshOwner as Behaviour;
            if (navMeshOwner != null && navMeshOwner.transform.IsChildOf(mainScript.transform))
                return;

            if (Vector3.Angle(mainScript.averageVelocity, enemyAIcollision.mainScript.transform.position - base.transform.position) > 130f)
                return;

            if (mainScript.backDoorOpen && mainScript.sideDoorOpen && mainScript.averageVelocity.magnitude < 2f && 
                (insideTruckNavMeshBounds.ClosestPoint(enemyAIcollision.mainScript.transform.position) == enemyAIcollision.mainScript.transform.position || 
                insideTruckNavMeshBounds.ClosestPoint(enemyAIcollision.mainScript.agent.destination) == enemyAIcollision.mainScript.agent.destination))
                return;

            bool dealDamage = false;
            for (int i = 0; i < enemiesLastHit.Length; i++)
            {
                if (enemiesLastHit[i] == enemyAIcollision.mainScript)
                {
                    if (Time.realtimeSinceStartup - timeSinceHittingEnemy < 0.6f || mainScript.averageVelocity.magnitude < 4f)
                    {
                        dealDamage = true;
                    }
                }
            }

            timeSinceHittingEnemy = Time.realtimeSinceStartup;
            Vector3 position = enemyAIcollision.transform.position;
            bool enemyDamageByCar = false;

            switch (enemyAIcollision.mainScript.enemyType.EnemySize)
            {
                case EnemySize.Tiny:
                    enemyDamageByCar = mainScript.CarReactToObstacle(mainScript.averageVelocity, position, mainScript.averageVelocity, CarObstacleType.Enemy, 1f, enemyAIcollision.mainScript, dealDamage);
                    break;
                case EnemySize.Giant:
                    enemyDamageByCar = mainScript.CarReactToObstacle(mainScript.averageVelocity, position, mainScript.averageVelocity, CarObstacleType.Enemy, 3f, enemyAIcollision.mainScript, dealDamage);
                    break;
                case EnemySize.Medium:
                    enemyDamageByCar = mainScript.CarReactToObstacle(mainScript.averageVelocity, position, mainScript.averageVelocity, CarObstacleType.Enemy, 2f, enemyAIcollision.mainScript, dealDamage);
                    break;
            }

            if (enemyDamageByCar)
            {
                enemyIndex = (enemyIndex + 1) % 3;
                enemiesLastHit[enemyIndex] = enemyAIcollision.mainScript;
                return;
            }

            for (int j = 0; j < enemiesLastHit.Length; j++)
            {
                if (enemiesLastHit[j] == enemyAIcollision.mainScript)
                {
                    enemiesLastHit[j] = null!;
                }
            }
        }
    }
}