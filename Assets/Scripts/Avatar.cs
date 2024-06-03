using UnityEngine;

namespace DigitalMetaverse
{
    public class Avatar : MonoBehaviour
    {
        private byte[] m_privateKey;
        private byte[] m_publicKey;

        public byte[] PublicKey => m_publicKey;
    }
}