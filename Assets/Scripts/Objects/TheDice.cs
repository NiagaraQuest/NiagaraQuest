using System.Collections;
using TMPro;
using UnityEngine;

public class theDice : MonoBehaviour
{
    // Variables de configuration pour le lancer de dé
    [SerializeField] float torqueMin; // Le minimum de force de rotation du dé
    [SerializeField] float torqueMax; // Le maximum de force de rotation
    [SerializeField] float throwStrength; // La force avec laquelle le dé est lancé

    private Rigidbody rb; // Référence au composant Rigidbody du dé
    private int rollvalue = 0; // La valeur actuelle du dé
    private bool hasStopped;   // Indique si le dé s'est complètement arrêté
    private Vector3 initialPosition; // Position initiale du dé

    // Propriété publique permettant de vérifier si le dé a fini de rouler
    public bool HasStopped => hasStopped;
    
    // Variables for landing sound
    private bool isCurrentlyRolling = false; // Is this a current roll in progress
    private bool hasPlayedLandingSound = false; // Has landing sound been played for this roll
    private float landingThreshold = 0.8f; // Velocity threshold to detect a significant landing
    private float lastVelocityMagnitude; // Store previous velocity to detect sudden changes
    private float rollingDelay = 0.3f; // Short delay after roll starts before we detect landing

    // Méthode appelée au démarrage pour initialiser les variables
    private void Start()
    {
        // Récupération du Rigidbody attaché au dé
        rb = GetComponent<Rigidbody>();
        
        // Enregistrement de la position initiale du dé
        initialPosition = transform.position;
        Debug.Log($"Initial dice position saved: {initialPosition}");
    }

    private void FixedUpdate()
    {
        // Only check for landing if we're in a rolling state and haven't played the sound yet
        if (isCurrentlyRolling && !hasPlayedLandingSound && Time.timeSinceLevelLoad > rollingDelay)
        {
            DetectLanding();
        }
        
        // Store current velocity for next frame comparison
        lastVelocityMagnitude = rb.linearVelocity.magnitude;
    }

    // Improved landing detection that focuses on the impact moment
    private void DetectLanding()
    {
        // Detect a significant decrease in velocity (impact with surface)
        float velocityDelta = lastVelocityMagnitude - rb.linearVelocity.magnitude;
        
        // If we detect a sudden drop in velocity after being in motion, that's a landing
        if (velocityDelta > landingThreshold && lastVelocityMagnitude > 1.0f)
        {
            if (DiceSound.Instance != null)
            {
                DiceSound.Instance.PlayDiceLanding();
                hasPlayedLandingSound = true;
                Debug.Log($"Die landed with impact! Delta: {velocityDelta}, Playing sound.");
            }
        }
    }

    // Méthode pour lancer le dé
    public void RollTheDice()
    {
        // Réinitialisation de l'état du dé
        hasStopped = false;
        rollvalue = 0; // Réinitialisation de la valeur

        // Reset landing sound variables
        isCurrentlyRolling = true;
        hasPlayedLandingSound = false;
        lastVelocityMagnitude = 0f;
        rollingDelay = Time.timeSinceLevelLoad + 0.3f; // Add a small delay

        // Appliquer une force vers le haut pour lancer le dé
        rb.AddForce(Vector3.up * throwStrength, ForceMode.Impulse);

        // Appliquer une rotation aléatoire pour rendre le lancer réaliste
        rb.AddTorque(
            transform.forward * Random.Range(torqueMin, torqueMax) +
            transform.up * Random.Range(torqueMin, torqueMax) +
            transform.right * Random.Range(torqueMin, torqueMax)
        );

        // Démarrer la vérification pour voir quand le dé s'arrête
        StartCoroutine(WaitForStop());
    }

    // Coroutine pour attendre que le dé s'arrête complètement
    IEnumerator WaitForStop()
    {
        // Attendre un court instant avant de vérifier l'arrêt
        yield return new WaitForSeconds(1.5f);

        // Tant que le dé a encore une rotation significative, continuer à attendre
        while (rb.linearVelocity.sqrMagnitude > 0.01f || rb.angularVelocity.sqrMagnitude > 0.01f)
        {
            yield return new WaitForFixedUpdate();
        }

        // Indiquer que le dé s'est arrêté
        hasStopped = true;
        isCurrentlyRolling = false;

        // Vérifier la valeur du dé lorsqu'il s'est arrêté
        CheckRoll();
        
        // Reset position but keep rotation
        yield return new WaitForSeconds(0.5f); // Short delay to see the result
        ResetPosition();
    }
    
    // Méthode pour réinitialiser la position du dé tout en conservant sa rotation
    private void ResetPosition()
    {
        // Freeze the rigidbody to prevent further movement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Store current rotation
        Quaternion currentRotation = transform.rotation;
        
        // Reset position
        transform.position = initialPosition;
        
        // Keep current rotation
        transform.rotation = currentRotation;
        
        Debug.Log($"Dice position reset to {initialPosition} while keeping rotation");
    }
    
    // Méthode pour déterminer la face visible du dé lorsque celui-ci s'arrête
    public void CheckRoll()
    {
        // Définir les dot products pour comparer l'orientation du dé avec l'axe vertical
        float yDot = Vector3.Dot(transform.up, Vector3.up);
        float zDot = Vector3.Dot(transform.forward, Vector3.up);
        float xDot = Vector3.Dot(transform.right, Vector3.up);

        // Trouver l'axe le plus aligné avec l'axe vertical
        if (Mathf.Abs(yDot) > Mathf.Abs(xDot) && Mathf.Abs(yDot) > Mathf.Abs(zDot))
        {
            rollvalue = (yDot > 0) ? 2 : 5; // Haut (2) / Bas (5)
        }
        else if (Mathf.Abs(xDot) > Mathf.Abs(yDot) && Mathf.Abs(xDot) > Mathf.Abs(zDot))
        {
            rollvalue = (xDot > 0) ? 4 : 3; // Gauche (4) / Droite (3)
        }
        else
        {
            rollvalue = (zDot > 0) ? 1 : 6; // Avant (1) / Arrière (6)
        }

        Debug.Log("Valeur du dé : " + rollvalue);
    }

    // Méthode publique permettant à d'autres scripts de récupérer la valeur actuelle du dé
    public int GetRollValue()
    {
        return rollvalue;
    }
}