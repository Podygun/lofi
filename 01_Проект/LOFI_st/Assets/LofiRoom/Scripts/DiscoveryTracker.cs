using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LofiRoom
{
    public class DiscoveryTracker : MonoBehaviour
    {
        public Text counterLabel;
        public MessageBanner banner;

        public string format = "Найдено: {0} / {1}";

        public string completionMessage = "Вы обошли всю комнату. Она вся ваша.";

        readonly List<ClickableProp> _all = new List<ClickableProp>();
        readonly HashSet<ClickableProp> _found = new HashSet<ClickableProp>();

        public int Total => _all.Count;
        public int Found => _found.Count;

        public void Register(ClickableProp item)
        {
            if (item == null || _all.Contains(item)) return;
            _all.Add(item);
            Refresh();
        }

        public void MarkDiscovered(ClickableProp item)
        {
            if (item == null) return;
            if (!_all.Contains(item)) _all.Add(item);

            if (_found.Add(item))
            {
                Refresh();
                if (_found.Count == _all.Count && banner != null)
                    banner.Show(completionMessage);
            }
        }

        void Refresh()
        {
            if (counterLabel != null)
                counterLabel.text = string.Format(format, _found.Count, _all.Count);
        }
    }
}
