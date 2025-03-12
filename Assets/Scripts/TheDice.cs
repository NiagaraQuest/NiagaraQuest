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
        yield return new WaitForSeconds(1.5f);

        // Tant que le d� a encore une rotation significative, continuer � attendre
        while (rb.linearVelocity.sqrMagnitude > 0.01f || rb.angularVelocity.sqrMagnitude > 0.01f)
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
        // D�finir les dot products pour comparer l'orientation du d� avec l'axe vertical
        float yDot = Vector3.Dot(transform.up, Vector3.up);
        float zDot = Vector3.Dot(transform.forward, Vector3.up);
        float xDot = Vector3.Dot(transform.right, Vector3.up);

        // Trouver l'axe le plus align� avec l'axe vertical
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
            rollvalue = (zDot > 0) ? 1 : 6; // Avant (1) / Arri�re (6)
        }

        Debug.Log("Valeur du d� : " + rollvalue);
    }


    // M�thode publique permettant � d'autres scripts de r�cup�rer la valeur actuelle du d�
    public int GetRollValue()
    {
        return rollvalue;
    }


}