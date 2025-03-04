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

    // Propriété publique permettant de vérifier si le dé a fini de rouler
    public bool HasStopped => hasStopped;

    // Méthode appelée au démarrage pour initialiser les variables
    private void Start()
    {
        // Récupération du Rigidbody attaché au dé
        rb = GetComponent<Rigidbody>();
    }

    // Méthode pour lancer le dé
    public void RollTheDice()
    {
        // Réinitialisation de l'état du dé
        hasStopped = false;
        rollvalue = 0; // Réinitialisation de la valeur
       
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
        yield return new WaitForFixedUpdate();

        // Tant que le dé a encore une rotation significative, continuer à attendre
        while (rb.angularVelocity.sqrMagnitude > 0.1f)
        {
            yield return new WaitForFixedUpdate();
        }

        // Indiquer que le dé s'est arrêté
        hasStopped = true;

        // Vérifier la valeur du dé lorsqu'il s'est arrêté
        CheckRoll();
    }
    // Méthode pour déterminer la face visible du dé lorsque celui-ci s'arrête
    public void CheckRoll()
    {
        // Les dot products permettent de comparer l'orientation du dé à l'axe vertical
        float yDot = Mathf.Round(Vector3.Dot(transform.up.normalized, Vector3.up));
        float zDot = Mathf.Round(Vector3.Dot(transform.forward.normalized, Vector3.up));
        float xDot = Mathf.Round(Vector3.Dot(transform.right.normalized, Vector3.up));

        // Vérification selon l'axe Y (haut/bas)
        switch (yDot)
        {
            case 1:  // Face "haut"
                rollvalue = 2;
                break;
            case -1: // Face "bas"
                rollvalue = 5;
                break;
        }

        // Vérification selon l'axe X (gauche/droite)
        switch (xDot)
        {
            case 1:  // Face "gauche"
                rollvalue = 4;
                break;
            case -1: // Face "droite"
                rollvalue = 3;
                break;
        }

        // Vérification selon l'axe Z (avant/arrière)
        switch (zDot)
        {
            case 1:  // Face "avant"
                rollvalue = 1;
                break;
            case -1: // Face "arrière"
                rollvalue = 6;
                break;
        }

       
    }
    // Méthode publique permettant à d'autres scripts de récupérer la valeur actuelle du dé
    public int GetRollValue()
    {
        return rollvalue;
    }


}