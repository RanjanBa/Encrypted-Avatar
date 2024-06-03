using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace DigitalMetaverse
{
    public class World : MonoBehaviour
    {
        [SerializeField]
        private string m_worldName = "MetaVerse Meetings";

        private List<Avatar> m_avatars;

        public ReadOnlyCollection<Avatar> Avatars => new ReadOnlyCollection<Avatar>(m_avatars);

        private void Awake()
        {
            m_avatars = new List<Avatar>();
        }

        public void AddAvatar(Avatar _avatar)
        {
            m_avatars ??= new List<Avatar>();

            m_avatars.Add(_avatar);
        }

        public void RemoveAvatar(Avatar _avatar)
        {
            if (m_avatars == null) return;

            m_avatars.Remove(_avatar);
        }
    }
}
