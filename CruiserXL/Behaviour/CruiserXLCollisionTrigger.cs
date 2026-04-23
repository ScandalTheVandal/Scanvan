using ScanVan.Patches;
using ScanVan.Utils;
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
            other.TryGetComponent<PlayerControllerB>(out var playerController);
            if (playerController == null)
                return;

            if (mainScript.localPlayerInPassengerSeat || mainScript.localPlayerInMiddlePassengerSeat)
            {
                return;
            }

            if (Time.realtimeSinceStartup - timeSinceHittingPlayer < 0.25f)
                return;

            float velocityMagnitude = mainScript.averageVelocity.magnitude;
            if (velocityMagnitude < 2f)
                return;

            Vector3 directionToPlayer = playerController.transform.position - mainScript.mainRigidbody.position;
            float angle = Vector3.Angle(Vector3.Normalize(mainScript.averageVelocity * 1000f), Vector3.Normalize(directionToPlayer * 1000f));

            if (angle > 70f)
                return;

            if (angle < 30f && mainScript.wheelRPM > 400f)
            {
                velocityMagnitude += 6f;
            }

            if ((playerController.gameplayCamera.transform.position - mainScript.mainRigidbody.position).y < -0.1f)
            {
                velocityMagnitude *= 2f;
            }

            timeSinceHittingPlayer = Time.realtimeSinceStartup;
            Vector3 impactForce = Vector3.ClampMagnitude(mainScript.averageVelocity, 40f);

            // prevent hitting players standing on the truck or sitting in the truck
            Transform physicsTransform = mainScript.physicsRegion.physicsTransform;
            if (playerController.physicsParent == physicsTransform || playerController.overridePhysicsParent == physicsTransform)
            {
                return;
            }

            if (playerController == GameNetworkManager.Instance.localPlayerController)
            {
                if (velocityMagnitude > 20f)
                {
                    GameNetworkManager.Instance.localPlayerController.KillPlayer(impactForce, spawnBody: true, CauseOfDeath.Crushing, 9, default, false);
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
                mainScript.CarReactToObstacle(mainScript.averageVelocity, playerController.transform.position, mainScript.averageVelocity, CarObstacleType.Player);
            }
        }
        else
        {
            if (!other.gameObject.CompareTag("Enemy"))
                return;

            if (Time.realtimeSinceStartup - timeSinceHittingEnemy < 0.25f)
                return;

            other.TryGetComponent<EnemyAICollisionDetect>(out var enemyAIcollision);
            if (enemyAIcollision == null)
                return;

            if (enemyAIcollision.mainScript == null)
                return;

            if (enemyAIcollision.mainScript.isEnemyDead)
                return;

            // prevent hitting and bouncing off unkillable entities (e.g., bees, ghost girl)
            if (!enemyAIcollision.mainScript.enemyType.canDie && enemyAIcollision.mainScript.enemyType.SizeLimit == NavSizeLimit.NoLimit)
                return;

            // Prevent hits if the cruiser is not running and it's not an angry dog
            MouthDogAI? dog = enemyAIcollision.mainScript as MouthDogAI;
            bool isAngryDog = dog != null && dog && dog.suspicionLevel > 8;
            if (!isAngryDog && !mainScript.ignitionStarted)
                return;

            // prevent hitting entities inside the truck, does this even work?
            Behaviour? navMeshOwner = enemyAIcollision.mainScript.agent.navMeshOwner as Behaviour;
            if (navMeshOwner != null && navMeshOwner.transform.IsChildOf(mainScript.transform))
                return;

            if (Vector3.Angle(mainScript.averageVelocity, enemyAIcollision.mainScript.transform.position - base.transform.position) > 130f)
                return;

            if ((mainScript.liftGateOpen || mainScript.sideDoorOpen) &&
                (insideTruckNavMeshBounds.ClosestPoint(enemyAIcollision.mainScript.transform.position) == enemyAIcollision.mainScript.transform.position ||
                insideTruckNavMeshBounds.ClosestPoint(enemyAIcollision.mainScript.agent.destination) == enemyAIcollision.mainScript.agent.destination))
            {
                return;
            }

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
