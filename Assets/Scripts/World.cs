using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace DigitalMetaverse
{
    public class World : MonoBehaviour
    {
        [SerializeField]
        private string m_worldName = "MetaVerse Meetings";

        private List<DigitalAvatar> m_avatars;

        public ReadOnlyCollection<DigitalAvatar> Avatars => new ReadOnlyCollection<DigitalAvatar>(m_avatars);

        private void Awake()
        {
            m_avatars = new List<DigitalAvatar>();
        }

        public void AddAvatar(DigitalAvatar _avatar)
        {
            m_avatars ??= new List<DigitalAvatar>();

            m_avatars.Add(_avatar);
        }

        public void RemoveAvatar(DigitalAvatar _avatar)
        {
            if (m_avatars == null) return;

            m_avatars.Remove(_avatar);
        }
    }
}
