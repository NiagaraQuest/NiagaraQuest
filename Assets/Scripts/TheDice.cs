using System.Collections;
using TMPro;
using UnityEngine;

public class theDice : MonoBehaviour
{
    // Variables de configuration pour le lancer de d�
    [SerializeField] float torqueMin; // Le minimum de force de rotation du d�
    [SerializeField] float torqueMax; // Le maximum de force de rotation
    [SerializeField] float throwStrength; // La force avec laquelle le d� est lanc�

    private Rigidbody rb; // R�f�rence au composant Rigidbody du d�
    private int rollvalue = 0; // La valeur actuelle du d�
    private bool hasStopped;   // Indique si le d� s'est compl�tement arr�t�

    // Propri�t� publique permettant de v�rifier si le d� a fini de rouler
    public bool HasStopped => hasStopped;

    // M�thode appel�e au d�marrage pour initialiser les variables
    private void Start()
    {
        // R�cup�ration du Rigidbody attach� au d�
        rb = GetComponent<Rigidbody>();
    }

    // M�thode pour lancer le d�
    public void RollTheDice()
    {
        // R�initialisation de l'�tat du d�
        hasStopped = false;
        rollvalue = 0; // R�initialisation de la valeur
       
        // Appliquer une force vers le haut pour lancer le d�
        rb.AddForce(Vector3.up * throwStrength, ForceMode.Impulse);

        // Appliquer une rotation al�atoire pour rendre le lancer r�aliste
        rb.AddTorque(
            transform.forward * Random.Range(torqueMin, torqueMax) +
            transform.up * Random.Range(torqueMin, torqueMax) +
            transform.right * Random.Range(torqueMin, torqueMax)
        );

        // D�marrer la v�rification pour voir quand le d� s'arr�te
        StartCoroutine(WaitForStop());
    }

    // Coroutine pour attendre que le d� s'arr�te compl�tement
    IEnumerator WaitForStop()
    {
        // Attendre un court instant avant de v�rifier l'arr�t
        yield return new WaitForFixedUpdate();

        // Tant que le d� a encore une rotation significative, continuer � attendre
        while (rb.angularVelocity.sqrMagnitude > 0.1f)
        {
            yield return new WaitForFixedUpdate();
        }

        // Indiquer que le d� s'est arr�t�
        hasStopped = true;

        // V�rifier la valeur du d� lorsqu'il s'est arr�t�
        CheckRoll();
    }
    // M�thode pour d�terminer la face visible du d� lorsque celui-ci s'arr�te
    public void CheckRoll()
    {
        // Les dot products permettent de comparer l'orientation du d� � l'axe vertical
        float yDot = Mathf.Round(Vector3.Dot(transform.up.normalized, Vector3.up));
        float zDot = Mathf.Round(Vector3.Dot(transform.forward.normalized, Vector3.up));
        float xDot = Mathf.Round(Vector3.Dot(transform.right.normalized, Vector3.up));

        // V�rification selon l'axe Y (haut/bas)
        switch (yDot)
        {
            case 1:  // Face "haut"
                rollvalue = 2;
                break;
            case -1: // Face "bas"
                rollvalue = 5;
                break;
        }

        // V�rification selon l'axe X (gauche/droite)
        switch (xDot)
        {
            case 1:  // Face "gauche"
                rollvalue = 4;
                break;
            case -1: // Face "droite"
                rollvalue = 3;
                break;
        }

        // V�rification selon l'axe Z (avant/arri�re)
        switch (zDot)
        {
            case 1:  // Face "avant"
                rollvalue = 1;
                break;
            case -1: // Face "arri�re"
                rollvalue = 6;
                break;
        }

       
    }
    // M�thode publique permettant � d'autres scripts de r�cup�rer la valeur actuelle du d�
    public int GetRollValue()
    {
        return rollvalue;
    }


}