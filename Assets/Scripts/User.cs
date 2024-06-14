using System;
using UnityEngine;

[RequireComponent(typeof(Client))]
[RequireComponent(typeof(LocalClient))]
public class User : MonoBehaviour
{
    private string m_userName;
    private string m_usedId;
    private string m_privateKey;
    private string m_publicKey;

    public string UserName => m_userName;
}
