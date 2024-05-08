using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private List<string> m_dialogsList;

    public List<string> DialogsList => m_dialogsList;
}