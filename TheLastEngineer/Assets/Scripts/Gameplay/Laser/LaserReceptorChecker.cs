using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LaserReceptorChecker : MonoBehaviour
{
    public UnityEvent OnChecked;
    public UnityEvent OnNotChecked;
    [SerializeField] List<LaserReceptor>  _laserReceptor=new List<LaserReceptor>();
    int contador = 0;
    bool isChecked;
    private void Awake()
    {
        foreach (var item in _laserReceptor)
        {
            //item.OnCompleated+= suma 1 y chequea si llega
            //item.OnEndHit agregar metodo que reste 1
        }
    }
    public void ReceptorCheck()
    {
        StartCoroutine(Checker());
    }
    private IEnumerator Checker()
    {
        yield return new WaitForSeconds(0.125f);
        foreach (var item in _laserReceptor)
        {
            if (!item._isCompleted)
            {
                contador = 0;
                break;
            }
            else
            {
                contador++;
                if (contador >= _laserReceptor.Count)
                {
                    isChecked = true;
                }
            }
            
        }
        if (isChecked)
        {
            OnChecked.Invoke();
        }
        else
        {
            foreach (var item in _laserReceptor)
            {
                item._isCompleted = false;
            }

        }

    }
}
