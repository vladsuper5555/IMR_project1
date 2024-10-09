using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Vuforia;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.GraphicsBuffer;

public class SkeletonManager : MonoBehaviour
{

    public ObserverBehaviour target1; // First trackable object
    public ObserverBehaviour target2; // Second trackable object

    public TextMeshPro target1WinnerText;
    public TextMeshPro target2WinnerText;

    public bool isTarget1Tracked = false;
    public bool isTarget2Tracked = false;

    public GameObject skeleton1;
    public GameObject skeleton2;
    public int skeleton1Health = 700;
    public int skeleton2Health = 700;
    public float[] skeleton1DamageRange = { 5, 25 };
    public float[] skeleton2DamageRange = { 5, 25 };
    public float attackSpeed = 1.0f;

    private Animator skeleton1Animator;
    private Animator skeleton2Animator;
    private bool isAttacking = false;

    void Start()
    {
        // Initialize the animators from the skeletons
        int seed = (int)DateTime.Now.Ticks;
        UnityEngine.Random.InitState(seed);
        skeleton1Animator = skeleton1.GetComponent<Animator>();
        skeleton2Animator = skeleton2.GetComponent<Animator>();
        // initially we want no text to be displayed
        target1WinnerText.text = "";
        target2WinnerText.text = "";

        if (target1 != null)
        {
            target1.OnTargetStatusChanged += OnTarget1StatusChanged;
        }

        if (target2 != null)
        {
            target2.OnTargetStatusChanged += OnTarget2StatusChanged;
        }
    }

    // Update tracking status for target 1
    private void OnTarget1StatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        isTarget1Tracked = targetStatus.Status == Status.TRACKED ||
                           targetStatus.Status == Status.EXTENDED_TRACKED;
    }

    // Update tracking status for target 2
    private void OnTarget2StatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        isTarget2Tracked = targetStatus.Status == Status.TRACKED ||
                           targetStatus.Status == Status.EXTENDED_TRACKED;
    }

    void Update()
    {
        double distance = CalculateDistance();
        if (!isTarget1Tracked || !isTarget2Tracked)
            return;

        if (!double.IsNaN(distance))
        {
            // Always make skeletons face each other
            FaceEachOther();

            if (distance < 0.12f && !isAttacking)
            {
                // Start attacking if they're close enough
                    StartCoroutine(EngageInCombat());
            }
        }
    }

    double CalculateDistance()
    {
        double x1 = skeleton1.transform.position.x;
        double y1 = skeleton1.transform.position.y;
        double z1 = skeleton1.transform.position.z;

        double x2 = skeleton2.transform.position.x;
        double y2 = skeleton2.transform.position.y;
        double z2 = skeleton2.transform.position.z;

        return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) + (z1 - z2) * (z1 - z2));
    }

    void FaceEachOther()
    {
        // Make skeleton1 look at skeleton2
        Vector3 directionToSkeleton2 = (skeleton2.transform.position - skeleton1.transform.position).normalized;
        skeleton1.transform.forward = new Vector3(directionToSkeleton2.x, 0, directionToSkeleton2.z);

        // Make skeleton2 look at skeleton1
        Vector3 directionToSkeleton1 = (skeleton1.transform.position - skeleton2.transform.position).normalized;
        skeleton2.transform.forward = new Vector3(directionToSkeleton1.x, 0, directionToSkeleton1.z);
    }

    IEnumerator EngageInCombat()
    {
        isAttacking = true;

        if (skeleton1Health > 0 && skeleton2Health > 0)
        {
            // Skeletons attack each other
            skeleton1Animator.SetTrigger("Attack");
            skeleton2Animator.SetTrigger("Attack");

            // Random damage calculation
            int skeleton1Damage = UnityEngine.Random.Range(5, 20);
            int skeleton2Damage = UnityEngine.Random.Range(5, 20);
            Debug.Log(skeleton1Damage + " " + skeleton2Damage);
            // Apply damage
            // the one that hits harder gives the damage
            if (skeleton1Damage > skeleton2Damage)
            {
                skeleton2Health -= skeleton1Damage;
                skeleton1Animator.SetTrigger("Hit");
            }
            else
            {
                skeleton1Health -= skeleton2Damage;
                skeleton2Animator.SetTrigger("Hit");
            }
            

            //Debug.Log($"Skeleton1 Health: {skeleton1Health}, Skeleton2 Health: {skeleton2Health}");

            // Check if anyone is dead
            if (skeleton1Health <= 0)
            {
                skeleton1Animator.SetTrigger("Die");
                Debug.Log("Skeleton 1 has died!");

                target2WinnerText.text = "Mafiot";
            }

            if (skeleton2Health <= 0)
            {
                skeleton2Animator.SetTrigger("Die");
                Debug.Log("Skeleton 2 has died!");

                target1WinnerText.text = "Mafiot";
            }

            // Wait for the next attack
            yield return new WaitForSeconds(attackSpeed);
        }

        isAttacking = false;
    }
}
